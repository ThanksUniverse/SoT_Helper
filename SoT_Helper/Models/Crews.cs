using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SoT_Helper.Services;
using System.Net;
using System.Security.Policy;
using System.Xml.Linq;
using System.Configuration;
using SoT_Helper.Extensions;

namespace SoT_Helper.Models
{
    public class Crews : DisplayObject
    {
        // Define the struct to store data about each crew
        private struct CrewData
        {
            public Guid Guid;
            public int Size;
            public ShipType ShipType;
        }

        public class Crew
        {
            public Guid CrewGuid { get; set; }
            public Dictionary<int, string> CrewMembers { get; set; }
            public Ship Ship { get; set; }
            public ShipType ShipType { get; set; }
            public string ShipName { get; set; }
            public string Emissary { get; set; }
        }

        // Initialize the CrewTracker dictionary to assign each crew a Short-ID
        public static Dictionary<Guid, int> CrewTracker = new Dictionary<Guid, int>();
        public static Dictionary<Guid, Crew> CrewInfoDetails = new Dictionary<Guid, Crew>();
        public static Dictionary<Guid, List<Guid>> Alliances = new Dictionary<Guid, List<Guid>>();
        private static Dictionary<Guid, string> AllianceTexts = new Dictionary<Guid, string>();
        private static Dictionary<Guid, Color> AllianceColors = new Dictionary<Guid, Color>();
        private static DateTime NextUpdateTime;
        private static int updateInterval = 5000;

        private readonly MemoryReader rm;
        private readonly int actorId;
        private readonly IntPtr address;
        private readonly List<CrewData> crewInfo;
        private string crewStr;

        public Crews(MemoryReader memoryReader, int actorId, IntPtr address)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = this.actorId = actorId;
            this.address = address;
            Rawname = "Crews";
            ActorAddress = (ulong)address;
            NextUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);
            // Collect and store information about the crews on the server
            crewInfo = GetCrewsInfo();

            // Sum all of the crew sizes into our totalPlayers variable
            int totalPlayers = 0;
            foreach (CrewData crew in crewInfo)
            {
                totalPlayers += crew.Size;
            }
            Size = 14;
            // All of our actual display information & rendering
            crewStr = BuildTextString();

