using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using SoT_Helper.Extensions;

namespace SoT_Helper.Models
{
    public class Actor : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.Yellow;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }

        public Actor(MemoryReader memoryReader, int actorId, ulong address, string rawName)
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
            else if (SoT_Tool.IsPatternMatched(_rawName, SoT_DataManager.ActorName_keys.Keys.ToList()))
            {
                Name = SoT_Tool.GetMatch(rawName, SoT_DataManager.ActorName_keys);
            }
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
        }

        public Actor(MemoryReader memoryReader, ulong address)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = GetActorId(address);
            ActorAddress = address;
            if (ActorId > 0 && ActorId < 500000)
            {
                Rawname = _rawName = rm.ReadGname(ActorId);
            }
            else
                return;

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
            if (ToDelete)
                return;
            try
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    //this.ShowText = false;
                    //this.ShowIcon = false;
                    //ToDelete = true;
                    return;
                }

                if((!bool.Parse(ConfigurationManager.AppSettings["ShowProjectiles"]) && Rawname.ToLower().Contains("bp_projectile")) 
                        ||(bool.Parse(ConfigurationManager.AppSettings["ShowTomes"]) && Rawname.ToLower().Contains("tome"))
                        ||(!Rawname.ToLower().Contains("bp_projectile") && !Rawname.ToLower().Contains("tome") && !bool.Parse(ConfigurationManager.AppSettings["ShowOther"]))
                        )
                {
                    this.ShowText = false;
                    this.ShowIcon = false;
                    //ToDelete = true;
                    return;
                }

                //"Actor.AttachmentReplication": 200,
                //"Actor.bReplicateAttachment": 126,
                //
                /*
                    "RepAttachment.AttachComponent": 56,
                    "RepAttachment.AttachParent": 0,
                    "RepAttachment.LocationOffset": 8,
                    "RepAttachment.RotationOffset": 32,
                    "RepMovement.AngularVelocity": 12,
                    "RepMovement.LinearVelocity": 0,
                    "RepMovement.Rotation": 36,
                */
                if(Parent > 0) 
                {
                    string parentRawname = rm.ReadGname(GetActorId(Parent));
                    Ship ship = SoT_DataManager.Ships.FirstOrDefault(x => x.ActorAddress == Parent);
                    if (ship != null)
                    {
                        ship.Loot.TryAdd(this.ActorAddress, this);
                    }
                    // do not display items in chests
                    if (!parentRawname.ToLower().Contains("ship") && !parentRawname.ToLower().Contains("pirate") 
                        && parentRawname.ToLower().Contains("chest")
                        && !Rawname.ToLower().Contains("chest"))
                    {
                        this.ShowText = false;
                        this.ShowIcon = false;
                        return;
                    }
                }
                var owner = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Actor.Owner"));
                if (owner > 0)
                {
                    string ownerRawname = rm.ReadGname(GetActorId(owner));

                    // do not display items in chests
                    if (ownerRawname.ToLower().Contains("chest") && !Rawname.ToLower().Contains("chest"))
                    {
                        this.ShowText = false;
                        this.ShowIcon = false;
                        return;
                    }
                }
                if (ParentComponent > 0)
                {
                    string parentRawname = rm.ReadGname(GetActorId(ParentComponent));
                    Ship ship = SoT_DataManager.Ships.FirstOrDefault(x => x.ActorAddress == ParentComponent);
                    if (ship != null)
                    {
                        ship.Loot.TryAdd(this.ActorAddress, this);
                    }
                    // do not display items in chests
                    //if (!parentRawname.ToLower().Contains("ship") && !parentRawname.ToLower().Contains("pirate")
                    //    && parentRawname.ToLower().Contains("chest")
                    //    )//&& !Rawname.ToLower().Contains("chest")
                    //{
                    //    this.ShowText = false;
                    //    this.ShowIcon = false;
                    //    return;
                    //}
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

                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y, Size, 1, Color, true);
                //    DrawEllipse(new Pen(Color, 1), new Rectangle(50, 50, 100, 100));
                    //.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y, Size, 1, Color, true);

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
