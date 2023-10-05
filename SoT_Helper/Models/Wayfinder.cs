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
    public class Wayfinder : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.ForestGreen;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private static Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        public Vector3 IslandPosition { get; set; }
        public string IslandName { get; set; }
        private long nextUpdate { get; set; }

        public Wayfinder(MemoryReader memoryReader, int actorId, ulong address, string rawName)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = actorId;
            ActorAddress = address;
            Rawname = _rawName = rawName;

            Name = "Wayfinder target";

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;

            
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
            Coords = new Coordinates() { x = 0, y = 0, z = 0 };
            Distance = 0;
            Text = BuildTextString();
            try
            {
                var target = rm.ReadVector3(ActorAddress + (ulong)SDKService.GetOffset("BP_Wayfinder_MultiTargetCompass_Wieldable_C.TargetLocation"));
                if (target.X == 0 && target.Y == 0 && target.Z == 0)
                {
                    ShowIcon = false;
                    ShowText = false;
                }
                else
                    Coords = new Coordinates() { x = target.X, y = target.Y, z = target.Z };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Used to track if the display object needs to be removed
            //ToDelete = false;
            //"BP_Wayfinder_MultiTargetCompass_Wieldable_C.TargetLocation": 2436,

        }

        public void SetActorAddress(ulong address)
        {
            ActorAddress = address;
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
                //if (!CheckRawNameAndActorId(ActorAddress))
                //{
                //    return;
                //}
                if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
                    return;

                if (!Coords.Equals(new Coordinates() { x = 0, y = 0, z = 0 }))
                {
                    Distance = MathHelper.CalculateDistance(this.Coords, myCoords);
                    ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);
                    this.ShowText = true;
                    this.ShowIcon = true;
                    Text = BuildTextString();

                }

                if (nextUpdate > DateTimeOffset.Now.Ticks)
                {
                    return;
                }

                
                //_myCoords = myCoords;
                //Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                var target = rm.ReadVector3(ActorAddress + (ulong)SDKService.GetOffset("BP_Wayfinder_MultiTargetCompass_Wieldable_C.TargetLocation"));
                if (target.X == 0 && target.Y == 0 && target.Z == 0 && !Coords.Equals(new Coordinates() { x = 0, y = 0, z = 0 }))
                    return;
                if (target.X == 0 && target.Y == 0 && target.Z == 0)
                {
                    ShowIcon = false;
                    ShowText = false;
                    nextUpdate = DateTimeOffset.Now.AddSeconds(3).Ticks;
                    return;
                }
                else
                    Coords = new Coordinates() { x = target.X, y = target.Y, z = target.Z };
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                Distance = newDistance;
                Text = BuildTextString();

                ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);
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

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
                return;
            if(ScreenCoords == null)
                return;
            if (ShowIcon)
            {
                renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);

                //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
                //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            }
            if (ShowText)
            {
                this.Text = BuildTextString();

                CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X, ScreenCoords.Value.Y+Size, Text, Color, 0);
            }
        }
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]))
                return;
            if (ScreenCoords == null)
                return;
            if (ShowIcon)
            {
                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);
            }
            if (ShowText)
            {
                this.Text = BuildTextString();

                renderer.DrawOutlinedString(ScreenCoords.Value.X, ScreenCoords.Value.Y + Size, Text, Color, 0);
            }
        }
    }
}
