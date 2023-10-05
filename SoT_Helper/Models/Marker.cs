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
    public class Marker : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.PaleGoldenrod;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        private bool track = false;

        public Marker(MemoryReader memoryReader, string name, ulong address, string rawName, Vector3 position)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = 0;
            ActorAddress = address;
            Rawname= _rawName = rawName;
            Name = name;
            Coords = new Coordinates() { x = position.X, y = position.Y, z = position.Z };
            //Coords.x = position.X;
            //Coords.Y = position.Y;
            //actor_root_comp_ptr = GetRootComponentAddress(address);

            //// Generate our Actors's info
            //if (SoT_DataManager.ActorName_keys.ContainsKey(_rawName))
            //    Name = SoT_DataManager.ActorName_keys[_rawName];
            //else if (SoT_Tool.IsPatternMatched(_rawName, SoT_DataManager.ActorName_keys.Keys.ToList()))
            //{
            //    Name = SoT_Tool.GetMatch(rawName, SoT_DataManager.ActorName_keys);
            //}
            //else
            //    Name = _rawName;

            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10/2);
            // Used to track if the display object needs to be removed
            //ToDelete = false;
        }

        public Marker(MemoryReader memoryReader, string name, Vector3 position)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = 0;
            Name = name;
            Coords = new Coordinates() { x = position.X, y = position.Y, z = position.Z };
            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
        }

        public Marker(MemoryReader memoryReader, string name, ulong address)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = 0;
            Name = name;
            track = true;
            ActorAddress = address;
            var position = rm.ReadVector3(ActorAddress);

            Coords = new Coordinates() { x = position.X, y = position.Y, z = position.Z };
            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            if (ToDelete)
                return;
            try
            {
                if(track)
                {
                    try
                    {
                        var position = rm.ReadVector3(ActorAddress);

                        Coords = new Coordinates() { x = position.X, y = position.Y, z = position.Z };
                    }
                    catch
                    (Exception ex)
                    {
                        ToDelete = true;
                        return;
                    }
                }

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
                ToDelete = true;
                SoT_DataManager.InfoLog += $"Error updating {Name}: {ex.Message}\n";
            }
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if(ToDelete) { return; }

            if (!bool.Parse(ConfigurationManager.AppSettings["ShowOther"]))
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
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ToDelete) { return; }

            if (!bool.Parse(ConfigurationManager.AppSettings["ShowOther"]))
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
        }
    }
}
