using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Models
{
    public class Skeleton : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.Orange;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }
        public string MeshName { get; set; }

        public Skeleton(MemoryReader memoryReader, int actorId, ulong address, string rawName)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = actorId;
            ActorAddress = address;
            Rawname= _rawName = rawName;

            actor_root_comp_ptr = GetRootComponentAddress(address);

            // Generate our Actors's info
            if (SoT_DataManager.ActorName_keys.ContainsKey(_rawName))
                Name = SoT_DataManager.ActorName_keys[_rawName];
            else
                Name = _rawName;

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10/2);
            // Used to track if the display object needs to be removed
            //ToDelete = false;
            var meshActor = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("AthenaAICharacter.AssignedMesh"));
            var meshActorId = rm.ReadInt((IntPtr)meshActor + (int)SDKService.GetOffset("Actor.actorId"));
            MeshName = rm.ReadGname(meshActorId);
            SoT_Tool.DumpSkeletonMeshNamesAsJSON();
            if (!SoT_DataManager.SkeletonMeshNames.ContainsKey(MeshName))
            {
                if(MeshName == "nme_skellyancient_01")
                {
                    SoT_DataManager.SkeletonMeshNames.Add(MeshName, "Ancient Skeleton");
                }
                else
                {
                    SoT_DataManager.SkeletonMeshNames.Add(MeshName, "Skeleton");
                }
                    SoT_Tool.DumpSkeletonMeshNamesAsJSON();

            }
            Name = SoT_DataManager.SkeletonMeshNames[MeshName];
            //"AthenaAICharacter.AssignedMesh": 3544,
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            if(!ToDelete)
            try
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    //this.ShowText = false;
                    //this.ShowIcon = false;
                    //ToDelete = true;
                    return;
                }

                //_myCoords = myCoords;
                Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                Distance = newDistance;

                ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);

                if (this.ScreenCoords != null)
                {
                    this.ShowText = true;
                    this.ShowIcon = true;

                    // Update our text to reflect our new distance
                    this.Distance = newDistance;
                    this.Text = BuildTextString();
                }
                else
                {
                    // If it isn't on our screen, set it to invisible to save resources
                    this.ShowText = false;
                    this.ShowIcon = false;
                }
            }
            catch(Exception ex) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;

                ShowIcon = false;
                ShowText = false;
                //ToDelete = true;
            }
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if(ToDelete) { return; }

            if (!bool.Parse(ConfigurationManager.AppSettings["ShowSkeletons"]))
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
                // Text
                CharmService.DrawOutlinedString(renderer,ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }
            if (MeshName == "nme_skellyancient_01")
                renderer.DrawTraceLine(Coords, Color.RoyalBlue);
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ToDelete) { return; }

            if (!bool.Parse(ConfigurationManager.AppSettings["ShowSkeletons"]))
                return;

            if (ShowIcon)
            {
                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);

                //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
                //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            }
            if (ShowText)
            {
                // Text
                renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }
            if (MeshName == "nme_skellyancient_01")
                renderer.Graphics.DrawTraceLine(Coords, Color.RoyalBlue);
        }
    }
}