            if (CrewInfoDetails.Where(x => x.Value.CrewMembers.Where(c => c.Value == SoT_Tool.PlayerName).Any()).Any())
                SoT_Tool.LocalPlayerCrewId = CrewInfoDetails.Where(x => x.Value.CrewMembers.Where(c => c.Value == SoT_Tool.PlayerName).Any()).First().Key;
                //ToList().ForEach(x => CrewInfoDetails.Remove(x.Key));
        }

        public static void Reset()
        {
            CrewTracker.Clear();
            CrewInfoDetails.Clear();
            Alliances.Clear();
            AllianceTexts.Clear();
            //AllianceColors.Clear();
        }

        public override void Update(Coordinates myCoords)
        {
            if (DateTime.Now < NextUpdateTime)
                return;

            NextUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);
            try
            {
                //if (SoT_Tool.captainedSessionService != null)
                //{
                //    var captainedCrewsArray = rm.ReadULong(SoT_Tool.captainedSessionService + (ulong)SDKService.GetOffset("CaptainedSessionService.CaptainedCrews"));
                //    var captainedCrewsCount = rm.ReadInt(SoT_Tool.captainedSessionService + (ulong)SDKService.GetOffset("CaptainedSessionService.CaptainedCrews"] + 8);

                //    for (int i = 0; i < captainedCrewsCount; i++)
                //    {
                //        var captainedCrew = rm.ReadULong(captainedCrewsArray + (ulong)(i * 0xa0)); // 0xa0 = 160
                //        var captainPirateId = rm.ReadFString(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.CaptainPirateId"));
                //        var crewId = rm.ReadGuid(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.CrewId"));
                //        var scrambledShipName = rm.ReadFText(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.ScrambledShipName"));
                //        var sessionToken = rm.ReadFName(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.SessionToken"));
                //        var shipId = rm.ReadGuid(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.ShipId"));
                //        var shipName = rm.ReadFString(captainedCrew + (ulong)SDKService.GetOffset("CaptainedCrew.ShipName"));
                //    }
                //}

                if (SoT_Tool.allianceService != null)
                {
                    var allianceArray = rm.ReadULong(SoT_Tool.allianceService + (ulong)SDKService.GetOffset("AllianceService.Alliances"));
                    var allianceCount = rm.ReadInt(SoT_Tool.allianceService + (ulong)SDKService.GetOffset("AllianceService.Alliances") + 8);

                    List<Guid> allianceIds = new List<Guid>();

                    if(allianceCount > 0) 
                    {
                        for(int i =0; i< allianceCount; i++) 
                        {
                            ulong alliance = allianceArray + 0x28 * (ulong)i; // 0x28 = 40
                            Guid allianceGuid = rm.ReadGuid(alliance + (ulong)SDKService.GetOffset("Alliance.AllianceId"));

                            allianceIds.Add(allianceGuid);

                            var crews = rm.ReadULong(alliance + (ulong)SDKService.GetOffset("Alliance.Crews"));
                            var crewsCount = rm.ReadInt(alliance + (ulong)SDKService.GetOffset("Alliance.Crews")+8);

                            if(!Alliances.ContainsKey(allianceGuid))
                                Alliances.Add(allianceGuid, new List<Guid>());

                            List<Guid> crewIds = new List<Guid>();

                            for (int j = 0; j < crewsCount; j++)
                            {
                                Guid crewGuid = rm.ReadGuid(crews + 0x10 * (ulong)j); // 0x10 = 16
                                crewIds.Add(crewGuid);
                                if (!Alliances[allianceGuid].Contains(crewGuid))
                                    Alliances[allianceGuid].Add(crewGuid);
                            }
                            var crewIdsToRemove = Alliances[allianceGuid].Except(crewIds).ToList();
                            foreach (var crewId in crewIdsToRemove)
                                Alliances[allianceGuid].Remove(crewId);
                        }
                    }
                    var allianceIdsToRemove = Alliances.Keys.Except(allianceIds).ToList();
                    foreach (var allianceId in allianceIdsToRemove)
                        Alliances.Remove(allianceId);
                }

                // A generic method to update all the interesting data about the crews on our server.
                // To be called when seeking to perform an update on the CrewService actor without reinitializing our class.

                // 1. Determine if our actor is what we expect it to be
                if (GetActorId(ActorAddress) != ActorId)
                {
                    ShowText = false;
                    ToDelete = true;
                    return;
                }

                // 2. Pull the latest crew information
                crewInfo.Clear();
                crewInfo.AddRange(GetCrewsInfo());

                // 2.1 Update players crew id
                if(CrewInfoDetails.Any(c => c.Value.CrewMembers.Any(m => m.Value == SoT_Tool.PlayerName)))
                {
                    SoT_Tool.LocalPlayerCrewId = CrewInfoDetails.Where(c => c.Value.CrewMembers.Any(m => m.Value == SoT_Tool.PlayerName))
                        .First().Key;
                }

                // 3. Update our strings accordingly
                crewStr = BuildTextString();
                var x = SoT_Tool.SOT_WINDOW_H / 20; //20;
                var y = SoT_Tool.SOT_WINDOW_W / 20;
                ScreenCoords = new System.Numerics.Vector2(x, y);
                ShowText = true;
            }
            catch(Exception ex) 
            {
                var test1 = Rawname;
                var test2 = actorId;
                var test3 = ToDelete;

                ShowText = false;
                ShowIcon = false;
                ToDelete = true;
            }
        }

        private List<CrewData> GetCrewsInfo()
        {
            // Find the starting address for our Crews TArray
            var crews = (IntPtr)ActorAddress +SDKService.GetOffset("CrewService.Crews");
            byte[] crewsRaw = rm.ReadBytes((IntPtr)ActorAddress + (int)SDKService.GetOffset("CrewService.Crews"), 16);

            // (Crews_Data<Array>, Crews length, Crews max)
            ulong CrewsData = BitConverter.ToUInt64(crewsRaw, 0);
            int CrewsLength = BitConverter.ToInt32(crewsRaw, 8);
            //int CrewsMax = BitConverter.ToInt32(crewsRaw, 12);

            // Will contain all of our condensed Crew Data
            List<CrewData> crewsData = new List<CrewData>();

            // For each crew on the server
            for (int i = 0; i < CrewsLength; i++)
            {
                // Each crew has a unique ID composed of four ints, maybe be useful if you
                // add functionality around Crews on your own
                byte[] crewGuidRaw = rm.ReadBytes((IntPtr)CrewsData + ((int)SDKService.GetOffset("Crew.Size") * i), 16);
                Guid crewGuid = new Guid(crewGuidRaw);

                //var asocActors = rm.ReadULong(CrewsData + (ulong)(SDKService.GetOffset("Crew.Size"] * i + 0x80));
                //var asocActorsCount = rm.ReadUInt((UIntPtr)CrewsData + (SDKService.GetOffset("Crew.Size"] * i + 0x80+8));

                // Read the TArray of Players on the the specific Crew, used to determine
                // Crew size
                byte[] crewRaw = rm.ReadBytes((IntPtr)CrewsData + (int)SDKService.GetOffset("Crew.Players") + ((int)SDKService.GetOffset("Crew.Size") * i), 16);

                // Players<Array>, current length, max length
                (ulong PlayersData, int PlayersLength, int PlayersMax) =
                    ((ulong)BitConverter.ToInt64(crewRaw, 0), BitConverter.ToInt32(crewRaw, 8), BitConverter.ToInt32(crewRaw, 12));

                var crew = CrewsData + (ulong)(SDKService.GetOffset("Crew.Size") * i);
                //var crewSessionTemplate = rm.ReadULong(crew + (ulong)SDKService.GetOffset("Crew.CrewSessionTemplate"));
                var crewSessionTemplate = crew + (ulong)SDKService.GetOffset("Crew.CrewSessionTemplate");
                var maxMatchmakingPlayers = rm.ReadInt(crewSessionTemplate + (ulong)SDKService.GetOffset("CrewSessionTemplate.MaxMatchmakingPlayers"));
  //              "Crew.CrewSessionTemplate": 48,
  //"CrewSessionTemplate.MaxMatchmakingPlayers": 48,

                //CrewData crewData = new CrewData() { Guid = crewGuid, Size = PlayersLength, ShipType = type };

                CrewData crewData = new CrewData() { Guid = crewGuid, Size = PlayersLength};

                crewData.ShipType = ShipType.Ship;
                if(maxMatchmakingPlayers == 2) crewData.ShipType = ShipType.Sloop;
                else if (maxMatchmakingPlayers == 3) crewData.ShipType = ShipType.Brigantine;
                else if (maxMatchmakingPlayers == 4) crewData.ShipType = ShipType.Galleon;

                //Ship ship;
                //if (SoT_DataManager.Ships.Any(s => s.CrewId == crewGuid))
                //{
                //    ship = SoT_DataManager.Ships.First(s => s.CrewId == crewGuid);
                //    crewData.ShipType = ship.ShipType;
                //    if(CrewInfoDetails.ContainsKey(crewGuid))
                //    {
                //        CrewInfoDetails[crewGuid].Ship = ship;
                //    }
                //}
                //else if(CrewInfoDetails.ContainsKey(crewGuid) && CrewInfoDetails[crewGuid].Ship != null)
                //{
                //    crewData.ShipType = CrewInfoDetails[crewGuid].Ship.ShipType;
                //}

                crewsData.Add(crewData);
                if (!CrewTracker.ContainsKey(crewGuid))
                {
                    CrewTracker[crewGuid] = CrewTracker.Count + 1;
                }

                if (!CrewInfoDetails.ContainsKey(crewGuid))
                {
                    CrewInfoDetails.Add(crewGuid, new Crew());
                    CrewInfoDetails[crewGuid].CrewGuid = crewGuid;
                    CrewInfoDetails[crewGuid].CrewMembers = new Dictionary<int, string>();
                    CrewInfoDetails[crewGuid].ShipType = crewData.ShipType;
                }
                else if (CrewInfoDetails[crewGuid].CrewMembers.Count() == PlayersLength && !CrewInfoDetails[crewGuid].CrewMembers.Where(m => m.Value == "").Any())
                {
                    continue;
                }

                // If our crew is >0 people on it, we care about it, so we add it to our tracker
                if (PlayersLength != CrewInfoDetails[crewGuid].CrewMembers.Count())
                {
                    var tempCrew = new Crew() { CrewGuid = crewGuid, CrewMembers = new Dictionary<int, string>() };

                    for (int j = 0; j < PlayersLength; j++)
                    {
                        var playerAddress = rm.ReadULong(PlayersData + (ulong)j * 8);
                        var playerId = rm.ReadInt(playerAddress + (ulong)SDKService.GetOffset("PlayerState.PlayerId"));
                        var name = rm.ReadFString(playerAddress + (ulong)SDKService.GetOffset("PlayerState.PlayerName"));

                        if (name != "NoStringFound")
                        {
                            tempCrew.CrewMembers.Add(playerId, name);
                        }
                        else
                        {
                            tempCrew.CrewMembers.Add(playerId, "");
                        }
                        //var parents = SoT_Tool.GetActorParents(playerAddress);
                        //var rawnames = SoT_Tool.GetActorParentRawnames(playerAddress);
                    }
                    
                    foreach (var member in tempCrew.CrewMembers)
                    {
                        if (!CrewInfoDetails[crewGuid].CrewMembers.ContainsKey(member.Key))
                        {
                            CrewInfoDetails[crewGuid].CrewMembers.Add(member.Key, member.Value);
                            if (SoT_DataManager.DisplayObjects.Any(d => d is Player))
                            {
                                var players = SoT_DataManager.DisplayObjects.Where(d => d is Player).Select(p => (Player)p).ToList();
                                if (players.Any(p => p.Name == member.Value))
                                {
                                    var player = players.First(p => p.Name == member.Value);
                                    player.PlayerCrewId = crewGuid;
                                }
                            }
                        }
                        else if(member.Value != "" && CrewInfoDetails[crewGuid].CrewMembers.ContainsKey(member.Key) && CrewInfoDetails[crewGuid].CrewMembers[member.Key] == "")
                        {
                            CrewInfoDetails[crewGuid].CrewMembers[member.Key] = member.Value;
                            if (SoT_DataManager.DisplayObjects.Any(d => d is Player))
                            {
                                var players = SoT_DataManager.DisplayObjects.Where(d => d is Player).Select(p => (Player)p).ToList();
                                if (players.Any(p => p.Name == member.Value))
                                {
                                    var player = players.First(p => p.Name == member.Value);
                                    player.PlayerCrewId = crewGuid;
                                }
                            }
                        }
                    }
                    var leavers = CrewInfoDetails[crewGuid].CrewMembers.Where(m => !tempCrew.CrewMembers.ContainsKey(m.Key)).Select(m => m.Key).ToList();
                    foreach (var leaver in leavers)
                    {
                        CrewInfoDetails[crewGuid].CrewMembers.Remove(leaver);
                    }
                }
                else if (PlayersLength > 0)
                {
                    for(int j = 0; j<PlayersLength; j++)
                    {
                        var playerAddress = rm.ReadULong(PlayersData + (ulong)j * 8);
                        var playerId = rm.ReadInt(playerAddress + (ulong)SDKService.GetOffset("PlayerState.PlayerId"));
                        if (CrewInfoDetails[crewGuid].CrewMembers.ContainsKey(playerId) && CrewInfoDetails[crewGuid].CrewMembers[playerId] != "")
                            continue;

                        var name = rm.ReadFString(playerAddress + (ulong)SDKService.GetOffset("PlayerState.PlayerName"));
                        if(name != "NoStringFound")
                        {
                            if (!CrewInfoDetails[crewGuid].CrewMembers.ContainsKey(playerId)) // .Where(n => n.Equals(name)).Any())
                                CrewInfoDetails[crewGuid].CrewMembers.Add(playerId, name);
                            else
                                CrewInfoDetails[crewGuid].CrewMembers[playerId] = name;
                            if(SoT_DataManager.DisplayObjects.Any(d => d is Player))
                            {
                                var players = SoT_DataManager.DisplayObjects.Where(d => d is Player).Select(p => (Player)p).ToList();
                                if(players.Any(p => p.Name == name))
                                {
                                    var player = players.First(p => p.Name == name);
                                    player.PlayerCrewId = crewGuid;
                                }
                            }
                        }
                        else if (!CrewInfoDetails[crewGuid].CrewMembers.ContainsKey(playerId))
                        {
                            CrewInfoDetails[crewGuid].CrewMembers.Add(playerId, "");
                        }

                        //var parents = SoT_Tool.GetActorParents(playerAddress);
                        //var rawnames = SoT_Tool.GetActorParentRawnames(playerAddress);
                    }
                }
            }
            //if (SoT_Tool.shipService != null)
            //{

            //    //var PersistentCrewShipDataPtr = rm.ReadULong(SoT_Tool.shipService + (ulong)0x400);
            //    //var PersistentCrewShipDataCount = rm.ReadInt(SoT_Tool.shipService + (ulong)0x400 + 8);
            //    //for (int i = 0; i < PersistentCrewShipDataCount; i++)
            //    //{
            //    //    var PersistentCrewShipData = PersistentCrewShipDataPtr + (ulong)(i * 0x38);
            //    //    var crewGuid = rm.ReadGuid(PersistentCrewShipData + (ulong)0);
            //    //    var CrewShipManifest = rm.ReadULong(PersistentCrewShipData + (ulong)0x10);
            //    //    var PiratesWhoHaveSpottedCrewsShip = rm.ReadULong(PersistentCrewShipData + (ulong)0x18);
            //    //    var PiratesWhoHaveSpottedCrewsShipCount = rm.ReadInt(PersistentCrewShipData + (ulong)0x18 + 8);
            //    //    var CrewsWhoHaveSpottedCrewsShip = rm.ReadULong(PersistentCrewShipData + (ulong)0x28);
            //    //    var CrewsWhoHaveSpottedCrewsShipCount = rm.ReadInt(PersistentCrewShipData + (ulong)0x28 + 8);
            //    //    //SDKService.GetOffset("PersistentCrewShipData.Size"]));
            //    //}
            //    var shipsPtr = rm.ReadULong(SoT_Tool.shipService + (ulong)SDKService.GetOffset("ShipService.ShipList"));
            //    var shipsCount = rm.ReadInt(SoT_Tool.shipService + (ulong)SDKService.GetOffset("ShipService.ShipList"] + 8);

            //    var crewedShipsPtr = rm.ReadULong(SoT_Tool.shipService + (ulong)SDKService.GetOffset("ShipService.CrewedShips"));
            //    var crewedShipsCount = rm.ReadInt(SoT_Tool.shipService + (ulong)SDKService.GetOffset("ShipService.CrewedShips"] + 8);
            //    for (int i = 0; i < crewedShipsCount; i++)
            //    {
            //        //var crewedShip = rm.ReadULong(crewedShipsPtr + (ulong)(i * SDKService.GetOffset("CrewShipEntry.Size"]));
            //        var crewedShip = crewedShipsPtr + (ulong)(i * SDKService.GetOffset("CrewShipEntry.Size"));
            //        var crewedShipPtr = rm.ReadULong(crewedShip + (ulong)SDKService.GetOffset("CrewShipEntry.Ship"));
            //        var crewedShipCrewId = rm.ReadGuid(crewedShip + (ulong)SDKService.GetOffset("CrewShipEntry.CrewId"));
            //        if (crewsData.Any(c => c.Guid == crewedShipCrewId))
            //        {

            //        }
            //        if (crewedShipCrewId != null && crewedShipCrewId != Guid.Empty && CrewInfoDetails[crewedShipCrewId].Ship == null)
            //        {
            //            if (SoT_DataManager.Ships.Any(s => s.ActorAddress == crewedShipPtr))
            //            {
            //                CrewInfoDetails[crewedShipCrewId].Ship = SoT_DataManager.Ships.First(s => s.ActorAddress == crewedShipPtr);
            //                CrewInfoDetails[crewedShipCrewId].Ship.CrewId = crewedShipCrewId;
            //            }
            //            else if (crewedShipPtr > 0)
            //            {
            //                var root = GetRootComponentAddress(crewedShipPtr);
            //                var children = rm.ReadULong(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.AttachChildren"));
            //                int childount = rm.ReadInt(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.AttachChildren"] + 8);
            //                var movedActors = rm.ReadULong(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.MovedActors"));
            //                var movedActorsCount = rm.ReadInt(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.MovedActors"] + 8);
            //                var parent = rm.ReadULong(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.AttachParent"));
            //                var bHiddenInGame = rm.ReadBool(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.bHiddenInGame"], 5);
            //                var bVisible = rm.ReadBool(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.bVisible"], 3);
            //                var componentVelocity = rm.ReadVector3(crewedShipPtr + (ulong)SDKService.GetOffset("SceneComponent.ComponentVelocity"));
            //                int id = GetActorId(crewedShipPtr);
            //                if (id != 0)
            //                {
            //                    string rawname = rm.ReadGname(id);
            //                    if (!string.IsNullOrEmpty(rawname))
            //                    {
            //                        var ship = new Ship(rm, id, crewedShipPtr, SoT_Tool.my_coords, rawname);
            //                        ship.CrewId = crewedShipCrewId;
            //                        SoT_DataManager.Ships.Add(ship);
            //                        CrewInfoDetails[crewedShipCrewId].Ship = ship;
            //                        if(!SoT_DataManager.DisplayObjects.Any(d => d.ActorAddress == ship.ActorAddress))
            //                        {
            //                            SoT_DataManager.DisplayObjects.Add(ship);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            return crewsData;
        }

        protected override string BuildTextString()
        {
            // Generates a string used for rendering. Separate function in the event
            // you need to add more data or want to change formatting
            string output = "";

            List<Guid> crewsInAlliances = new List<Guid>();

            if (Alliances.Any())
            {
                int allianceId = 0;

                foreach (var alliance in Alliances)
                {
                    Color allianceColor = Color.Blue;
                    string allianceText = "";
                    if (allianceId == 0)
                        allianceColor = Color.Green;
                    else if (allianceId == 1)
                        allianceColor = Color.Yellow;
                    else if (allianceId == 2)
                        allianceColor = Color.Blue;
                    else if (allianceId == 3)
                        allianceColor = Color.Orange;

                    allianceText += $"{allianceColor.Name} Alliance\n";

                    foreach (var crewGuid in alliance.Value)
                    {
                        //Guid crewGuid = crewInfo[i].Guid;
                        crewsInAlliances.Add(crewGuid);
                        allianceText += GetCrewText(crewGuid);
                    }
                    //output += $"\\Alliance[{alliance.Key}]\n";

                    if (AllianceTexts.ContainsKey(alliance.Key))
                        AllianceTexts[alliance.Key] = allianceText;
                    else
                        AllianceTexts.Add(alliance.Key, allianceText);
                    if(AllianceColors.ContainsKey(alliance.Key))
                        AllianceColors[alliance.Key] = allianceColor;
                    else
                        AllianceColors.Add(alliance.Key, allianceColor);
                }
            }
            for (int i = 0; i < crewInfo.Count; i++)
            {
                // We store all of the crews in a tracker dictionary. This allows us
                // to assign each crew a "Short"-ID based on count on the server.
                Guid crewGuid = crewInfo[i].Guid;
                if (crewsInAlliances.Contains(crewGuid))
                    continue;
                output += GetCrewText(crewGuid);
            }

            return output;
        }

        private string GetCrewText(Guid crewGuid)
        {
            string output = "";
            int shortId = CrewTracker.GetValueOrDefault(crewGuid);
            if (CrewInfoDetails.TryGetValue(crewGuid, out var crew))
            {
                ShipType shiptype = crewInfo.First(c => c.Guid == crewGuid).ShipType;
                output += $" Crew #{shortId} - {crew.CrewMembers.Count} - {shiptype}\n";

                if (!crew.CrewMembers.Any(m => m.Value.Length > 0))
                {
                    output += "  - "+ string.Join(",",crew.CrewMembers.Keys.Select(n => "["+n.ToString()+"]"));
                    output +="\n";
                }
                else
                foreach (var player in crew.CrewMembers)
                {
                    output += $"  - {player}\n";
                }
            }
            return output;
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowCrews"]))
                return;
            if (ShowText)
            {
                int allianceTextLines = 0;
                foreach(var alliance in Alliances) 
                {
                    if (AllianceTexts.TryGetValue(alliance.Key, out var allianceText))
                    {
                        CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y + allianceTextLines* CharmService.TextSize*1.1f, allianceText, AllianceColors[alliance.Key], 0);

                        allianceTextLines += allianceText.Split('\n').Length;
                    }
                }
                CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y + allianceTextLines * CharmService.TextSize * 1.1f, crewStr, Color.LightBlue, 0);
            }
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowCrews"]))
                return;
            if (ShowText)
            {
                int allianceTextLines = 0;
                foreach (var alliance in Alliances)
                {
                    if (AllianceTexts.TryGetValue(alliance.Key, out var allianceText))
                    {
                        renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y + allianceTextLines * CharmService.TextSize * 1.1f, allianceText, AllianceColors[alliance.Key], 0);

                        allianceTextLines += allianceText.Split('\n').Length;
                    }
                }
                renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y + allianceTextLines * CharmService.TextSize * 1.1f, crewStr, Color.LightBlue, 0);
            }
        }
    }
}
