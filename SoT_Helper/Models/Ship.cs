using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static Charm;
using Renderer = SoT_Helper.Services.Charm.Renderer;
using SoT_Helper.Extensions;
using Newtonsoft.Json.Linq;
using System.Net;

namespace SoT_Helper.Models
{
    public enum Shape
    {
        Circle = 0,
        Box = 1,
        Line = 2,
    }

    public struct Icon
    {
        public Icon(Shape iconShape, float size, Color? iconColor = null, float offset_X = 0, float offset_Y = 0)
        {
            IconShape = iconShape;
            this.size = size;
            Offset_X = offset_X;
            Offset_Y = offset_Y;
            IconColor = iconColor.HasValue ? iconColor.Value : Color.WhiteSmoke;
        }

        public Shape IconShape { get; set; }
        public float size { get; set; }
        //public bool Visible { get; set; }
        public float Offset_X { get; }
        public float Offset_Y { get; }
        public Color IconColor { get; set; }

    }

    public enum ShipType
    {
        Sloop,
        Brigantine,
        Galleon,
        Ship
    }

    public class Ship : DisplayObject
    {
        private static readonly Color SHIP_COLOR = Color.Brown;
        private const int CIRCLE_SIZE = 10;
        private readonly Coordinates _myCoords;
        private readonly string _rawName;
        private Coordinates _coords;
        private Color _color;
        public static Color PlayerShipColor { get; set; } = Color.Aquamarine;

        public Coordinates Coords { get => _coords; set => _coords = value; }

        public static Ship PlayerShip { get; set; }
        public ShipType ShipType { get; set; }

        public Color Color { get => _color; set => _color = value; }
        public Icon Icon { get; set; }
        public Guid CrewId { get; set; }
        public Guid ShipId { get; set; }
        public string ShipName { get; set; }

        private float linearSpeed;
        private float angularSpeed;
        private int direction;
        public int totalTreasureGoldValue;
        public Vector3 LinearVelocity { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public Marker SunkPositionMarker { get; set; }
        public BasicActor ProxyClass { get; set; }
        public Ship NearClass { get; set; }
        private Dictionary<string, int> shipGeneralStorage = new Dictionary<string, int>();

        public Dictionary<ulong, DisplayObject> Loot { get; set; } = new Dictionary<ulong, DisplayObject>();

        public Ship(MemoryReader memoryReader, int actorId, ulong address, Coordinates myCoords, string rawName)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = actorId;
            ActorAddress = address;
            _myCoords = myCoords;
            Rawname = _rawName = rawName;
            SunkPositionMarker = null;

            //"Ship.CrewOwnershipComponent": 1984,
            //"CrewOwnershipComponent.CachedCrewId": 212,
            //"CrewOwnershipComponent.LastKnownCrewId": 228,

            // Get our Ship's CrewId
            ulong CrewOwnershipComponent = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.CrewOwnershipComponent"));
            CrewId = rm.ReadGuid(CrewOwnershipComponent + (ulong)SDKService.GetOffset("CrewOwnershipComponent.CachedCrewId"));

            //ulong CrewShipManifest = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.CrewShipManifest"));
            //CrewId = rm.ReadGuid(CrewShipManifest + (ulong)SDKService.GetOffset("CrewShipManifest.AssociatedCrew"));
            if(CrewId == SoT_Tool.LocalPlayerCrewId)
            {
                PlayerShip = this;
            }
            // Generate our Ship's info
            Name = SoT_DataManager.Ship_keys[_rawName];
            actor_root_comp_ptr = GetRootComponentAddress(address);
            Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
            Distance = MathHelper.CalculateDistance(Coords, _myCoords);
            ScreenCoords = MathHelper.ObjectToScreen(_myCoords, Coords);

            //if(Distance < 1750 && PlayerShip == null)
            //    PlayerShip = this;

            //"Ship.ProxyClass": 4352,
            var proxyAddress = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.ProxyClass"));
            ProxyClass = new BasicActor()
            {
                ActorAddress = proxyAddress,
                ActorId = GetActorId(proxyAddress),
                RawName = rm.ReadGname(ActorId)
            };

            if(SoT_DataManager.Ships.Any(s => s.ActorAddress == proxyAddress))
            {
                SoT_DataManager.Ships.First(s => s.ActorAddress == proxyAddress).NearClass = this;
            }

            // All of our actual display information & rendering
            Color = SHIP_COLOR;
            Text = BuildTextString();
            Size = 5;
            Icon = new Icon(Shape.Circle, 5, Color.Brown, 0, -150);
            DisplayText = new DisplayText(14, Size,-150-14/2);

            ShipType = ShipType.Ship;
            if (Name.ToLower().Contains("sloop"))
                ShipType = ShipType.Sloop;
            if (Name.ToLower().Contains("brig"))
                ShipType = ShipType.Brigantine;
            if (Name.ToLower().Contains("gall"))
                ShipType = ShipType.Galleon;
        }

