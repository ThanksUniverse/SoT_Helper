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
using System.Windows.Forms;
using static SoT_Helper.Services.InputHelper;

namespace SoT_Helper.Models
{
    // Enum Cooking.ECookingState
    enum ECookingState
    {
        Raw,
	    Undercooked,
	    Cooked,
	    Burned,
	    Fresh,
	    ECookingState_MAX,
    };

    enum CookingBotState
    {
        Idle,
        Cooking,
        FocusingOnCookingPot,
        FocusingOnTargetFoodBarrel,
        FocusingOnSourceFoodBarrel,
        TakingFoodFromBarrel,
        CheckingForFood,
        PuttingFoodInPot,
        WaitingForCookingToFinish,
        PuttingCookedFoodInBarrel,
        Done,
        Failed,
    }

    public class CookPot : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.Yellow;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }
        private Keys ActiveKey = Keys.None;

        private long waitUntil = 0;
        private ulong _playerAddress;

        private StorageContainer storageContainerSource;
        private StorageContainer storageContainerTarget;
        private List<ulong> cookingPots = new List<ulong>();
        private int sourceUncookedCount = 0;
        private int targetUncookedCount = 0;
        private CookingBotState cookingBotState = CookingBotState.Idle;
        int itemsChecked = 0;
        bool cookingBotActive = false;
        bool containerOpen = false;
        Coordinates botCoords;
        int shipdistance;
        string lastCheckedItem = "";

        public CookPot(MemoryReader memoryReader, ulong address, ulong playerAddress)
            : base(memoryReader)
        {
            rm = memoryReader;
            ActorId = GetActorId(address);
            ActorAddress = address;
            _playerAddress = playerAddress;
            if (ActorId > 0 && ActorId < 500000)
            {
                Rawname = _rawName = rm.ReadGname(ActorId);
            }
            else
                return;
            ActiveKey = Keys.None;
            Name = Rawname;
            actor_root_comp_ptr = GetRootComponentAddress(address);
            Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            Text = BuildTextString();
            //Icon = new Icon(Shape.Circle, 5, Color, 0, 0);
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);
            //find all storage containers within 2 meters

            FindFoodContainers();
            FindCookingpots();

            var cookerComponent = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("CookingPot.CookerComponent"));
            var currentlyCookingItem = rm.ReadULong(cookerComponent + (ulong)SDKService.GetOffset("CookingClientRepresentation.CurrentlyCookingItem"));
            var currentlyCookingItemName = rm.ReadRawname(currentlyCookingItem);
            var CookingClientRepresentation = cookerComponent + (ulong)SDKService.GetOffset("CookerComponent.CookingState");
            var hasItem = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.HasItem"));
            var isCooking = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.Cooking"));
            var VisibleCookedExtent = rm.ReadFloat(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.VisibleCookedExtent"));
            var wieldeditemcomponent = rm.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + (uint)SDKService.GetOffset("AthenaCharacter.WieldedItemComponent"));
            var currentlywieldeditem = rm.ReadULong((UIntPtr)wieldeditemcomponent + (uint)SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem"));
            var itemname = rm.ReadRawname(currentlywieldeditem);

            if(isCooking)
                cookingBotState = CookingBotState.Cooking;
            else if(hasItem)
                cookingBotState = CookingBotState.CheckingForFood;
            else
                cookingBotState = CookingBotState.CheckingForFood;
        }

        private void FindCookingpots()
        {
            var cookpots = SoT_DataManager.Actors.Where(a => a.Value.RawName.Contains("CookingPot"))
                .Select(a => a.Value).ToList();
            if (cookpots.Count == 0)
            {
                MessageBox.Show("No cookpots found");
                return;
            }
            for (int i = 0; i < cookpots.Count; i++)
            {
                var pot = cookpots[i];
                pot.Distance = (float)SoT_Tool.GetDistanceFromActor(pot.ActorAddress);
            }
            var cookingpots = cookpots.Where(a => (float)SoT_Tool.GetDistanceFromActor(a.ActorAddress) < 3).ToList();
            cookingPots = cookingpots.Select(a => a.ActorAddress).ToList();
        }

        private void FindFoodContainers()
        {
            List<StorageContainer> storageContainers = new List<StorageContainer>();
            if(SoT_DataManager.DisplayObjects.Any(x => x.Rawname.Contains("AnyItemCrate") && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3))
            {
                var storagecrates = SoT_DataManager.DisplayObjects.Where(x => x.Rawname.Contains("AnyItemCrate") && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3).Select(a => a).ToList();
                foreach (var crate in storagecrates.Where(s => !(s is StorageContainer)))
                {
                    var storageAddress = rm.ReadULong(crate.ActorAddress + (ulong)SDKService.GetOffset("Actor.Owner"));
                    var id = GetActorId(storageAddress);
                    var rawname = rm.ReadGname(id);
                    var storage = new StorageContainer(rm, id, storageAddress, rawname);
                    storage.Update(SoT_Tool.my_coords);
                    storageContainers.Add(storage);
                }
            }
            if (!SoT_DataManager.DisplayObjects.Any(x => x is StorageContainer && (x.Rawname.Contains("Food") || x.Rawname.Contains("AnyItem")) && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3))
                return;

            var foodContainers = SoT_DataManager.DisplayObjects.Where(x => x is StorageContainer && (x.Rawname.Contains("Food") || x.Rawname.Contains("AnyItem")) && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3).Select(a => (StorageContainer)a).ToList();
            foodContainers.AddRange(storageContainers);
            //SoT_DataManager.Actors.Where(x => x.Value.RawName.EndsWith("StorageBarrel_Food_C")).Select(a => a.Value).ToList();
            //
            if (foodContainers.Count() > 2)
            {
                //var test = foodContainers.ToDictionary(x => x, x => x.GetSimpleStorageItems().Where(f => f.Key == "UF").Sum(f => f.Value)).OrderBy(x => x.Value);

                // Select two of the storage containers with the most uncooked food items
                var closestContainers = foodContainers.OrderByDescending(x => x.GetSimpleStorageItems().Where(f => f.Key == "UF").Sum(f => f.Value)).Take(2).ToList();
                foodContainers = closestContainers;
            }

            for (int i = 0; i < foodContainers.Count(); i++)
            {
                var barrel = foodContainers.ElementAt(i);
                StorageContainer storageContainer = new StorageContainer(rm, barrel.ActorId, barrel.ActorAddress, barrel.Rawname);
                storageContainer.Update(SoT_Tool.my_coords);
                var items = storageContainer.GetSimpleStorageItems();
                var uncookedCount = items.Where(x => x.Key == "UF").Sum(x => x.Value);
                if (i == 0)
                {
                    storageContainerSource = storageContainer;
                    sourceUncookedCount = uncookedCount;
                }
                else
                {
                    if (uncookedCount > sourceUncookedCount)
                    {
                        // we want the source to have the most uncooked food items
                        targetUncookedCount = sourceUncookedCount;
                        storageContainerTarget = storageContainerSource;
                        storageContainerSource = storageContainer;
                        sourceUncookedCount = uncookedCount;
                    }
                    else
                    {
                        storageContainerTarget = storageContainer;
                        targetUncookedCount = uncookedCount;
                    }
                }
            }
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public override void Update(Coordinates myCoords)
        {
            if(ToDelete)
                return;
            try
            {
                if (!CheckRawNameAndActorId(ActorAddress))
                {
                    return;
                }

                ScreenCoords = new System.Numerics.Vector2(500, 480);
                this.ShowText = true;
                this.ShowIcon = true;
                if(ProcessUtils.IsForegroundWindow() && cookingBotActive)
                {
                    if (waitUntil > DateTime.UtcNow.Ticks)
                    {
                        return;
                    }

                    var item = SoT_Tool.GetCurrentlyWieldedItemRawName();
                    if (item.ToLower().Contains("cutlass") || item.ToLower().Contains("pistol") || item.ToLower().Contains("blunder") || item.ToLower().Contains("rifle"))
                    {
                        cookingBotActive = false;
                        cookingBotState = CookingBotState.Idle;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        Text = "You are wielding a weapon. Deactivating fishing bot.";
                    }
                    else
                        AutoCook();
                    return;
                }
                Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                if (!cookingBotActive && MathHelper.CalculateDistance(Coords, SoT_Tool.my_coords) < 3)
                {
                    var cookerComponent = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("CookingPot.CookerComponent"));
                    //var currentlyCookingItem = rm.ReadULong(cookerComponent + (ulong)SDKService.GetOffset("CookingClientRepresentation.CurrentlyCookingItem"));
                    //var currentlyCookingItemName = rm.ReadRawname(currentlyCookingItem);
                    var CookingClientRepresentation = cookerComponent + (ulong)SDKService.GetOffset("CookerComponent.CookingState");
                    //var hasItem = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.HasItem"));
                    var isCooking = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.Cooking"));
                    //var VisibleCookedExtent = rm.ReadFloat(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.VisibleCookedExtent"));
                    //var wieldeditemcomponent = rm.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + SDKService.GetOffset("AthenaCharacter.WieldedItemComponent"));
                    //var currentlywieldeditem = rm.ReadULong((UIntPtr)wieldeditemcomponent + SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem"));
                    //var itemname = rm.ReadRawname(currentlywieldeditem);
                    if(isCooking)
                    {
                        Text = "Cooking bot detected cooking";
                        cookingBotState = CookingBotState.CheckingForFood;
                        cookingBotActive = true;
                        //botCoords = new Coordinates() { x = SoT_Tool.my_coords.GetPosition().X, y = SoT_Tool.my_coords.GetPosition().Y, z = SoT_Tool.my_coords.GetPosition().Z };
                        //shipdistance = MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords);
                        FindFoodContainers();
                    }
                }
            }
            catch (Exception ex) 
            {
                var test1 = Rawname;
                var test2 = ActorId;
                var test3 = ToDelete;

                ShowIcon = false;
                ShowText = false;
                //ToDelete = true;
                SoT_DataManager.InfoLog += $"Error updating {Name}: {ex.Message}\n";
            }
        }

        private void AutoCook()
        {
            try
            {
                if (containerOpen)
                {
                    if (ActiveKey != Keys.None)
                    {
                        InputHelper.ReleaseKey(ActiveKey);
                        ActiveKey = Keys.None;
                    }
                    //if(!MouseOver(SoT_Tool.GetScreenCenter()))
                    //    return;

                    InputHelper.PressKey(Keys.X);
                    ActiveKey = Keys.X;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                    containerOpen = false;
                    return;
                }

                if (itemsChecked >= 5)
                {
                    //ReleaseKeys();
                    //SoT_DataManager.InfoLog += $"\nCooking bot cannot find any relevant items to cook.";

                    if (sourceUncookedCount > 0)
                    {
                        ReleaseKeys();
                        if (ActiveKey != Keys.None)
                        {
                            InputHelper.ReleaseKey(ActiveKey);
                            ActiveKey = Keys.None;
                            waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                            return;
                        }
                        if (cookingBotState == CookingBotState.TakingFoodFromBarrel)
                        {
                            var mpos = InputHelper.GetCursorPositionVector2();
                            var center = ProcessUtils.GetScreenCenter();
                            var windowPos = ProcessUtils.GetProcessWindowPosition();
                            var centerRight = center * new Vector2(1.5f, 1f);
                            var targetPos = centerRight + windowPos;

                            Text = $"Mouse pos {mpos} Target: {targetPos}";

                            var loadout = SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food"));
                            var storage = storageContainerSource.GetSimpleStorageItems();
                            if (MouseOver(targetPos, 10))
                            {
                                if (loadout.Items.Count <
                                    loadout.Capacity
                                    && storage.Where(x => x.Key == "UF").Sum(x => x.Value) > 0)
                                {
                                    InputHelper.PressKey(Keys.F);
                                    ActiveKey = Keys.F;
                                    waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                                    return;
                                }
                                containerOpen = true;
                                cookingBotState = CookingBotState.CheckingForFood;
                                Text = "Checking for food to cook";
                                itemsChecked = 0;

                                if (storage.Where(x => x.Key == "UF").Sum(x => x.Value) == 0)
                                {
                                    sourceUncookedCount = 0;
                                }
                                return;
                            }
                            return;
                        }
                        else if (cookingBotState == CookingBotState.FocusingOnSourceFoodBarrel)
                        {
                            Text = "Focusing on source food barrel";
                            if (LookAtActor(storageContainerSource.ActorAddress, 10))
                            {
                                cookingBotState = CookingBotState.TakingFoodFromBarrel;
                                Text = "Taking food from barrel";
                                if (storageContainerSource.Rawname.Contains("AnyItem"))
                                {
                                    InputHelper.PressKey(Keys.R);
                                    ActiveKey = Keys.R;
                                }
                                else
                                {
                                    InputHelper.PressKey(Keys.F);
                                    ActiveKey = Keys.F;
                                }
                                waitUntil = DateTime.UtcNow.AddSeconds(2).Ticks;
                            }
                            return;
                        }
                        else if (cookingBotState == CookingBotState.PuttingCookedFoodInBarrel)
                        {
                            var mpos = InputHelper.GetCursorPositionVector2();
                            var center = ProcessUtils.GetScreenCenter();
                            var windowPos = ProcessUtils.GetProcessWindowPosition();
                            var targetPos = center * new Vector2(0.5f, 1) + windowPos;

                            Text = $"Mouse pos {InputHelper.GetCursorPositionVector2()}";

                            if (MouseOver(targetPos, 7))
                            {
                                var loadout = SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food"));
                                if (loadout.Items.Count > 0)
                                {
                                    InputHelper.PressKey(Keys.F);
                                    ActiveKey = Keys.F;
                                    waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                                    return;
                                }

                                cookingBotState = CookingBotState.FocusingOnSourceFoodBarrel;
                                containerOpen = true;
                            }
                            return;
                        }
                        else if (cookingBotState == CookingBotState.FocusingOnTargetFoodBarrel)
                        {
                            cookingBotState = CookingBotState.FocusingOnTargetFoodBarrel;
                            Text = "Focusing on target food barrel";
                            if (LookAtActor(storageContainerTarget.ActorAddress, 10))
                            {
                                cookingBotState = CookingBotState.PuttingCookedFoodInBarrel;
                                Text = "Putting cooked food in barrel";
                                if (storageContainerTarget.Rawname.Contains("AnyItem"))
                                {
                                    InputHelper.PressKey(Keys.R);
                                    ActiveKey = Keys.R;
                                }
                                else
                                {
                                    InputHelper.PressKey(Keys.F);
                                    ActiveKey = Keys.F;
                                }
                                //InputHelper.PressKey(Keys.F);
                                //ActiveKey = Keys.F;
                                waitUntil = DateTime.UtcNow.AddSeconds(2).Ticks;
                            }
                            return;
                        }
                        itemsChecked = 0;
                        Text = "Idle";
                        return;
                    }
                    if (storageContainerTarget != null && storageContainerSource != null)
                    {
                        var storageTarget = storageContainerTarget.GetSimpleStorageItems();
                        if (storageTarget.Where(x => x.Key == "UF").Count() > 0)
                        {
                            targetUncookedCount = sourceUncookedCount;
                            sourceUncookedCount = storageTarget.Where(x => x.Key == "UF").Count();
                            var newsource = storageContainerTarget;
                            storageContainerTarget = storageContainerSource;
                            storageContainerSource = newsource;
                            itemsChecked = 5;
                            cookingBotState = CookingBotState.FocusingOnSourceFoodBarrel;
                            return;
                        }
                    }

                    //ToDelete = true;
                    itemsChecked = 0;
                    cookingBotState = CookingBotState.Idle;
                    cookingBotActive = false;
                    return;
                }
                foreach(var pot in cookingPots)
                {
                    CheckCookingPot(pot);
                }

                return;
            }
            catch (Exception ex)
            {
                SoT_DataManager.InfoLog += "\n" + ex.ToString();
                SoT_DataManager.InfoLog += "\nCooking bot crashed.";
                ToDelete = true;
                return;
            }
        }

        private void CheckCookingPot(ulong address)
        {
            var cookerComponent = rm.ReadULong(address + (ulong)SDKService.GetOffset("CookingPot.CookerComponent"));
            var currentlyCookingItem = rm.ReadULong(cookerComponent + (ulong)SDKService.GetOffset("CookingClientRepresentation.CurrentlyCookingItem"));
            string currentlyCookingItemName = "None";
            if (currentlyCookingItem != 0)
                currentlyCookingItemName = rm.ReadRawname(currentlyCookingItem);
            var CookingClientRepresentation = cookerComponent + (ulong)SDKService.GetOffset("CookerComponent.CookingState");
            var hasItem = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.HasItem"));
            var isCooking = rm.ReadBool(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.Cooking"));
            var VisibleCookedExtent = rm.ReadFloat(CookingClientRepresentation + (ulong)SDKService.GetOffset("CookingClientRepresentation.VisibleCookedExtent"));
            var wieldeditemcomponent = rm.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + (uint)SDKService.GetOffset("AthenaCharacter.WieldedItemComponent"));
            var currentlywieldeditem = rm.ReadULong((UIntPtr)wieldeditemcomponent + (uint)SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem"));
            var itemname = rm.ReadRawname(currentlywieldeditem);

            if (cookingBotState == CookingBotState.FocusingOnCookingPot)
            {
                actor_root_comp_ptr = GetRootComponentAddress(ActorAddress);
                Coords = CoordBuilder(actor_root_comp_ptr, coord_offset);
                Distance = MathHelper.CalculateDistance(this.Coords, SoT_Tool.my_coords);
                if (Distance > 2)
                {
                    ActiveKey = Keys.None;
                    ReleaseKeys();
                    //ToDelete = true;
                    //SoT_DataManager.InfoLog += $"\nCooking bot too far from cooking pot.";
                    //SoT_DataManager.InfoLog += $"\nCooking bot self destructing. Hastalavista.";
                    Text = $"Cooking bot too far from cooking pot. {Distance}m";
                    cookingBotState = CookingBotState.Idle;
                    cookingBotActive = false;
                    waitUntil = DateTime.UtcNow.AddSeconds(1000).Ticks;
                    return;
                }
                if (LookAtActor(address))
                {
                    if (VisibleCookedExtent > 1)
                    {
                        InputHelper.PressKey(Keys.F);
                        Text = "Taking cooked item";
                        waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                        return;
                    }
                    else
                    {
                        Text = $"Looking at cooking pot";
                        cookingBotState = CookingBotState.PuttingFoodInPot;
                        return;
                    }
                }
                else
                {
                    Text = $"Looking for cooking pot";
                    return;
                }
            }

            if (cookingBotState == CookingBotState.Idle)
            {
                if (isCooking)
                {
                    cookingBotState = CookingBotState.Cooking;
                    cookingBotActive = true;
                    Text = $"Cooking level {VisibleCookedExtent}";
                    botCoords = new Coordinates() { x = SoT_Tool.my_coords.GetPosition().X, y = SoT_Tool.my_coords.GetPosition().Y, z = SoT_Tool.my_coords.GetPosition().Z };
                    shipdistance = MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords);
                    waitUntil = DateTime.UtcNow.AddSeconds(1).Ticks;
                }
                else
                {
                    Text = $"Idle";
                }
                return;
            }

            if (!hasItem && !isCooking && VisibleCookedExtent == 0 && (currentlyCookingItemName == "None" || currentlyCookingItemName == ""))
            {
                if ((itemname.Contains("Raw") || itemname.Contains("Undercooked") || itemname.ToLower().Contains("uncooked")) &&
                    (cookingBotState == CookingBotState.FocusingOnCookingPot || cookingBotState == CookingBotState.PuttingFoodInPot))
                {
                    cookingBotState = CookingBotState.PuttingFoodInPot;
                    InputHelper.ReleaseKey(Keys.F);
                    Text = "Placing next item to cook";
                    waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                    //itemsChecked++;
                    if (ActiveKey != Keys.None)
                        InputHelper.ReleaseKey(ActiveKey);
                    InputHelper.PressKey(Keys.F);
                    ActiveKey = Keys.F;
                    return;
                }
                else if (itemname.ToLower().Contains("raw") || itemname.ToLower().Contains("undercooked")
                    || itemname.ToLower().Contains("uncooked"))
                {
                    cookingBotState = CookingBotState.FocusingOnCookingPot;
                    Text = "Focusing on cooking pot";
                    return;
                }
                else
                {
                    var loadout = SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food"));
                    if (!loadout.Items.Any())
                    {
                        InputHelper.ReleaseKey(ActiveKey);
                        cookingBotState = CookingBotState.FocusingOnSourceFoodBarrel;
                        Text = "Focusing on source food barrel";
                        itemsChecked = 5;
                        return;
                    }
                    else if (!loadout.Items.Any(i => i.ToLower().Contains("raw")
                    || i.ToLower().Contains("uncooked") || i.ToLower().Contains("undercooked")))
                    {
                        itemsChecked = 5;
                        Text = "Focusing on target food barrel";
                        cookingBotState = CookingBotState.FocusingOnTargetFoodBarrel;
                        return;
                    }
                    if (ActiveKey != Keys.None)
                        InputHelper.ReleaseKey(ActiveKey);
                    SoT_DataManager.InfoLog += $"\n{itemname} does not need to be cooked. Checking next item.";
                    cookingBotState = CookingBotState.CheckingForFood;
                    Text = "Checking for next item to cook";
                    InputHelper.PressKey(Keys.D3);
                    lastCheckedItem = itemname;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                    ActiveKey = Keys.D3;
                    itemsChecked++;
                    if (itemsChecked > 4)
                    {
                        cookingBotState = CookingBotState.FocusingOnTargetFoodBarrel;
                        Text = "Focusing on target food barrel";
                        lastCheckedItem = "";
                    }
                    return;
                }
            }
            else
            {
                if (ActiveKey != Keys.None)
                    InputHelper.ReleaseKey(ActiveKey);
                ActiveKey = Keys.None;
                itemsChecked = 0;
                if (itemname != "None" && itemname != "")
                {
                    InputHelper.PressKey(Keys.X);
                    ActiveKey = Keys.X;
                    Text = "Putting item away. Awaiting cooking.";
                    waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                    return;
                }
                if (VisibleCookedExtent > 1.5f)
                {
                    //InputHelper.PressKey(Keys.F);
                    //Text = "Taking cooked item";
                    //waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                    cookingBotState = CookingBotState.FocusingOnCookingPot;
                    return;
                }
                else
                {
                    Text = $"Cooking level {VisibleCookedExtent}";
                    return;
                }
            }
        }

        private bool MouseOver(Vector2 target, float precision = 7)
        {
            var mpos = InputHelper.GetCursorPositionVector2();
            var relativeTarget = target - mpos;
            int x = 0; int y = 0;
            x = (int)(relativeTarget.X / Math.Abs(relativeTarget.X));
            y = (int)(relativeTarget.Y / Math.Abs(relativeTarget.Y));
            if (Math.Abs(relativeTarget.X) > 100)
                x = x * 40;
            else if (Math.Abs(relativeTarget.X) > 40)
                x = x * 15;
            if (Math.Abs(relativeTarget.Y) > 100)
                y = y * 40;
            else if (Math.Abs(relativeTarget.Y) > 40)
                y = y * 15;

            if (Math.Abs(relativeTarget.X) > precision || Math.Abs(relativeTarget.Y) > precision)
            {
                MoveMouse(x, y);
                Text = $"Mouse pos {InputHelper.GetCursorPositionVector2()}";
                //waitUntil = DateTime.UtcNow.AddSeconds(1).Ticks;
                return false;
            }

            return true;
        }

        private bool LookAtActor(ulong address, float precision = 7)
        {
            var rootcompptr = GetRootComponentAddress(address);
            var coords = CoordBuilder(rootcompptr, coord_offset);
            var distance = MathHelper.CalculateDistance(coords, SoT_Tool.my_coords);
            if(distance > 1000)
            {
                SoT_DataManager.InfoLog += $"\nCooking bot failed to look at actor. Distance {distance}";
                cookingBotActive = false;
                cookingBotState = CookingBotState.Idle;
                return false;
            }
            var center = SoT_Tool.GetScreenCenter();
            var target = MathHelper.ObjectTracePointToScreen(coords).Value;
            var pos = GetCursorPositionVector2();
            var relativeTarget = target - center;

            if (Math.Abs(relativeTarget.X) > precision || Math.Abs(relativeTarget.Y) > precision)
            {
                // round vector2 string output to two decimals
                Text = $"Looking for cookpot {target.ToString("F2")} cursor at {pos.ToString("F2")} relative target {relativeTarget.ToString("F2")}";

                int x = 0; int y = 0;
                x = (int)(relativeTarget.X / Math.Abs(relativeTarget.X));
                y = (int)(relativeTarget.Y / Math.Abs(relativeTarget.Y));
                if (Math.Abs(relativeTarget.X) > 100)
                    x = x * 30;
                else if (Math.Abs(relativeTarget.X) > 40)
                    x = x * 10;
                if (Math.Abs(relativeTarget.Y) > 100)
                    y = y * 30;
                else if (Math.Abs(relativeTarget.Y) > 40)
                    y = y * 10;

                InputHelper.MoveMouse((int)x, (int)y);
                return false;
            }
            return true;
        }

        private void ReleaseKeys()
        {
            InputHelper.ReleaseKey((byte)ActiveKey);
            InputHelper.PressMouseKey(MouseEventF.RightUp);
            InputHelper.PressMouseKey(MouseEventF.LeftUp);
            InputHelper.ReleaseKey((byte)Keys.A);
            InputHelper.ReleaseKey((byte)Keys.W);
            InputHelper.ReleaseKey((byte)Keys.S);
            InputHelper.ReleaseKey((byte)Keys.D);
            InputHelper.ReleaseKey(Keys.F);
            InputHelper.ReleaseKey(Keys.D3);
            InputHelper.ReleaseKey(Keys.X);
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if(ToDelete) { return; }

            if (ShowIcon)
            {
                renderer.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y,
                    Size, 1, Color, true);
            }
            if (ShowText)
            {
                // Text
                CharmService.DrawOutlinedString(renderer,ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    "Cooking bot:\n" + Text, Color, 0);
            }
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ToDelete) { return; }

            if (ShowIcon)
            {
                renderer.Graphics.DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y, Size, 1, Color, true);
            }
            if (ShowText)
            {
                // Text
                renderer.DrawOutlinedString(ScreenCoords.Value.X + DisplayText.Offset_X,
                    ScreenCoords.Value.Y + DisplayText.Offset_Y,
                    "Cooking bot:\n" + Text, Color, 0);
            }
        }
    }
}
