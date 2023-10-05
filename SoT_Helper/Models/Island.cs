using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace SoT_Helper.Models
{
    public class Island : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.ForestGreen;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        public Vector3 IslandPosition { get; set; }
        public string IslandName { get; set; }
        public float Rotation { get; set; }

        public Vector3 CameraOrientation { get; set; }
        public Vector3 CameraPosition { get; set; }

        public float testRot;
        public ulong captureParams;

        public int TrackingRange { get; set; }
        //public List<Vector3> TreasureSpots = new List<Vector3>();
        //public List<XMarkSpot> marks = new List<XMarkSpot>();

        [JsonConstructor]
        public Island(MemoryReader memoryReader) : base(memoryReader)
        { }

        public Island(MemoryReader memoryReader, ulong islandDataEntry)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorAddress = islandDataEntry;

            var islandNameLoc = rm.ReadFText(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.LocalisedName"));
            var islandName = rm.ReadFName(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.IslandName"));

            var WorldMapData = rm.ReadULong(islandDataEntry + (ulong)SDKService.GetOffset("IslandDataAssetEntry.WorldMapData"));
            captureParams = WorldMapData + (ulong)SDKService.GetOffset("WorldMapIslandDataAsset.CaptureParams");
            Vector3 worldCamPos = rm.ReadVector3(captureParams + (ulong)SDKService.GetOffset("WorldMapIslandDataCaptureParams.WorldSpaceCameraPosition"));

            Vector3 cameraPosition = rm.ReadVector3(captureParams + (ulong)0);
            CameraPosition = cameraPosition;
            Vector3 cameraOrientation = rm.ReadVector3(captureParams + (ulong)0x0c);
            CameraOrientation = cameraOrientation;

            IslandPosition = worldCamPos;
            Coords = new Coordinates() { x = IslandPosition.X, y = IslandPosition.Y, z = IslandPosition.Z };

            Name = IslandName = islandNameLoc;
            //ActorId = actorId;
            Rawname = _rawName = islandName;



            //var min = SoT_Tool.minmemaddress;
            //var max = SoT_Tool.maxmemaddress;

            //List<ulong> parents = new List<ulong>();
            //string parentrawname;

            //parents = SoT_Tool.GetActorOwners(ActorAddress);

            //int parentid = 0;
            //if (parents.Count > 0)
            //{
            //    parentid = GetActorId(parents.First());
            //    parentrawname = rm.ReadGname(parentid);
            //    SoT_Tool.TestLOD(parents.First(), parentrawname);
            //}

            //parents = SoT_Tool.GetActorParentComponents(ActorAddress);
            //foreach(var parent in parents)
            //{
            //    parentid = GetActorId(parent);
            //    parentrawname = rm.ReadGname(parentid);
            //    SoT_Tool.TestLOD(parent, parentrawname);
            //    var rootcompptr = GetRootComponentAddress(parent);
            //    if(rootcompptr != 0)
            //    {
            //        parentid = GetActorId(rootcompptr);
            //        parentrawname = rm.ReadGname(parentid);
            //        SoT_Tool.TestLOD(rootcompptr, parentrawname);
            //    }
            //}


            //int id = GetActorId(islandDataEntry);
            //ActorId = id;
            //var rawname = rm.ReadGname(ActorId);

            //SoT_Tool.TestLOD(ActorAddress, rawname);

            ////int id2 = GetActorId(parents.First());

            //actor_root_comp_ptr = GetRootComponentAddress(ActorAddress);
            //if(actor_root_comp_ptr != 0)
            //{
            //    int id2 = GetActorId(actor_root_comp_ptr);
            //    var rawname2 = rm.ReadGname(id2);
            //    SoT_Tool.TestLOD(actor_root_comp_ptr, rawname2);
            //}

            //// Generate our Actors's info
            //if (SoT_DataManager.ActorName_keys.ContainsKey(_rawName))
            //    Name = SoT_DataManager.ActorName_keys[_rawName];
            //else
            //    Name = _rawName;

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
            // Used to track if the display object needs to be removed
            //ToDelete = false;
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            //if (GetActorId(ActorAddress) != ActorId && CheckRawNameAndActorId(ActorAddress))
            //{
            //    //this.ShowText = false;
            //    //this.ShowIcon = false;
            //    //ToDelete = true;
            //    return;
            //}

            //_myCoords = myCoords;
            //Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
            float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

            Distance = newDistance;

            if(TrackingRange != 0 && Distance > TrackingRange)
            {
                this.ShowText = false;
                this.ShowIcon = false;
                return;
            }

            ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);
            this.ShowText = true;
            this.ShowIcon = true;
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            //if (ShowIcon)
            //{
            //    renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
            //        Size, 1, Color, true);

            //    //    //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
            //    //    //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            //}
            if (ShowText)
            {
                if(this.ScreenCoords != null)
                {
                    this.Text = BuildTextString();

                    CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y, Text, Color, 0);
                }
            }
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ShowText)
            {
                if (this.ScreenCoords != null)
                {
                    this.Text = BuildTextString();

                    renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y, Text, Color, 0);
                }
            }
        }
    }
}