        protected override string BuildTextString()
        {
            ulong repMovement = ActorAddress + (ulong)SDKService.GetOffset("Actor.ReplicatedMovement");
            Vector3 rotation = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.Rotation"));// * new Vector3(100, 100, 100);

            //Vector3 actorRotation = rm.ReadVector3(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.ActorRotation"));
            //Vector3 actorRelativeRotation = rm.ReadVector3(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.RelativeRotation"));
            //Vector3 actorVelocity = rm.ReadVector3(actor_root_comp_ptr + (ulong)SDKService.GetOffset("SceneComponent.ComponentVelocity")); // / new Vector3(100, 100, 100);
            
            direction = (int)(rotation.Y*100);
            //direction = (int)(actorRotation.Y);

            if ((Distance < 1750 && !bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])) 
                || (Distance < 500 && bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])))
            {
                try
                {
                    LinearVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.LinearVelocity")); // / new Vector3(100, 100, 100);
                    AngularVelocity = rm.ReadVector3(repMovement + (ulong)SDKService.GetOffset("RepMovement.AngularVelocity"));
                    linearSpeed = GetMagnitude(LinearVelocity);
                    //var linearSpeed2 = GetMagnitude(actorVelocity);
                    angularSpeed = GetMagnitude(AngularVelocity);

                    ulong HullDamage = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.HullDamage"));
                    int damagezoneCount = rm.ReadInt((IntPtr)HullDamage + (int)SDKService.GetOffset("HullDamage.ActiveHullDamageZones") + 8);
                    ulong ShipInternalWater = rm.ReadULong(HullDamage + (ulong)SDKService.GetOffset("HullDamage.InternalWater"));
                    float waterAmount = rm.ReadFloat(ShipInternalWater + (ulong)SDKService.GetOffset("ShipInternalWater.WaterAmount"));

                    var MaxWaterAmount = (ulong)SDKService.GetOffset("ShipInternalWaterParams.MaxWaterAmount");
                    var InternalWaterParams = (ulong)SDKService.GetOffset("ShipInternalWater.InternalWaterParams");

                    float maxWaterAmount = rm.ReadFloat(ShipInternalWater + (ulong)SDKService.GetOffset("ShipInternalWater.InternalWaterParams")
                        + (ulong)SDKService.GetOffset("ShipInternalWaterParams.MaxWaterAmount"));

                    if((int)(waterAmount / maxWaterAmount * 100)==100 || damagezoneCount > 4)
                    {
                        if(SunkPositionMarker == null)
                        {
                            SunkPositionMarker = new Marker(rm, "Sunken " + Name, ActorAddress, Rawname, Coords.GetPosition());
                            //SoT_DataManager.DisplayObjects.Add(SunkPositionMarker);
                        }
                    }
                    else
                    {
                        if(SunkPositionMarker != null)
                        {
                            SunkPositionMarker.ToDelete = true;
                            SunkPositionMarker = null;
                        }
                    }

                    //return $"{Name} ({damagezoneCount} holes, {waterAmount} water) - {Distance}m";
                    if(CrewId != Guid.Empty && Crews.CrewTracker.ContainsKey(CrewId))
                    {
                        int id = Crews.CrewTracker[CrewId];
                        return $"C#{id}:{Name} ({damagezoneCount}h,{(int)(waterAmount / maxWaterAmount * 100)}%) - [{Distance}m], {GetDirection((direction + 180) % 360 - 180)} {((direction + 180) % 360)}";
                    }
                    return $"{Name} ({damagezoneCount}h,{(int)(waterAmount / maxWaterAmount * 100)}%) - [{Distance}m], {GetDirection((direction + 180) % 360 - 180)} {((direction + 180) % 360)}";
                }
                catch (Exception ex)
                {
                    SoT_DataManager.InfoLog += $"Ship exception caught: {Name} {Rawname} Distance:{Distance} EX:{ex.Message}";
                }
            }
            if (CrewId != Guid.Empty && Crews.CrewTracker.ContainsKey(CrewId))
            {
                int id = Crews.CrewTracker[CrewId];
                return $"C#{id}:{Name} - [{Distance}m], {GetDirection((direction + 180) % 360 - 180)} {((direction + 180) % 360)}";
            }
            return $"{Name} - [{Distance}m], {GetDirection((direction + 180) % 360 - 180)} {((direction + 180) % 360)}";
        }

        public static string GetDirection(float y)
        {
            if (y >= -22.5f && y < 22.5f)
            {
                return "E";
            }
            else if (y >= 22.5f && y < 67.5f)
            {
                return "SE";
            }
            else if (y >= 67.5f && y < 112.5f)
            {
                return "S";
            }
            else if (y >= 112.5f && y < 157.5f)
            {
                return "SW";
            }
            else if ((y >= 157.5f && y <= 180.0f) || (y >= -180.0f && y < -157.5f))
            {
                return "W";
            }
            else if (y >= -157.5f && y < -112.5f)
            {
                return "NW";
            }
            else if (y >= -112.5f && y < -67.5f)
            {
                return "N";
            }
            else if (y >= -67.5f && y < -22.5f)
            {
                return "NE";
            }
            else
            {
                return "Unknown";
            }
        }

        private static float GetMagnitude(Vector3 velocity)
        {
            return (float)Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2) + Math.Pow(velocity.Z, 2));
        }

        public bool GetShipMigrationBasedOnVelocityEnabled()
        {
            var shipconf = ActorAddress + (ulong)SDKService.GetOffset("Ship.ShipConfigurationSettings");
            var migration = rm.ReadBool(shipconf + (ulong)SDKService.GetOffset("ShipConfigurationSettings.MigrationBasedOnVelocityEnabled"));
            return migration;
        }

        public void FlipMigrationBasedOnVelocityEnabled()
        {
            var shipconf = ActorAddress + (ulong)SDKService.GetOffset("Ship.ShipConfigurationSettings");
            var migration = rm.ReadBool(shipconf + (ulong)SDKService.GetOffset("ShipConfigurationSettings.MigrationBasedOnVelocityEnabled"));
            rm.WriteBool(shipconf + (ulong)SDKService.GetOffset("ShipConfigurationSettings.MigrationBasedOnVelocityEnabled"), !migration);
        }

        string debugText = "";

        public override void Update(Coordinates myCoords)
        {
            if (ToDelete)
                return;

            debugText = "";
            try
            {
                debugText = "Check";
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    if(SunkPositionMarker != null)
                    {
                        if(!SoT_DataManager.DisplayObjects.Contains(SunkPositionMarker))
                            SoT_DataManager.DisplayObjects.Add(SunkPositionMarker);
                    }
                    return;
                }

                debugText = "Coordbuild";
                //_myCoords = myCoords;
                Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                _coords.z += 7;
                float newDistance = MathHelper.CalculateDistance(this.Coords, myCoords);

                // Update our text to reflect our new distance
                this.Distance = newDistance;

                // Ships have two actors dependant on distance. This switches them
                // seamlessly at 1750m
                if ((this.Name.Contains("Near") && newDistance >= 1750) || (!this.Name.Contains("Near") && newDistance < 1750))
                {
                    //this.ToDelete = true;
                    this.ShowText = false;
                    this.ShowIcon = false;
                    //this.ActorAddress = 0;
                    return;
                }

                //ToDelete = false;

                debugText = "ObjectToScreen";
                ScreenCoords = MathHelper.ObjectToScreen(myCoords, Coords);
                this.Text = BuildTextString();

                if(ScreenCoords != null)
                {
                    this.ShowText = true;
                    this.ShowIcon = true;
                }

                if (!this.Name.Contains("Near"))
                    return;

                if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]))
                {
                    List<ulong> toDelete = new List<ulong>();
                    // Remove any loot that has been deleted or is no longer on ship
                    foreach (var item in Loot.Where(l => l.Value.ToDelete || (l.Value.Rawname.Contains("ItemInfo") && l.Value.Parent != ActorAddress)).ToList())
                    {
                        toDelete.Add(item.Key);
                    }
                    foreach (var item in toDelete)
                    {
                        Loot.Remove(item);
                    }

                    List<StorageContainer> containerList = new List<StorageContainer>();

                    foreach (var item in Loot.Where(l => l.Value is not StorageContainer
                    && (l.Value.Rawname.Contains("AnyItemCrate") || l.Value.Rawname.Contains("GhostResourceCrate")
                    || l.Value.Rawname.Contains("CannonballCrate") || l.Value.Rawname.Contains("FirebombCrate")
                    || l.Value.Rawname.Contains("WoodCrate") || l.Value.Rawname.Contains("BananaCrate") // BaitBox
                    )).ToList())
                    {
                        if (item.Value.Rawname.Contains("ItemInfo"))
                        { 
                            //var storageAddress = rm.ReadULong(item.Key + (ulong)SDKService.GetOffset("Actor.Owner"));
                            var storageAddress = rm.ReadULong(item.Key + (ulong)SDKService.GetOffset("Actor.Owner"));

                            if (!Loot.ContainsKey(storageAddress))
                            {
                                var id = GetActorId(storageAddress);
                                var rawname = rm.ReadGname(id);
                                var storage = new StorageContainer(rm, id, storageAddress, rawname);
                                //Loot.Add(storageAddress, storage);
                                containerList.Add(storage);
                                storage.Update(myCoords);
                            }
                        }
                        else
                        {
                            var storage = new StorageContainer(rm, item.Value.ActorId, item.Key, item.Value.Rawname);
                            containerList.Add(storage);
                            //Loot.Remove(item.Key);
                            //Loot.Add(item.Key, storage);
                            storage.Update(myCoords);
                        }
                    }
                    foreach (var item in containerList)
                    {
                        if(Loot.ContainsKey(item.ActorAddress))
                        {
                            Loot.Remove(item.ActorAddress);
                        }
                        Loot.Add(item.ActorAddress, item);
                    }
                    if (Loot.Any(l => l.Value is StorageContainer))
                    {
                        shipGeneralStorage.Clear();
                        var shipstorage = Loot.Where(l => l.Value is StorageContainer && !l.Value.ToDelete).Select(s => (StorageContainer)s.Value).ToList();

                        foreach (var storage in shipstorage)
                        {
                            storage.Update(myCoords);
                            foreach (var item in storage.GetSimpleStorageItems())
                            {
                                if (!shipGeneralStorage.TryAdd(item.Key, item.Value))
                                {
                                    shipGeneralStorage[item.Key] += item.Value;
                                }
                            }
                        }
                    }
                    totalTreasureGoldValue = 0;
                    List<ulong> rewardsRead = new List<ulong>();
                    foreach(var item in Loot)
                    {
                        if(item.Value.Rawname.Contains("ItemInfo"))
                        {
                            if (rewardsRead.Contains(item.Key))
                            {
                                continue;
                            }
                            rewardsRead.Add(item.Key);
                            var rewardName = rm.ReadFName(item.Key + (ulong)SDKService.GetOffset("BootyItemInfo.HandInRewardId"));
                            var value = SoT_Tool.GetRewardGoldValue(item.Key);
                            totalTreasureGoldValue += value;
                        }
                        else if(item.Value.Rawname.Contains("Proxy"))
                        {
                            var itemInfo = rm.ReadULong(item.Key + (ulong)SDKService.GetOffset("ItemProxy.ItemInfo"));
                            if(rewardsRead.Contains(itemInfo))
                            {
                                continue;
                            }
                            rewardsRead.Add(itemInfo);
                            var rewardName = rm.ReadFName(itemInfo + (ulong)SDKService.GetOffset("BootyItemInfo.HandInRewardId"));
                            var value = SoT_Tool.GetRewardGoldValue(itemInfo);
                            totalTreasureGoldValue += value;
                        }
                    }
                }

                // Get Ship's CrewId
                ulong CrewOwnershipComponent = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.CrewOwnershipComponent"));
                CrewId = rm.ReadGuid(CrewOwnershipComponent + (ulong)SDKService.GetOffset("CrewOwnershipComponent.CachedCrewId"));

                //ulong CrewShipManifest = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("Ship.CrewShipManifest"));
                //CrewId = rm.ReadGuid(CrewShipManifest + (ulong)SDKService.GetOffset("CrewShipManifest.AssociatedCrew"));
                // Check if ship is our ship
                if (CrewId != Guid.Empty && CrewId == SoT_Tool.LocalPlayerCrewId)
                {
                    PlayerShip = this;
                    //"Ship.ShipConfigurationSettings": 2080,
                    //"ShipConfigurationSettings.MigrationBasedOnVelocityEnabled": 20,
                    var shipconf = ActorAddress + (ulong)SDKService.GetOffset("Ship.ShipConfigurationSettings");
                    var migration = rm.ReadBool(shipconf + (ulong)SDKService.GetOffset("ShipConfigurationSettings.MigrationBasedOnVelocityEnabled"));
                }
                
                //ToDelete = false;

                debugText = "ObjectToScreen";
                ScreenCoords = MathHelper.ObjectToScreen(myCoords, Coords);
                this.Text = BuildTextString();

                if (this.ScreenCoords != null && !ToDelete)
                {
                    this.ShowText = true;
                    this.ShowIcon = true;

                    //Icon = new Icon(Shape.Circle, 5, Icon.IconColor);
                    //DisplayText = new DisplayText(Color.Brown, 14, Icon.size,0);
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

                SoT_DataManager.InfoLog += $"\nException in Ship.Update on {Rawname} UpdateStep {debugText}: {ex.Message}";

                this.ShowText = false;
                this.ShowIcon = false;
                //this.ToDelete = true;
            }
        }

        public override void DrawGraphics(Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowShips"]))
            {
                return;
            }

            if (ShowIcon && ScreenCoords != null)
            {
                renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Icon.size, 1, Icon.IconColor, true);

                //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
                //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            }
            if (ShowText && ScreenCoords != null)
            {
                Color TextColor = Color;
                if (PlayerShip == this)
                    TextColor = PlayerShipColor;

                CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, TextColor, 0);

                if( (Distance < 1750 && Rawname.Contains("Near") && !bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])) ||
                    (Distance < 500 && bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])))
                {
                    string text2 = $"LinearSpeed: {(int)linearSpeed} AngularSpeed: {(int)(angularSpeed * 100)}";

                    //CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X + DisplayText.Offset_X,
                    //    ScreenCoords.Value.Y + DisplayText.Offset_Y + CharmService.TextSize,
                    //    text2, TextColor, 0);
                    CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X + DisplayText.Offset_X,
                        ScreenCoords.Value.Y + DisplayText.Offset_Y + CharmService.TextSize,
                        text2, TextColor, 0);
                    //if(Loot.Any())
                    //{
                    //    for(int i = 0; i < Loot.Count; i++)
                    //    {
                    //        var loot = Loot.ElementAt(i).Value;
                    //        CharmService.DrawOutlinedString(renderer,
                    //                ScreenCoords.Value.X + DisplayText.Offset_X,
                    //                ScreenCoords.Value.Y + DisplayText.Offset_Y
                    //                + CharmService.TextSize * (i + 2),
                    //                loot.Text, TextColor, 0);
                    //    }
                    //}
                }
            }
            //if (Distance < 500)
            //{
            //    renderer.DrawTraceLine(Coords, Color.Red);
            //}
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowShips"]))
            {
                return;
            }

            if (ShowIcon && ScreenCoords != null)
            {
                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    (int)Icon.size, 1, Icon.IconColor, true);

                //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
                //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            }
            if (ShowText && ScreenCoords != null)
            {
                Color TextColor = Color;
                if (PlayerShip == this)
                    TextColor = PlayerShipColor;

                renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    Text, TextColor, 0);

                if ((Distance < 1750 && Rawname.Contains("Near") && !bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])) ||
                    (Distance < 500 && bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"])))
                {
                    string text2 = $"[{(int)linearSpeed}m/s]"; //AngularSpeed: {(int)(angularSpeed * 100)}";

                    renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                        ScreenCoords.Value.Y + DisplayText.Offset_Y + CharmService.TextSize,
                        text2, TextColor, 0);
                }
            }
        }

        public void DrawShipStatus(PaintEventArgs renderer, float y_offset = 0)
        {
            if ((this.Name.Contains("Near") && Distance >= 1750) || (!this.Name.Contains("Near") && Distance < 1750))
                return;

            if (!CheckRawNameAndActorId(ActorAddress))
            {
                if (this == PlayerShip)
                {
                    PlayerShip = null;
                }
                return;
            }

            if (PlayerShip == this && bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]))
            {
                var x = SoT_Tool.SOT_WINDOW_W / 90; //20;
                var y = SoT_Tool.SOT_WINDOW_H / 3;
                renderer.DrawOutlinedString(x, y,
                    Text, PlayerShipColor, 0);

                if (Distance < 1750)
                {
                    string text2 = $"LinearSpeed: {(int)linearSpeed} AngularSpeed: {(int)(angularSpeed * 100)}";

                    renderer.DrawOutlinedString(x,
                        y + CharmService.TextSize,
                        text2, PlayerShipColor, 0);
                    if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]) && shipGeneralStorage != null && shipGeneralStorage.Any())
                    {
                        Dictionary<string, int> tempStorage = shipGeneralStorage;

                        string lootText = "";
                        for (int i = 0; i < shipGeneralStorage.Count; i++)
                        {
                            var loot = shipGeneralStorage.ElementAt(i);
                            lootText += loot.Key + ":" + loot.Value + " ";
                        }
                        if (totalTreasureGoldValue > 0)
                            lootText += "LootGoldValue = " + totalTreasureGoldValue;

                        renderer.DrawOutlinedString(x,
                            y + CharmService.TextSize * 2,
                            lootText, PlayerShipColor, 0);
                    }
                }
            }
            else if (bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]) && Distance < 1750)
            {
                var x = SoT_Tool.SOT_WINDOW_W / 90; //20;
                var y = SoT_Tool.SOT_WINDOW_H / 2 + y_offset * 3 * CharmService.TextSize;
                renderer.DrawOutlinedString(x, y,
                    Text, Color, 0);

                string text2 = $"LinearSpeed: {(int)linearSpeed} AngularSpeed: {(int)(angularSpeed * 100)}";

                renderer.DrawOutlinedString(x,
                    y + CharmService.TextSize,
                    text2, Color, 0);
                if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]) && shipGeneralStorage.Any())
                {
                    string lootText = "";
                    for (int i = 0; i < shipGeneralStorage.Count; i++)
                    {
                        var loot = shipGeneralStorage.ElementAt(i);
                        lootText += loot.Key + ":" + loot.Value + " ";
                    }
                    if (totalTreasureGoldValue > 0)
                        lootText += "LootGoldValue = " + totalTreasureGoldValue;
                    renderer.DrawOutlinedString(x,
                        y + CharmService.TextSize * 2,
                        lootText, Color, 0);
                }
            }
        }

        public void DrawShipStatus(SoT_Helper.Services.Charm.Renderer renderer, float y_offset = 0)
        {
            if ((this.Name.Contains("Near") && Distance >= 1750) || (!this.Name.Contains("Near") && Distance < 1750))
                return;

            if (!CheckRawNameAndActorId(ActorAddress))
            {
                if(this == PlayerShip)
                {
                    PlayerShip = null;
                }
                return;
            }

            if (PlayerShip == this && bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]))
            {
                var x = SoT_Tool.SOT_WINDOW_W / 90; //20;
                var y = SoT_Tool.SOT_WINDOW_H / 3;
                CharmService.DrawOutlinedString(renderer, x, y,
                    Text, PlayerShipColor, 0);

                if (Distance < 1750)
                {
                    string text2 = $"LinearSpeed: {(int)linearSpeed} AngularSpeed: {(int)(angularSpeed * 100)}";

                    CharmService.DrawOutlinedString(renderer, x,
                        y + CharmService.TextSize,
                        text2, PlayerShipColor, 0);

                    //if (Loot.Any())
                    //{
                    //    for (int i = 0; i < Loot.Count; i++)
                    //    {
                    //        var loot = Loot.ElementAt(i).Value;
                    //        CharmService.DrawOutlinedString(renderer, x,
                    //        y + CharmService.TextSize * (i + 1),
                    //        loot.Text, PlayerShipColor, 0);
                    //    }
                    //}
                    if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]) && shipGeneralStorage != null && shipGeneralStorage.Any())
                    {
                        Dictionary<string, int> tempStorage = shipGeneralStorage;

                        string lootText = "";
                        for (int i = 0; i < shipGeneralStorage.Count; i++)
                        {
                            var loot = shipGeneralStorage.ElementAt(i);
                            lootText += loot.Key + ":" + loot.Value + " ";
                        }
                        if(totalTreasureGoldValue > 0)
                            lootText += "LootGoldValue = " + totalTreasureGoldValue;

                        CharmService.DrawOutlinedString(renderer, x,
                            y + CharmService.TextSize * 2,
                            lootText, PlayerShipColor, 0);
                    }
                }
            }
            else if (bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]) && Distance < 1750)
            {
                var x = SoT_Tool.SOT_WINDOW_W / 90; //20;
                var y = SoT_Tool.SOT_WINDOW_H / 2+ y_offset*3 * CharmService.TextSize;
                CharmService.DrawOutlinedString(renderer, x, y,
                    Text, Color, 0);

                string text2 = $"LinearSpeed: {(int)linearSpeed} AngularSpeed: {(int)(angularSpeed * 100)}";

                CharmService.DrawOutlinedString(renderer, x,
                    y + CharmService.TextSize,
                    text2, Color, 0);
                if (bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]) && shipGeneralStorage.Any())
                {
                    string lootText = "";
                    for (int i = 0; i < shipGeneralStorage.Count; i++)
                    {
                        var loot = shipGeneralStorage.ElementAt(i);
                        lootText += loot.Key + ":" + loot.Value + " ";
                    }
                    if (totalTreasureGoldValue > 0)
                        lootText += "LootGoldValue = " + totalTreasureGoldValue;
                    CharmService.DrawOutlinedString(renderer, x,
                        y + CharmService.TextSize * 2,
                        lootText, Color, 0);
                }
                //if (Loot.Any())
                //{
                //    for (int i = 0; i < Loot.Count; i++)
                //    {
                //        var loot = Loot.ElementAt(i).Value;
                //        CharmService.DrawOutlinedString(renderer, x,
                //        y + CharmService.TextSize * (i + 1),
                //                loot.Text, Color, 0);
                //    }
                //}
            }
        }
    }
}
