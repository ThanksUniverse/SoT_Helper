using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace SoT_Helper.Models
{
    public class TreasureMap : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.ForestGreen;
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
        public List<XMarkSpot> marks = new List<XMarkSpot>();

        public TreasureMap(MemoryReader memoryReader, int actorId, ulong address, string rawName)
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

            ulong XMarkSpotArray = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.Marks"));
            int XMarkSpotCount = rm.ReadInt((IntPtr)ActorAddress + (int)SDKService.GetOffset("XMarksTheSpotMap.Marks") + 8);
            float XMarkSpotMapRotation = rm.ReadFloat(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.Rotation"));
            MapRotation = XMarkSpotMapRotation;
            UpdateXMarksList(XMarkSpotArray, XMarkSpotCount);

            //ulong XMarkSpotArray = rm.ReadULong(actor_address + (ulong)offsets["XMarksTheSpotMap.Marks"));
            var MapTexturePath = rm.ReadFString(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.MapTexturePath"));

            var islandDataAsset = rm.ReadULong((ulong)SoT_Tool.islandService + (ulong)SDKService.GetOffset("IslandService.IslandDataAsset"));
            //islandDataAsset = 0;
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
                    Coords = new Coordinates() { x = IslandPosition.X, y = IslandPosition.Y, z = IslandPosition.Z };
                    UpdateMarks();
                }
            }
        }

        private void UpdateXMarksList(ulong XMarkSpotArray, int XMarkSpotCount)
        {
            marks = new List<XMarkSpot>();

            for (int i = 0; i < XMarkSpotCount; i++)
            {
                ulong mapspot = XMarkSpotArray + (ulong)i * 16;
                var position = rm.ReadVector2(mapspot + (ulong)SDKService.GetOffset("XMarksTheSpotMapMark.Position"));
                //var rotation = rm.ReadFloat(mapspot + (ulong)SDKService.GetOffset("XMarksTheSpotMapMark.Rotation"));
                //marks.Add(new XMarkSpot() { Position = position, Rotation = rotation });
                marks.Add(new XMarkSpot() { Position = position });
            }
        }

        public void UpdateMarks()
        {
            float cameraOrthoWidth = rm.ReadFloat(captureParams + (ulong)SDKService.GetOffset("WorldMapIslandDataCaptureParams.CameraOrthoWidth"));
            Vector3 worldCamPos = rm.ReadVector3(captureParams + (ulong)SDKService.GetOffset("WorldMapIslandDataCaptureParams.WorldSpaceCameraPosition")); // / new Vector3(100,100,100);
            const float ScaleFactor = 1.041669f;
            float islandScale = cameraOrthoWidth;
            if(SoT_Tool.useTreasureMapScalingFactor)
                islandScale = islandScale / ScaleFactor;
            IslandPosition = worldCamPos;
            Coords = new Coordinates() { x = IslandPosition.X, y = IslandPosition.Y, z = IslandPosition.Z };

            TreasureSpots.Clear();
            foreach (var mark in marks)
            {
                Vector2 v = new Vector2(mark.Position.X*100, mark.Position.Y * 100);
                float rotation = 180 + MapRotation;
                Vector2 vectorRotated = MathHelper.RotatePoint(v, new Vector2(0.5f, 0.5f), rotation, false);
                Vector2 vectorAlligned = new Vector2(vectorRotated.X - 0.5f, vectorRotated.Y - 0.5f);
                Vector2 offsetPos = vectorAlligned * islandScale/100;
                Vector3 digSpot = new Vector3(IslandPosition.X - offsetPos.X, IslandPosition.Y - offsetPos.Y, 0);

                TreasureSpots.Add(digSpot);
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
                if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
                    return;
                //_myCoords = myCoords;
                //Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                Distance = newDistance;

                ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);
                this.ShowText = true;
                this.ShowIcon = true;

                int XMarkSpotCount = rm.ReadInt((IntPtr)ActorAddress + (int)SDKService.GetOffset("XMarksTheSpotMap.Marks") + 8);
                if (XMarkSpotCount != marks.Count)
                {
                    ulong XMarkSpotArray = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("XMarksTheSpotMap.Marks"));
                    UpdateXMarksList(XMarkSpotArray, XMarkSpotCount);
                    UpdateMarks();
                }
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

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
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

                foreach (var spot in TreasureSpots)
                {
                    var spotLevelled = new Vector3() { X = spot.X, Y = spot.Y, Z = SoT_Tool.my_coords.z };

                    float spotDistance = MathHelper.CalculateDistance(spotLevelled, SoT_Tool.my_coords);

                    // make the height of the treasure spot 2m lower than the player
                    spotLevelled = new Vector3() { X = spot.X, Y = spot.Y, Z = SoT_Tool.my_coords.z-2};

                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, spotLevelled);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        // Make sure the spot is not too far away
                        if (spotDistance > 750)
                            continue;
                        string text = $"X - {spotDistance}m";

                        CharmService.DrawOutlinedString(renderer, spotCoords.Value.X,
                        spotCoords.Value.Y,
                        text, Color.BlueViolet, 4);
                    }
                }
            }
        }
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
                return;
            if (ShowText)
            {
                if (this.ScreenCoords != null)
                {
                    this.Text = BuildTextString();

                    renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y, Text, Color, 0);
                }

                foreach (var spot in TreasureSpots)
                {
                    var spotLevelled = new Vector3() { X = spot.X, Y = spot.Y, Z = SoT_Tool.my_coords.z };

                    float spotDistance = MathHelper.CalculateDistance(spotLevelled, SoT_Tool.my_coords);

                    // make the height of the treasure spot 2m lower than the player
                    spotLevelled = new Vector3() { X = spot.X, Y = spot.Y, Z = SoT_Tool.my_coords.z - 2 };

                    var spotCoords = MathHelper.ObjectToScreen(SoT_Tool.my_coords, spotLevelled);
                    if (spotCoords != null && spotCoords.HasValue)
                    {
                        // Make sure the spot is not too far away
                        if (spotDistance > 750)
                            continue;
                        string text = $"X - {spotDistance}m";

                        renderer.DrawOutlinedString(spotCoords.Value.X,
                        spotCoords.Value.Y,
                        text, Color.BlueViolet, 4);
                    }
                }
            }
        }
    }
}
