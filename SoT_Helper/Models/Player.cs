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
using static SoT_Helper.Models.Crews;

namespace SoT_Helper.Models
{
    public class Player : DisplayObject
    {
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public Guid PlayerCrewId { get; set; }
        private FishingBot _fishingRod;

        public Player(MemoryReader memory_reader, int actorId, ulong address, string rawName) : base(memory_reader)
        {
            rm = memory_reader;
            ActorId = actorId;
            ActorAddress = address;
            Rawname = rawName;
            actor_root_comp_ptr = GetRootComponentAddress(address);

            var playerstate = rm.ReadULong((UIntPtr)address + (uint)SDKService.GetOffset("Pawn.PlayerState"));
            Name = rm.ReadFString(playerstate + (ulong)SDKService.GetOffset("PlayerState.PlayerName"));
            if(Name == SoT_Tool.PlayerName)
                SoT_Tool.PlayerAddress = ActorAddress;
            //UpdateHealth();
            //var parents = SoT_Tool.GetActorParents(actor_root_comp_ptr);
            //List<string> parentRawNames = parents.Select(p => rm.ReadGname(GetActorId(p))).ToList();

            // All of our actual display information & rendering
            Color = Color.Red;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            //if (!bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"]))
            //    return;

            // If hide friendly players is enabled, check if the player is in our crew or alliance

            if (bool.Parse(ConfigurationManager.AppSettings["HideFriendlyPlayers"]) && SoT_Tool.LocalPlayerCrewId != Guid.Empty
                && (PlayerCrewId == SoT_Tool.LocalPlayerCrewId
                || Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                        && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId))))
                return;

            if (ShowIcon)
            {
                renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);

                // show health:
                renderer.DrawBox(ScreenCoords.Value.X + Size, ScreenCoords.Value.Y + CharmService.TextSize, 50, 5,
                    1, Color.Black, false);

