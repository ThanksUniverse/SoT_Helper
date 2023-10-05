using Newtonsoft.Json;
using SoT_Helper.Extensions;
using SoT_Helper.Models.SDKClasses;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Models
{
    public class StorageContainer : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.White;
        private const int CIRCLE_SIZE = 10;
        private readonly string _rawName;
        private Coordinates _coords;
        private long nextUpdate = 0;

        public Coordinates Coords { get => _coords; set => _coords = value; }
        public ulong ContainerNodesPtr { get; set; }
        public ulong StorageContainerComponent { get; set; }
        public int ItemCount { get; set; }
        public Dictionary<string, int> Items { get; set; }
        public Dictionary<string, int> ItemsRaw { get; set; }

        private static List<int> _storageOffsets { get; set; } = new List<int>();

    public StorageContainer(MemoryReader memoryReader, int actorId, ulong address, string rawName)
            : base(memoryReader)
        {
            try
            {
                rm = memoryReader;
                ActorId = actorId;
                ActorAddress = address;
                Rawname = _rawName = rawName;

                actor_root_comp_ptr = GetRootComponentAddress(address);

                Color = ACTOR_COLOR;

                // Generate our StorageContainer's info
                if (SoT_Tool.GetMatch(_rawName, SoT_DataManager.ActorName_keys) != "")
                {
                    Name = SoT_Tool.GetMatch(_rawName, SoT_DataManager.ActorName_keys);
                    Color = Color.YellowGreen;
                }
                else if (SoT_DataManager.ActorName_keys.ContainsKey(_rawName))
                    Name = SoT_DataManager.ActorName_keys[_rawName];
                else if (_rawName.ToLower().Contains("box"))
                    Name = "Box";
                else if (_rawName.ToLower().Contains("crate"))
                    Name = "Crate";
                else
                    Name = "Barrel";

                //Name = _rawName;

                // All of our actual display information & rendering
                Text = BuildTextString();
                Size = 5;
                DisplayText = new DisplayText(10, 0, -10 / 2);
                Items = new Dictionary<string, int>();

                int storagecontaineroffset = 0;
                //if (Rawname == "BP_MerchantCrate_AnyItemCrate_Proxy_C")
                //{
                //    //ActorAddress = rm.ReadULong(address + (ulong)SDKService.GetOffset("ItemInfo.ProxyType"]);
                //    //Rawname = rm.ReadRawname(ActorAddress);
                //    storagecontaineroffset = SDKService.GetOffset("BP_MerchantCrate_AnyItemCrate_Proxy_C.StorageContainer"];
                //    SoT_DataManager.RawName_StorageOffset_map.TryAdd(_rawName, storagecontaineroffset);
                //}
                //else
                //if (!SoT_DataManager.RawName_StorageOffset_map.TryGetValue(_rawName, out storagecontaineroffset))
                {
                    storagecontaineroffset = GetStorageContainerOffset(address);
                    rm.ThrowExceptions = false;

                    if (storagecontaineroffset > 0)
                    {
                        SoT_DataManager.RawName_StorageOffset_map.TryAdd(_rawName, storagecontaineroffset);

                        string fileName = "StorageOffsetMap.json";
                    }
                    else
                    {
                        SoT_DataManager.RawName_StorageOffset_map.TryGetValue(_rawName, out storagecontaineroffset);
                    }
                }
                rm.ThrowExceptions = false;

                if (storagecontaineroffset > 0)
                {
                    CheckRawNameAndActorId(ActorAddress);
                    StorageContainerComponent = rm.ReadULong(address + (ulong)storagecontaineroffset);
                    ContainerNodesPtr = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes") + (ulong)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes");
                    UpdateStorage();
                }
                // Used to track if the display object needs to be removed
                //ToDelete = false;
            }
            catch (Exception ex)
            {
                SoT_DataManager.InfoLog += "StorageContainer: "+rawName+" EX:" + ex.Message + "\n";
            }
        }

        private static void UpdateStorageOffsetList()
        {
            if(_storageOffsets.Count > 0)
                return;
            //_storageOffsets = new List<int>(); // { 0x4d8, 0x4e0, 0x740, 0x688, 0x818 }; 0x4d8 = 1232, 0x4e0 = 1248, 0x740 = 1856, 0x688 = 1672, 0x818 = 2088
            _storageOffsets = SoT_Tool.offsets.Where(o => o.Key.EndsWith("StorageContainer") || o.Key.EndsWith("StorageContainerComponent")).Select(o => o.Value).ToList();
            //_storageOffsets.Add(SDKService.GetOffset("BP_IslandStorageBarrel_C.StorageContainer"]); //1240
            //_storageOffsets.Add(SDKService.GetOffset("BP_BuoyantStorageBarrel_LockedToWater_CursedSails_C.StorageContainer"]); //1864
            //_storageOffsets.Add(SDKService.GetOffset("BP_ShipStorageBarrel_Food_C.StorageContainer"]); //1248
            //_storageOffsets.Add(SDKService.GetOffset("BP_ShipStorageBarrel_Wood_C.StorageContainer"]); //1248
            //_storageOffsets.Add(SDKService.GetOffset("BP_MA_LS_BuoyantStorageBarrel_LockedToWater_C.StorageContainer"]); //1856
            //_storageOffsets.Add(SDKService.GetOffset("MerchantCrateFilledItemProxy.StorageContainerComponent"]); //2248
            //_storageOffsets.Add(SDKService.GetOffset("BP_MerchantCrate_AnyItemCrate_Proxy_C.StorageContainer"]); //2192
            //// BP_MerchantCrate_AnyItemCrate_Proxy_C.StorageContainer
            //_storageOffsets.Add(SDKService.GetOffset("StorageSeat.StorageContainerComponent"]); //1672
            _storageOffsets = _storageOffsets.Distinct().ToList();
        }

        private int GetStorageContainerOffset(ulong address)
        {
            if(Rawname.ToLower().Contains("iteminfo"))
            {
                ulong proxytype = rm.ReadULong(address + (ulong)SDKService.GetOffset("ItemInfo.ProxyType"));
                var proxyname = rm.ReadRawname(proxytype);
                address = proxytype;

                var offset = SDKService.GetOffset(proxyname + ".StorageContainer");
                if(offset == 0)
                {
                    offset = SDKService.GetOffset(proxyname + ".StorageContainerComponent");
                }
                StorageContainerComponent = rm.ReadULong(address + (ulong)offset);
                if(StorageContainerComponent > 0)
                {
                    ContainerNodesPtr = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes") + (ulong)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes");
                    UpdateStorage();

                }
                return 0;
            }
            UpdateStorageOffsetList();
            //storageOffsets.Add(SDKService.GetOffset("StorageSeat.StorageContainerComponent"]);

            Dictionary<int, string> offsetCheck = new Dictionary<int, string>();

            rm.ThrowExceptions = true;

            foreach (int offset in _storageOffsets)
            {
                offsetCheck.Add(offset, "Unknown");
                int numberOfItems = 0;
                try
                {
                    var StorageContainerComponent = rm.ReadULong(address + (ulong)offset);

                    if (StorageContainerComponent > SoT_Tool.maxmemaddress)
                    {
                        offsetCheck[offset] = "Failed1";
                        continue;
                    }

                    var ShowCapacityInDescription = rm.ReadBool(StorageContainerComponent + 0x308);
                    var backingstore = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes");
                    var containerNodes = rm.ReadULong((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes"));
                    if (containerNodes > SoT_Tool.maxmemaddress)
                    {
                        offsetCheck[offset] = "Failed2";
                        continue;
                    }
                    var containerNodesCount = rm.ReadUInt((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes") + 8);
                    if (containerNodesCount >= 0 && containerNodesCount < 100)
                        for (int i = 0; i < containerNodesCount; i++)
                        {
                            var NumItems = rm.ReadInt((IntPtr)(containerNodes + (ulong)i * 16 + 8)); //0x10 is 16 and it is the size of the FStorageContainerNode class

                            if (NumItems < 0 || NumItems > 100)
                            {
                                offsetCheck[offset] = "Failed3";
                                continue;
                            }

                            var itemDesc = rm.ReadULong((UIntPtr)(containerNodes + (ulong)i * 16));
                            int id = rm.ReadInt((IntPtr)itemDesc + 24);
                            string itemRawName = "";
                            SoT_DataManager.Actor_name_map.TryGetValue(id, out itemRawName);
                            if (string.IsNullOrWhiteSpace(itemRawName))
                                itemRawName = rm.ReadGname(id);

                            if (!string.IsNullOrWhiteSpace(itemRawName))
                            {
                                SoT_DataManager.Actor_name_map.TryAdd(id, itemRawName);
                            }

                            if (NumItems > 0 && NumItems < 100 && !string.IsNullOrWhiteSpace(itemRawName) && itemRawName != "None")
                            {
                                numberOfItems += NumItems;
                            }
                        }
                    else
                        offsetCheck[offset] = "Failed4";
                    if (numberOfItems > 0 && (!offsetCheck.TryGetValue(offset, out string check) || !check.Contains("Failed")))
                    {
                        offsetCheck[offset] = "Succeeded";
                        ItemCount = numberOfItems;
                    }
                }
                catch (Exception ex) 
                {
                    offsetCheck[offset] = "Failed5";
                }
            }
            rm.ThrowExceptions = false;

            if (offsetCheck.Where(o => o.Value == "Succeeded").Count() == 1)
                return offsetCheck.Where(o => o.Value == "Succeeded").First().Key;
            else if (offsetCheck.Where(o => !o.Value.Contains("Failed")).Count() == 1)
                return offsetCheck.Where(o => !o.Value.Contains("Failed")).First().Key;
            else if (offsetCheck.Where(o => o.Value.Contains("Failed")).Count() == _storageOffsets.Count())
            {
                if(offsetCheck.Where(o => o.Value.Contains("Failed5")).Count() == _storageOffsets.Count())
                {
                    BasicActor basicActor = new BasicActor() { ActorAddress = ActorAddress, ActorId = ActorId, RawName = Rawname };
                    SoT_DataManager.IgnoreBasicActors.AddOrUpdate(ActorAddress, basicActor, (k,v) => basicActor);
                    ToDelete = true;
                    //SoT_DataManager.IgnoreActors.Add(ActorId, Rawname);
                    //SoT_DataManager.IgnorePatternList.Add(Rawname);
                }
                return -2;
            }
            return -1;
        }

        private int GetCurrentItemCount()
        {
            int numberOfItems = 0;
            var containerNodes = rm.ReadULong((UIntPtr)ContainerNodesPtr);

            var containerNodesCount = rm.ReadUInt((UIntPtr)ContainerNodesPtr + 8);
            if (containerNodesCount >= 0 && containerNodesCount < 100)
                for (int i = 0; i < containerNodesCount; i++)
                {
                    var NumItems = rm.ReadInt((IntPtr)(containerNodes + (ulong)i * 16 + 8)); //0x10 is 16 and it is the size of the FStorageContainerNode class

                    if (NumItems < 0 || NumItems > 100)
                    {
                        //SoT_DataManager.IgnorePatternList.Add(Rawname);
                        ToDelete = true;
                        break;
                    }
                    numberOfItems += NumItems;
                }
            return numberOfItems;
        }

        private void UpdateStorage()
        {
            if (Parent > 0)
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
            if (ParentComponent > 0)
            {
                string parentRawname = rm.ReadGname(GetActorId(ParentComponent));
                Ship ship = SoT_DataManager.Ships.FirstOrDefault(x => x.ActorAddress == ParentComponent);
                if (ship != null)
                {
                    ship.Loot.TryAdd(this.ActorAddress, this);
                }
                // do not display items in chests
                if (!parentRawname.ToLower().Contains("ship") && !parentRawname.ToLower().Contains("pirate")
                    && parentRawname.ToLower().Contains("chest")
                    )//&& !Rawname.ToLower().Contains("chest")
                {
                    this.ShowText = false;
                    this.ShowIcon = false;
                    return;
                }
            }

            //var ShowCapacityInDescription = rm.ReadBool(StorageContainerComponent + 0x308);
            var backingstore = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes");
            var containerNodes = rm.ReadULong((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes"));

            ItemCount = 0;

            var containerNodesCount = rm.ReadUInt((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes") + 8);
            if (containerNodesCount >= 0 && containerNodesCount < 100)
            {
                Items.Clear();
                //ItemsRaw.Clear();
                for (int i = 0; i < containerNodesCount; i++)
                {
                    var NumItems = rm.ReadInt((IntPtr)(containerNodes + (ulong)i * (ulong)SDKService.GetOffset("StorageContainerNode.Size") + 8)); //0x10 is 16 and it is the size of the FStorageContainerNode class

                    if (NumItems < 0 || NumItems > 100)
                    {
                        ToDelete = true;
                        ShowIcon = false;
                        ShowText = false;
                        return;
                    }

                    var itemDesc = rm.ReadULong((UIntPtr)(containerNodes + (ulong)i * (ulong)SDKService.GetOffset("StorageContainerNode.Size")));
                    int id = rm.ReadInt((IntPtr)itemDesc + 24);
                    string itemRawName = "";
                    itemRawName = rm.ReadGname(id);
                    SoT_DataManager.Actor_name_map.TryGetValue(id, out itemRawName);
                    if (string.IsNullOrWhiteSpace(itemRawName))
                    {
                        itemRawName = rm.ReadGname(id);
                        if (!string.IsNullOrWhiteSpace(itemRawName))
                        {
                            SoT_DataManager.Actor_name_map.TryAdd(id, itemRawName);
                        }
                    }

                    if (NumItems > 0 && NumItems < 100 && !string.IsNullOrWhiteSpace(itemRawName) && itemRawName != "None")
                    {
                        ItemCount += NumItems;
                        var itemName = SoT_Tool.GetMatch(itemRawName, SoT_DataManager.ActorName_keys);
                        if (string.IsNullOrWhiteSpace(itemName))
                            itemName = itemRawName;

                        if (!Items.TryAdd(itemName, NumItems))
                        {
                            Items[itemName] += NumItems;
                        }
                        //if (!ItemsRaw.TryAdd(itemRawName, NumItems))
                        //{
                        //    ItemsRaw[itemRawName] += NumItems;
                        //}
                    }
                }
            }
        }

        public Dictionary<string, int> GetSimpleStorageItems()
        {
            var items = new Dictionary<string, int>();
            //var ShowCapacityInDescription = rm.ReadBool(StorageContainerComponent + 0x308);
            var backingstore = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes");
            var containerNodes = rm.ReadULong((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes"));

            ItemCount = 0;

            var containerNodesCount = rm.ReadUInt((UIntPtr)backingstore + (uint)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes") + 8);
            if (containerNodesCount >= 0 && containerNodesCount < 100)
            {
                //Items.Clear();
                //ItemsRaw.Clear();
                for (int i = 0; i < containerNodesCount; i++)
                {
                    var NumItems = rm.ReadInt((IntPtr)(containerNodes + (ulong)i * (ulong)SDKService.GetOffset("StorageContainerNode.Size") + 8)); //0x10 is 16 and it is the size of the FStorageContainerNode class

                    if (NumItems < 0 || NumItems > 100)
                    {
                        continue;
                    }

                    var itemDesc = rm.ReadULong((UIntPtr)(containerNodes + (ulong)i * (ulong)SDKService.GetOffset("StorageContainerNode.Size")));
                    int id = rm.ReadInt((IntPtr)itemDesc + 24);
                    string itemRawName = "";
                    itemRawName = rm.ReadGname(id);

                    if (NumItems > 0 && NumItems < 100 && !string.IsNullOrWhiteSpace(itemRawName) && itemRawName != "None")
                    {
                        ItemCount += NumItems;
                        string itemName = "O";
                        if(itemRawName.StartsWith("BP_fod") && !itemRawName.Contains("Leeches")
                            && !itemRawName.Contains("Earthworms") && !itemRawName.Contains("Grubs") && !itemRawName.ToLower().Contains("raw")
                            && !itemRawName.ToLower().Contains("uncooked") && !itemRawName.ToLower().Contains("undercooked") && !itemRawName.ToLower().Contains("burned"))
                        {
                            itemName = "F";
                        }
                        else if (itemRawName.StartsWith("BP_fod") && (itemRawName.ToLower().Contains("raw")
                            || itemRawName.ToLower().Contains("uncooked") || itemRawName.ToLower().Contains("undercooked")))
                        {
                            itemName = "UF";
                        }
                        else if (itemRawName == "BP_gmp_repair_wood_02_a_ItemDesc_C")
                        {
                            itemName = "W";
                        }
                        else if (itemRawName == "BP_cmn_cannon_ball_01_a_ItemDesc_C")
                        {
                            itemName = "C";
                        }
                        else if (itemRawName.StartsWith("BP_cmn_cannonball"))
                        {
                            itemName = "SC";
                        }
                        else if (itemRawName.StartsWith("BP_cmn_cannonball_cur"))
                        {
                            itemName = "CB";
                        }
                        if (!items.TryAdd(itemName, NumItems))
                        {
                            items[itemName] += NumItems;
                        }
                        //if (!ItemsRaw.TryAdd(itemRawName, NumItems))
                        //{
                        //    ItemsRaw[itemRawName] += NumItems;
                        //}
                    }
                }
            }
            return items;
        }

        protected override string BuildTextString()
        {
            if(ItemCount > 0) 
            {
                return $"{Name} ({ItemCount}) - {Distance}m";
            }

            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            if(!ToDelete)
            try 
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    return;
                }
                if (nextUpdate > DateTime.Now.Ticks)
                    return;
                    
                if (Parent != 0 && !bool.Parse(ConfigurationManager.AppSettings["ShowItemsOnShips"]))
                {
                    var parentActorId = rm.ReadInt((IntPtr)Parent + (int)SDKService.GetOffset("Actor.actorId"));
                    var parentRawname = rm.ReadGname(parentActorId);
                    if (SoT_DataManager.Ship_keys.ContainsKey(parentRawname))
                    {
                        ShowIcon = false;
                        ShowText = false;
                    }
                }
                if (ParentComponent != 0 && !bool.Parse(ConfigurationManager.AppSettings["ShowItemsOnShips"]))
                {
                    var parentActorId = rm.ReadInt((IntPtr)ParentComponent + (int)SDKService.GetOffset("Actor.actorId"));
                    var parentRawname = rm.ReadGname(parentActorId);
                    if (SoT_DataManager.Ship_keys.ContainsKey(parentRawname))
                    {
                        ShowIcon = false;
                        ShowText = false;
                    }
                }

                if (ContainerNodesPtr == 0)
                {
                    if(nextUpdate > DateTime.Now.Ticks)
                    {
                        return;
                    }

                    var storagecontaineroffset = GetStorageContainerOffset(ActorAddress);
                    if (storagecontaineroffset > 0)
                    {
                        StorageContainerComponent = rm.ReadULong(ActorAddress + (ulong)storagecontaineroffset);
                        ContainerNodesPtr = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes") 
                                + (ulong)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes");
                        UpdateStorage();
                    }
                    else if(StorageContainerComponent != 0)
                    {
                        ContainerNodesPtr = StorageContainerComponent + (ulong)SDKService.GetOffset("StorageContainerComponent.ContainerNodes") 
                                + (ulong)SDKService.GetOffset("StorageContainerBackingStore.ContainerNodes");
                        UpdateStorage();
                    }
                    else
                    {
                        nextUpdate= DateTime.Now.AddSeconds(10).Ticks;
                    }
                    this.ShowText = false;
                    this.ShowIcon = false;

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

                    // Update the position of our circle and text
                    //this.Icon.X = this.ScreenCoords[0];
                    //this.Icon.Y = this.ScreenCoords[1];
                    //this.TextRender.X = this.ScreenCoords[0] + TEXT_OFFSET_X;
                    //this.TextRender.Y = this.ScreenCoords[1] + TEXT_OFFSET_Y;

                    if (StorageContainerComponent > 0)
                    {
                        //UpdateStorage();

                        int newcount = GetCurrentItemCount();
                        //var contains = Items;
                        if (ItemCount != newcount)
                            UpdateStorage();

                        // Update our text to reflect our new distance
                        this.Distance = newDistance;
                        this.Text = BuildTextString();
                        if(ItemCount==0 && Color == Color.White)
                        {
                            this.ShowText = false;
                            this.ShowIcon = false;
                        }
                    }
                    else
                    {
                        this.ShowText = false;
                        this.ShowIcon = false;
                    }
                    
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
            catch(Exception ex) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;
                
                this.ShowText = false;
                this.ShowIcon = false;
                //ToDelete = true;
            }

           
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowContainers"]))
                return;
            if (ShowIcon)
            {
                //renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                //    Icon.size, 1, Icon.IconColor, true);

                //renderer.DrawBox(ScreenCoords.Value.X + Icon.Offset_X, ScreenCoords.Value.Y + Icon.Offset_Y,
                //    Icon.size, Icon.size, Icon.size / 2, Icon.IconColor, true);
            }
            if (ShowText)
            {
                CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X,
                    ScreenCoords.Value.Y,
                    Text, Color, 0);

                if (!bool.Parse(ConfigurationManager.AppSettings["ShowContainerItems"]) || Rawname == "BP_MerchantCrate_AnyItemCrate_Proxy_C")
                    return;

                for(int i = 0; i < Items.Count; i++)
                {
                    string displayText = "";
                    //if (Items.ToList()[i].Value == 1)
                    //    displayText = $"{Items.ToList()[i].Key}";
                    //else
                    displayText = $"[{Items.ToList()[i].Value}] {Items.ToList()[i].Key}";

                    // Container Item text
                    CharmService.DrawOutlinedString(renderer, ScreenCoords.Value.X,
                    ScreenCoords.Value.Y + (i + 1) * CharmService.TextSize - 1,
                    displayText, Color, - 1);

                }
            }
        }
        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["ShowContainers"]))
                return;

            if (ShowText)
            {
                renderer.DrawOutlinedString(ScreenCoords.Value.X,
                    ScreenCoords.Value.Y,
                    Text, Color, 0);

                if (!bool.Parse(ConfigurationManager.AppSettings["ShowContainerItems"]) || Rawname == "BP_MerchantCrate_AnyItemCrate_Proxy_C")
                    return;

                for (int i = 0; i < Items.Count; i++)
                {
                    string displayText = "";
                    //if (Items.ToList()[i].Value == 1)
                    //    displayText = $"{Items.ToList()[i].Key}";
                    //else
                    displayText = $"[{Items.ToList()[i].Value}] {Items.ToList()[i].Key}";

                    // Container Item text
                    renderer.DrawOutlinedString(ScreenCoords.Value.X,
                    ScreenCoords.Value.Y + (i + 1) * CharmService.TextSize - 1,
                    displayText, Color, -1);

                }
            }
        }
    }
}
