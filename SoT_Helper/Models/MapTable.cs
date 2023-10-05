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
using System.Numerics;
using System.Drawing.Printing;
using Microsoft.VisualBasic.Logging;
using System.Configuration;
using SoT_Helper.Extensions;

namespace SoT_Helper.Models
{
    public struct TrackedShip
    {
        public Guid CrewId { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public byte ReapersMarkLevel { get; set; }
        public byte EmissaryLevel { get; set; }

    }

    public class MapTable : DisplayObject
    {
        private static DateTime NextUpdateTime;
        private static int updateInterval = 2000;

        private readonly MemoryReader rm;
        private readonly int actorId;
        private string crewStr;

        private List<Vector2> MapPins; 
        private List<Vector3> BootyPositions;
        public List<TrackedShip> TrackedShips { get; set; }

        public MapTable(MemoryReader memoryReader, int actorId, ulong address)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = this.actorId = actorId;
            Rawname = rm.ReadGname(ActorId);
            Name = "MapTable";
            ActorAddress = address;
            
            NextUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);
            // Collect and store information about the crews on the server
            MapPins = new List<Vector2>();
            BootyPositions = new List<Vector3>();
            TrackedShips = new List<TrackedShip>();
        }

        private void UpdateMapObjects()
        {
            if (DateTime.Now > NextUpdateTime)
            {
                MapPins.Clear();
                var mapPins = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("MapTable.MapPins"));
                var mapPinsCount = rm.ReadInt(ActorAddress + (ulong)SDKService.GetOffset("MapTable.MapPins.Count"));
                for (int i = 0; i < mapPinsCount; i++)
                {
                    var pin = rm.ReadVector2(mapPins + (ulong)(i * 8)) * 100; // The array is an array of Vector2s so each entry is 8 bytes
                    MapPins.Add(pin);
                }
                NextUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);
            }

            BootyPositions.Clear();
            var trackedBootyItemInfos = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("MapTable.TrackedBootyItemInfos"));
            var trackedBootyItemInfosCount = rm.ReadInt(ActorAddress + (ulong)SDKService.GetOffset("MapTable.TrackedBootyItemInfos.Count"));
            for (int i = 0; i < trackedBootyItemInfosCount; i++)
            {
                var bootyPos = rm.ReadVector3(trackedBootyItemInfos + (ulong)(i * 16)); // The array is an array of Vector3 and 4 bytes of padding so each entry is 16 bytes
                byte bootyType = rm.ReadByte(trackedBootyItemInfos + (ulong)(i * 16) + 12);
                BootyPositions.Add(bootyPos);
            }

            TrackedShips.Clear();
            var trackedShips = rm.ReadULong(ActorAddress + 
                (ulong)SDKService.GetOffset("MapTable.TrackedShips"));
            var trackedShipsCount = rm.ReadInt(ActorAddress + 
                (ulong)SDKService.GetOffset("MapTable.TrackedShips.Count"));
            for (int i = 0; i < trackedShipsCount; i++)
            {
                var trackedShip = trackedShips + 
                    (ulong)(i * SDKService.GetOffset("WorldMapShipLocation.Size"));
                var shipCrewId = rm.ReadGuid(trackedShip +
                    (ulong)SDKService.GetOffset("WorldMapShipLocation.CrewId"));
                var shipPos = rm.ReadVector2(trackedShip + 
                    (ulong)SDKService.GetOffset("WorldMapShipLocation.Location"));
                var shipRotation = rm.ReadFloat(trackedShip +
                    (ulong)SDKService.GetOffset("WorldMapShipLocation.Rotation"));
                var ReapersMarkLevel = rm.ReadByte(trackedShip +
                    (ulong)SDKService.GetOffset("WorldMapShipLocation.ReapersMarkLevel"));
                var EmissaryLevel = rm.ReadByte(trackedShip +
                    (ulong)SDKService.GetOffset("WorldMapShipLocation.EmissaryLevel"));
                TrackedShips.Add(new TrackedShip() { CrewId = shipCrewId, Position = shipPos, Rotation = shipRotation, ReapersMarkLevel = ReapersMarkLevel, EmissaryLevel = EmissaryLevel });
                //byte shipType = rm.ReadByte(trackedShips + (ulong)(i * 64) + 12);
                //BootyPositions.Add(shipPos);
            }
        }

        public override void Update(Coordinates myCoords)
        {
            if(ToDelete || !CheckRawNameAndActorId(ActorAddress))
            {
                return;
            }

            if ( ToDelete) //DateTime.Now < NextUpdateTime ||
                return;
            try
            {
                //NextUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);

                UpdateMapObjects();
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

        
        protected override string BuildTextString()
        {
            // Generates a string used for rendering. Separate function in the event
            // you need to add more data or want to change formatting
            string output = "";

            return output;
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            //if (ShowIcon)
            //{
            //    renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
            //        Size, 1, Color, true);

            //    //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
            //    //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            //}
            if (ShowText)
            {
                int i = 0;
                if(bool.Parse(ConfigurationManager.AppSettings["ShowMapPins"]))
                foreach (var pin in MapPins)
                {
                    Vector3 pos = new Vector3() { X = pin.X, Y = pin.Y, Z = 150 + i * 50 };
                    float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X, spotCoords.Value.Y, $"MapPin [{distance}m]", Color.BurlyWood, -2);
                    }
                    i++;
                }

                if (bool.Parse(ConfigurationManager.AppSettings["ShowMapLoot"]))
                foreach (var booty in BootyPositions)
                {
                    Vector3 pos = new Vector3() { X = booty.X, Y = booty.Y, Z = booty.Z };
                    float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X, spotCoords.Value.Y, $"Booty [{distance}m]", Color.Gold, 0);
                    }
                }
                i = 0;
                if (bool.Parse(ConfigurationManager.AppSettings["ShowMapShips"]))
                foreach (var ship in TrackedShips)
                {
                    Vector3 pos = new Vector3() { X = ship.Position.X, Y = ship.Position.Y, Z = 100+i*200 };
                    float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        string text = $"MapShip";
                        if(Crews.CrewTracker.ContainsKey(ship.CrewId))
                        {
                            var crewNo = Crews.CrewTracker[ship.CrewId];
                            
                            var shiptype = Crews.CrewInfoDetails.First(c => c.Key == ship.CrewId).Value.ShipType;

                            text = $"{shiptype} Crew#{crewNo}";
                        }
                        if(ship.EmissaryLevel > 0)
                            text += $" L{ship.EmissaryLevel}";
                        text += $" [{distance}m]";
                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X, spotCoords.Value.Y, text, Color.PaleVioletRed, -2);
                    }
                    i++;
                }
            }
        }
        //PaintEventArgs
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ShowText)
            {
                int i = 0;
                if (bool.Parse(ConfigurationManager.AppSettings["ShowMapPins"]))
                    foreach (var pin in MapPins)
                    {
                        Vector3 pos = new Vector3() { X = pin.X, Y = pin.Y, Z = 150 + i * 50 };
                        float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                        var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                        if (spotCoords != null && spotCoords.HasValue)
                        {
                            renderer.DrawOutlinedString(spotCoords.Value.X, spotCoords.Value.Y, $"MapPin [{distance}m]", Color.BurlyWood, -2);
                        }
                        i++;
                    }

                if (bool.Parse(ConfigurationManager.AppSettings["ShowMapLoot"]))
                    foreach (var booty in BootyPositions)
                    {
                        Vector3 pos = new Vector3() { X = booty.X, Y = booty.Y, Z = booty.Z };
                        float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                        var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                        if (spotCoords != null && spotCoords.HasValue)
                        {
                            renderer.DrawOutlinedString(spotCoords.Value.X, spotCoords.Value.Y, $"Booty [{distance}m]", Color.Gold, 0);
                        }
                    }
                i = 0;
                if (bool.Parse(ConfigurationManager.AppSettings["ShowMapShips"]))
                    foreach (var ship in TrackedShips)
                    {
                        Vector3 pos = new Vector3() { X = ship.Position.X, Y = ship.Position.Y, Z = 100 + i * 200 };
                        float distance = MathHelper.CalculateDistance(pos, SoT_Tool.my_coords);
                        var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, pos);
                        if (spotCoords != null && spotCoords.HasValue)
                        {
                            string text = $"MapShip";
                            if (Crews.CrewTracker.ContainsKey(ship.CrewId))
                            {
                                var crewNo = Crews.CrewTracker[ship.CrewId];

                                var shiptype = Crews.CrewInfoDetails.First(c => c.Key == ship.CrewId).Value.ShipType;

                                text = $"{shiptype} Crew#{crewNo}";
                            }
                            if (ship.EmissaryLevel > 0)
                                text += $" L{ship.EmissaryLevel}";
                            text += $" [{distance}m]";
                            renderer.DrawOutlinedString(spotCoords.Value.X, spotCoords.Value.Y, text, Color.PaleVioletRed, -2);
                        }
                        i++;
                    }
            }
        }
    }
}