                Color healthColor = Color.Green;
                if (Health / MaxHealth < 0.25f)
                    healthColor = Color.Red;
                else if (Health / MaxHealth < 0.50f)
                    healthColor = Color.Orange;
                else if (Health / MaxHealth < 0.75f)
                    healthColor = Color.Yellow;
                renderer.DrawBox(ScreenCoords.Value.X + Size, ScreenCoords.Value.Y + CharmService.TextSize, 50 * Health / MaxHealth-1, 5-1,
                    1, healthColor, true);
            }
            if (ShowText)
            {
                CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }
            if(bool.Parse(ConfigurationManager.AppSettings["ShowPlayerTracelines"]) && Distance < 500
                 // Check if player is not in our crew
                 && ActorAddress != SoT_Tool.PlayerAddress
                 && PlayerCrewId != SoT_Tool.LocalPlayerCrewId &&
                // Check if player is not in an alliance with us
                !Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                        && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId))) 
            {
                renderer.DrawTraceLine(Coords, Color.Red);
            }
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["HideFriendlyPlayers"]) && SoT_Tool.LocalPlayerCrewId != Guid.Empty
                && (PlayerCrewId == SoT_Tool.LocalPlayerCrewId
                || Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                        && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId))))
                return;

            if (ShowIcon)
            {
                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);

                // show health:
                renderer.Graphics.DrawBox(ScreenCoords.Value.X + Size, ScreenCoords.Value.Y + CharmService.TextSize, 50, 5,
                    1, Color.Black, false);

                Color healthColor = Color.Green;
                if (Health / MaxHealth < 0.25f)
                    healthColor = Color.Red;
                else if (Health / MaxHealth < 0.50f)
                    healthColor = Color.Orange;
                else if (Health / MaxHealth < 0.75f)
                    healthColor = Color.Yellow;
                renderer.Graphics.DrawBox(ScreenCoords.Value.X + Size, ScreenCoords.Value.Y + CharmService.TextSize, 50 * Health / MaxHealth - 1, 5 - 1,
                    1, healthColor, true);
            }
            if (ShowText)
            {
                renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, Color, 0);
            }
            if (bool.Parse(ConfigurationManager.AppSettings["ShowPlayerTracelines"]) && Distance < 500
                 // Check if player is not in our crew
                 && ActorAddress != SoT_Tool.PlayerAddress
                 && PlayerCrewId != SoT_Tool.LocalPlayerCrewId &&
                // Check if player is not in an alliance with us
                !Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                        && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId)))
            {
                renderer.Graphics.DrawTraceLine(Coords, Color.Red);
            }
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

                if (PlayerCrewId == Guid.Empty)
                {
                    if (Name != "NoStringFound" && Crews.CrewInfoDetails.Any(c => c.Value.CrewMembers.Any(cm => cm.Value == Name)))
                    {
                        var crewId = Crews.CrewInfoDetails.Where(c => c.Value.CrewMembers.Any(cm => cm.Value == Name)).FirstOrDefault().Key;
                        PlayerCrewId = crewId;
                        if (crewId == SoT_Tool.LocalPlayerCrewId)
                            Color = Color.Aquamarine;

                    }
                    if (Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                        && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId)))
                    {
                        if (PlayerCrewId == SoT_Tool.LocalPlayerCrewId)
                            Color = Color.Aquamarine;
                        else
                            Color = Color.LightSteelBlue;
                    }
                }
                else
                {
                    if (PlayerCrewId == SoT_Tool.LocalPlayerCrewId)
                        Color = Color.Aquamarine;
                    else if (Crews.Alliances.Any(a => a.Value.Any(c => c == PlayerCrewId)
                                               && a.Value.Any(c => c == SoT_Tool.LocalPlayerCrewId)))
                        Color = Color.LightSteelBlue;
                    else if (PlayerCrewId != SoT_Tool.LocalPlayerCrewId)
                        Color = Color.Red;
                }

                if (Name == SoT_Tool.PlayerName)
                {
                    SoT_Tool.PlayerAddress = ActorAddress;
                }

                Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                Distance = newDistance;

                if (!bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"]))
                {
                    this.ShowText = false;
                    this.ShowIcon = false;
                    return;
                }

                ScreenCoords = MathHelper.ObjectToScreen(myCoords, this.Coords);

                if (this.ScreenCoords != null)
                {
                    this.ShowText = true;
                    this.ShowIcon = true;

                    UpdateHealth();

                    // Update our text to reflect our new distance
                    this.Distance = newDistance;

                    if(Name == "NoStringFound")
                    {
                        var playerstate = rm.ReadULong((UIntPtr)ActorAddress + (uint)SDKService.GetOffset("Pawn.PlayerState"));
                        Name = rm.ReadFString(playerstate + (ulong)SDKService.GetOffset("PlayerState.PlayerName"));
                        if(Name != "NoStringFound" && Crews.CrewInfoDetails.Any(c => c.Value.CrewMembers.Any(cm => cm.Value == Name)))
                        {
                            var crewId = Crews.CrewInfoDetails.Where(c => c.Value.CrewMembers.Any(cm => cm.Value == Name)).FirstOrDefault().Key;
                            PlayerCrewId = crewId;
                            if (crewId == SoT_Tool.LocalPlayerCrewId)
                                Color = Color.Aquamarine;
                        }
                    }

                    this.Text = BuildTextString();
                }
                else
                {
                    // If it isn't on our screen, set it to invisible to save resources
                    this.ShowText = false;
                    this.ShowIcon = false;
                }
            }
            catch (Exception ex) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;

                this.ShowText = false;
                this.ShowIcon = false;
                //ToDelete = true;
            }
            //BuiltTextString();
        }

        private void UpdateHealth()
        {
            var healthComponent = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("AthenaCharacter.HealthComponent"));
            MaxHealth = rm.ReadFloat(healthComponent + (ulong)SDKService.GetOffset("HealthComponent.MaxHealth"));
            var healthInfo = healthComponent + (ulong)SDKService.GetOffset("HealthComponent.CurrentHealthInfo");
            Health = rm.ReadFloat(healthInfo + (ulong)SDKService.GetOffset("CurrentHealthInfo.Health"));
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }
    }
}
