using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace SoT_Helper.Models
{
    public struct Riddle
    {
        public string IslandName { get; set; }
        public string RiddlePattern { get; set; }
        public List<string> RiddleName { get; set; }
        public List<string> RiddleSubstition { get; set; }
    }

    public class RiddleMap : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.LightGreen;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        public Vector3 IslandPosition { get; set; }
        public string IslandName { get; set; }
        public float MapRotation { get; set; }

        public float testRot = 90;
        public ulong captureParams;
        public List<Vector3> TreasureSpots = new List<Vector3>();
        public List<Riddle> Riddles = new List<Riddle>();
        public Dictionary<string, Coordinates> RiddleLocations = new Dictionary<string, Coordinates>();
        public int RiddleProgress = 0;
        private bool failedToFindLocations = false;
        private long nextRiddleScan = 0;

        public RiddleMap(MemoryReader memoryReader, int actorId, ulong address, string rawName)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = actorId;
            ActorAddress = address;
            Rawname = _rawName = rawName;

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
            // Used to track if the display object needs to be removed
            //ToDelete = false;

            //ulong XMarkSpotArray = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.Marks"));
            //int XMarkSpotCount = rm.ReadInt((IntPtr)ActorAddress + SDKService.GetOffset("XMarksTheSpotMap.Marks"] + 8);
            //float XMarkSpotMapRotation = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.Rotation"));
            //MapRotation = XMarkSpotMapRotation;
            //UpdateXMarksList(XMarkSpotArray, XMarkSpotCount);

            //"RiddleMap.MapInventoryTexturePath": 2160,
            var MapTexturePath = rm.ReadFString(ActorAddress + (ulong)SDKService.GetOffset("RiddleMap.MapInventoryTexturePath"));

            var islandDataAsset = rm.ReadULong((ulong)SoT_Tool.islandService + (ulong)SDKService.GetOffset("IslandService.IslandDataAsset"));
            var islandDataEntriesArray = rm.ReadULong((ulong)islandDataAsset + (ulong)SDKService.GetOffset("IslandDataAsset.IslandDataEntries"));
            var islandDataEntriesCount = rm.ReadInt((IntPtr)islandDataAsset + (int)SDKService.GetOffset("IslandDataAsset.IslandDataEntries") + 8);
            for (int i = 0; i < islandDataEntriesCount; i++)
            {
                var islandDataEntry = rm.ReadULong((UIntPtr)islandDataEntriesArray + (uint)i * 8);

                var islandNameLoc = rm.ReadFText(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.LocalisedName"));
                var islandName = rm.ReadFName(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.IslandName"));
                islandName = islandName.ToLower();
                islandName = islandName.Replace(" ", "_");
                islandName = islandName.Replace("'", "");
                if (MapTexturePath.Contains(islandName))
                {
                    IslandName = islandNameLoc.ToString();
                    Name = IslandName;

                    var WorldMapData = rm.ReadULong(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.WorldMapData"));
                    captureParams = WorldMapData + (ulong)SDKService.GetOffset("WorldMapIslandDataAsset.CaptureParams");
                    Vector3 worldCamPos = rm.ReadVector3(captureParams + (ulong)SDKService.GetOffset("WorldMapIslandDataCaptureParams.WorldSpaceCameraPosition"));
                    IslandPosition = worldCamPos;
                    Coords = new Coordinates() { x = IslandPosition.X, y = IslandPosition.Y, z = 0 };
                }
            }
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
            //return $"{Name} - {Distance}m - {testRot} rotation";
        }

        public override void Update(Coordinates myCoords)
        {
            if (ToDelete)
                return;
            try
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    return;
                }

                if (!bool.Parse(ConfigurationManager.AppSettings["TrackRiddleSpots"]))
                    return;

                //_myCoords = myCoords;
                ScreenCoords = MathHelper.ObjectToScreen(myCoords, Coords);
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                Distance = newDistance;

                if(failedToFindLocations && DateTime.Now.Ticks > nextRiddleScan)
                {
                    failedToFindLocations = false;
                }

                if(!failedToFindLocations && Distance < 500 && (Riddles.Count == 0 || RiddleLocations.Count == 0))
                {
                    GetRiddleSpots();
                    GetRiddleSpotLocations();
                    if (RiddleLocations.Count == 0)
                    {
                        failedToFindLocations = true;
                        nextRiddleScan = DateTime.Now.AddSeconds(30).Ticks;
                    }
                }
                //if (Distance < 500 && Riddles.Count > 0)
                //{
                //    ulong contents = ActorAddress + (ulong)SDKService.GetOffset("RiddleMap.Contents"];
                //    RiddleProgress = rm.ReadInt((IntPtr)contents + SDKService.GetOffset("RiddleMapContents.Progress"));
                //}
            

                this.ShowText = true;
                this.ShowIcon = true;
            }
            catch (Exception e) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;

                this.ShowText = false;
                this.ShowIcon = false;
                ToDelete = true;
            }
}

        private void GetRiddleSpots()
        {
            Riddles.Clear();
            ulong contents = ActorAddress + (ulong)SDKService.GetOffset("RiddleMap.Contents");
            int progress = rm.ReadInt((IntPtr)contents + (int)SDKService.GetOffset("RiddleMapContents.Progress"));
            ulong riddles = rm.ReadULong((ulong)contents + (ulong)SDKService.GetOffset("RiddleMapContents.Text"));
            int riddleCount = rm.ReadInt((IntPtr)contents + (int)SDKService.GetOffset("RiddleMapContents.Text")+8);
            for (int i = 0; i < riddleCount; i++)
            {
                ulong riddlePtr = riddles + (ulong)i * (ulong)SDKService.GetOffset("TreasureMapTextDesc.Size");

                string pattern = rm.ReadFText(riddlePtr + (ulong)SDKService.GetOffset("TreasureMapTextDesc.Pattern"));
                ulong substitutions = rm.ReadULong(riddlePtr + (ulong)SDKService.GetOffset("TreasureMapTextDesc.Substitutions"));
                int subCount = rm.ReadInt((IntPtr)riddlePtr + (int)SDKService.GetOffset("TreasureMapTextDesc.Substitutions") + 8);

                List<string> names = new List<string>();
                List<string> subs = new List<string>();

                for (int j = 0; j < subCount; j++)
                {
                    ulong sub = substitutions + (ulong)j * (ulong)SDKService.GetOffset("TreasureMapTextEntry.Size");
                    string name = rm.ReadFString(sub + (ulong)SDKService.GetOffset("TreasureMapTextEntry.Name"));
                    string substitution = rm.ReadFText(sub + (ulong)SDKService.GetOffset("TreasureMapTextEntry.Substitution"));
                    names.Add(name);
                    subs.Add(substitution);
                }
                Riddle riddle = new Riddle() { IslandName = IslandName, RiddlePattern = pattern, RiddleName = names, RiddleSubstition = subs };
                Riddles.Add(riddle);
            }
        }

        private void GetRiddleSpotLocations()
        {
            int riddleNo = 1;
            for(int i = 1; i< Riddles.Count; i++)
            {
                //string placeName = Riddles[i].RiddleSubstition.Last().Where(c => Char.IsLetter(c)) (" ", "");
                string placeName = new string(Riddles[i].RiddleSubstition.Last().Where(c => Char.IsLetterOrDigit(c)).ToArray());
                //var placeActor = SoT_DataManager.Actors.Any(a => a.Value.RawName == placeName);
                if (SoT_DataManager.Actors.Any(a => new string(a.Value.RawName.Where(c => Char.IsLetterOrDigit(c)).ToArray()).ToLower() == placeName.ToLower()))
                {
                    var actorWithPlaceName = SoT_DataManager.Actors.Select((a) => new KeyValuePair<BasicActor,string>(a.Value, new string(a.Value.RawName.Where(c => Char.IsLetterOrDigit(c)).ToArray()))).ToArray();
                    var adress = actorWithPlaceName.First(a => a.Value.ToLower() == placeName.ToLower()).Key.ActorAddress;
                    var coords = SoT_Tool.GetActorCoords(adress).Result;
                    var name = "Riddle " + riddleNo;
                    riddleNo++;
                    if (i == Riddles.Count-1)
                    {
                        name += ": " +Riddles[i].RiddleSubstition[1] + " " + Riddles[i].RiddleSubstition[0];

                        //RiddleLocations.Add("X", coords);
                    }
                    else
                    {
                        if (Riddles[i].RiddlePattern.ToLower().Contains("lantern") || Riddles[i].RiddlePattern.ToLower().Contains("light")
                            || Riddles[i].RiddlePattern.ToLower().Contains("flame") || Riddles[i].RiddlePattern.ToLower().Contains("beacon"))
                        {
                            name += ": Lantern";
                        }
                        else if (Riddles[i].RiddlePattern.ToLower().Contains("tune") 
                            || Riddles[i].RiddlePattern.ToLower().Contains("music")
                            || Riddles[i].RiddlePattern.ToLower().Contains("instrument")
                            || Riddles[i].RiddlePattern.ToLower().Contains("play")
                            || Riddles[i].RiddlePattern.ToLower().Contains("shanty"))
                        {
                            name += ": Music";
                        }
                        else if (Riddles[i].RiddlePattern.ToLower().Contains("this map") || Riddles[i].RiddlePattern.ToLower().Contains("read"))
                        {
                            name += ": Read Riddlemap";
                        }
                        else
                        {
                            LogService.Log("Failed to match action to riddle pattern: " + Riddles[i].RiddlePattern);
                        }
                    }
                    RiddleLocations.Add(name, coords);
                }
                else
                {
                    LogService.Log("Failed to find actor for riddle location: " + placeName);
                    for(int j = 0; j< Riddles[i].RiddleSubstition.Count;j++)
                    {
                        LogService.Log($"Sub 1: {Riddles[i].RiddleSubstition[j]} Name: {Riddles[i].RiddleName[j]}");
                    }
                }
            }
            
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackRiddleSpots"]))
                return;
            //if (ShowIcon)
            //{
            //    renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
            //        Size, 1, Color, true);

            //    //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
            //    //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            //}
            if (ShowText)
            {
                if(this.ScreenCoords != null)
                {
                    this.Text = BuildTextString();

                    CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y, Text, Color, 0);
                }

                ulong contents = ActorAddress + (ulong)SDKService.GetOffset("RiddleMap.Contents");
                RiddleProgress = rm.ReadInt((IntPtr)contents + (int)SDKService.GetOffset("RiddleMapContents.Progress"));

                foreach (var loc in RiddleLocations)
                {
                    float locDistance = MathHelper.CalculateDistance(loc.Value, SoT_Tool.my_coords);

                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, loc.Value);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        // Make sure the spot is not too far away
                        if (locDistance > 750)
                            continue;
                        string text = $"{loc.Key} - {locDistance}m";

                        Color color = Color.BlueViolet;
                        if (loc.Key.Contains("Riddle " + RiddleProgress.ToString()))
                            color = Color.Pink;

                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X,
                        spotCoords.Value.Y,
                        text, color, 0);
                    }
                }
            }
        }
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackRiddleSpots"]))
                return;
            if (ShowText)
            {
                if (this.ScreenCoords != null)
                {
                    this.Text = BuildTextString();

                    renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y, Text, Color, 0);
                }

                ulong contents = ActorAddress + (ulong)SDKService.GetOffset("RiddleMap.Contents");
                RiddleProgress = rm.ReadInt((IntPtr)contents + (int)SDKService.GetOffset("RiddleMapContents.Progress"));

                foreach (var loc in RiddleLocations)
                {
                    float locDistance = MathHelper.CalculateDistance(loc.Value, SoT_Tool.my_coords);

                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, loc.Value);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        // Make sure the spot is not too far away
                        if (locDistance > 750)
                            continue;
                        string text = $"{loc.Key} - {locDistance}m";

                        Color color = Color.BlueViolet;
                        if (loc.Key.Contains("Riddle " + RiddleProgress.ToString()))
                            color = Color.Pink;

                        renderer.DrawOutlinedString(spotCoords.Value.X,
                        spotCoords.Value.Y,
                        text, color, 0);
                    }
                }
            }
        }

    }
}
