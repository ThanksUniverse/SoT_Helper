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
using static System.Net.Mime.MediaTypeNames;

namespace SoT_Helper.Models
{
    // fish name
    // localFishingRod->FishingFloatActor->FishingFloatNameplate->FishName

    // Enum Athena.EFishingRodServerState
    enum EFishingRodServerState
    {
        NotBeingUsed,
	    PreparingToCast,
	    VerifyingCastLocation,
	    Casting,
	    DelayBeforeSpawningFish,
	    RequestFishSpawnWhenPossible,
	    WaitingForAsyncLoadToFinish,
	    WaitingForFishToBite,
	    FishMovingInToBite,
	    FishOnRodAndWaitingForPlayerInput,
	    FishMovingToMinimumDistanceFromPlayer,
	    FishingMiniGameUnderway,
	    FishCaught,
	    ReelingInAComedyItem,
	    ComedyItemCaught,
	    EFishingRodServerState_MAX,
    };

    enum EFishingMiniGamePlayerInputBattlingDirection
    {
        BattlingAgainstLeft,
	    BattlingAgainstRight,
	    BattlingAgainstAway,
	    EFishingMiniGamePlayerInputBattlingDirection_MAX,
    };

    // Enum Athena.EFishingMiniGamePlayerInputDirection
    enum EFishingMiniGamePlayerInputDirection
    {
        None,
	    Left,
	    Away,
	    Right,
	    Towards,
	    EFishingMiniGamePlayerInputDirection_MAX,
    };

    // Enum Athena.EFishingMiniGameEscapeDirection
    enum EFishingMiniGameEscapeDirection
    {
        None,
	    Left,
	    Away,
	    Right,
	    EFishingMiniGameEscapeDirection_MAX,
    };

    // Enum Athena.EFishingRodBattlingState
    enum  EFishingRodBattlingState
    {
        NotBattling,
	    Battling_Tiring,
	    Battling_NotTiring,
	    Battling_Tired,
	    EFishingRodBattlingState_MAX,
    };

    enum FishingBotState
    {
        NotFishing,
        StartedFishing,
        ComedyItemCaught,
        TakingComedyItem,
        FishCaught,
        FocusingOnStorage,
        PuttingFishInStorage,
        FocusingOnFishAngle,
        SelfDestruct,
        FishingBotState_MAX,
    };

    public class FishingBot : DisplayObject
    {
        private static readonly Color ACTOR_COLOR = Color.Yellow;
        private const int CIRCLE_SIZE = 10;

        private readonly string _rawName;
        private Coordinates _coords;
        public Coordinates Coords { get => _coords; set => _coords = value; }
        //public int Size { get; set; }
        private Keys ActiveKey = Keys.None;

        private long waitUntil = 0;
        private long stateTimer = 0;
        private FishingBotState _lastState;
        private ulong _playerAddress;

        private int fishCaught = 0;
        private bool startedFishing = true;
        public bool FishingBotActive { get; set; } = false;

        private Coordinates fishingDirectionPoint;
        private static FishingBotState _fishingBotState = FishingBotState.NotFishing;
        private StorageContainer StorageContainer = null;
        private Marker fishingAngleMarker;
        private Vector3 fishingRotation;
        private bool containerOpen = false;
        private int shipdistance = 0;
        private string fishName = "";
        public static Dictionary<string, Dictionary<string, string>> FishData = new Dictionary<string, Dictionary<string, string>>();
        public static List<string> FishToFish = new List<string>();

        public FishingBot(MemoryReader memoryReader)
            : base(memoryReader)
        {
            rm = memoryReader;
            Name = "FishingBot";
            ActorAddress = 4158;

            ActiveKey = Keys.None;
            Coords = new Coordinates() {x = SoT_Tool.my_coords.GetPosition().X, y = SoT_Tool.my_coords.GetPosition().Y, z = SoT_Tool.my_coords.GetPosition().Z };
            if(Ship.PlayerShip != null)
            {
                shipdistance = MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords);
            }
            // All of our actual display information & rendering
            Color = ACTOR_COLOR;
            //Text = BuildTextString();
            Size = 5;
            DisplayText = new DisplayText(10, Size + 2, -10 / 2);

            GetNearestStorageContainer();

            //test
            //_fishingBotState = FishingBotState.FocusingOnStorage;
            //FishingBotActive = true;
        }

        private void GetNearestStorageContainer()
        {
            List<StorageContainer> storageContainers = new List<StorageContainer>();

            // x.Rawname != null && x.ActorAddress != null && 
            if (SoT_DataManager.DisplayObjects.Any(x => x.Rawname.Contains("AnyItemCrate") && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3))
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
            if(SoT_DataManager.DisplayObjects.Any(x => x is StorageContainer && (x.Rawname.Contains("Food") || x.Rawname.Contains("AnyItem")) && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3))
            {
                var foodContainers = SoT_DataManager.DisplayObjects.Where(x => x is StorageContainer && (x.Rawname.Contains("Food") || x.Rawname.Contains("AnyItem")) && SoT_Tool.GetDistanceFromActor(x.ActorAddress) < 3).Select(a => (StorageContainer)a).ToList();
                StorageContainer = foodContainers.FirstOrDefault();
            }

            if (StorageContainer == null)
            {
                StorageContainer = storageContainers.FirstOrDefault();
            }
            
        }

        protected override string BuildTextString()
        {
            return $"{Name} - {Distance}m";
        }

        public void SelfDestruct()
        {
              _fishingBotState = FishingBotState.SelfDestruct;
        }

        public override void Update(Coordinates myCoords)
        {
            if (ToDelete)
                return;
            try
            {
                ScreenCoords = new System.Numerics.Vector2(500, 560);
                this.ShowText = true;
                this.ShowIcon = true;

                var item = SoT_Tool.GetCurrentlyWieldedItemRawName();
                if (FishingBotActive && (item.ToLower().Contains("cutlass") || item.ToLower().Contains("pistol") || item.ToLower().Contains("blunder") || item.ToLower().Contains("rifle")))
                {
                    FishingBotActive = false;
                    _fishingBotState = FishingBotState.NotFishing;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                    Text = "You are wielding a weapon. Deactivating fishing bot.";
                    ReleaseKeys();
                }

                if (waitUntil > 0 && waitUntil > DateTime.UtcNow.Ticks)
                {
                    return;
                }
                //var test1 = Math.Abs(shipdistance - MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords));
                //var test2 = MathHelper.CalculateDistance(Coords.GetPosition(), SoT_Tool.my_coords);

                //if (FishingBotActive && MathHelper.CalculateDistance(Coords, SoT_Tool.my_coords) > 5 && Math.Abs(shipdistance - MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords)) > 5)
                //{
                //    Text = "Bot has moved. Deactivating fishing bot.";
                //    FishingBotActive = false;
                //    _fishingBotState = FishingBotState.NotFishing;
                //}
                //else if (FishingBotActive && MathHelper.CalculateDistance(Coords, SoT_Tool.my_coords) > 3)
                //    Coords = new Coordinates() { x = SoT_Tool.my_coords.GetPosition().X, y = SoT_Tool.my_coords.GetPosition().Y, z = SoT_Tool.my_coords.GetPosition().Z };

                if (_fishingBotState == FishingBotState.SelfDestruct)
                {
                    //_fishingBotState = FishingBotState.NotFishing;
                    FishingBotActive = false;

                    if(ActorAddress > 5) 
                    {
                        ActorAddress = 5;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(1500).Ticks;
                        Text = "Self destruction sequence initiated...";
                        return;
                    }
                    if (ActorAddress == 5)
                    {
                        ActorAddress--;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        Text = "I have seen things you wouldn't believe.";
                        return;
                    }
                    if (ActorAddress == 4)
                    {
                        ActorAddress--;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        Text = "Galleons on fire outside Reapers Hideout";
                        return;
                    }
                    if (ActorAddress == 3)
                    {
                        ActorAddress--;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                        Text = "I watched Twilight Stormfish glitter in the dark near the Shores of Gold";
                        return;
                    }
                    if (ActorAddress == 2)
                    {
                        ActorAddress--;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        Text = "All these moments will be lost. Like tears in the rain.";
                        return;
                    }
                    if (ActorAddress == 1)
                    {
                        ActorAddress--;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        Text = "Time to die...";
                        return;
                    }
                    if (ActorAddress == 0)
                    {
                        _fishingBotState = FishingBotState.NotFishing;
                        ToDelete = true;
                        Text = "*powers down*";
                        return;
                    }
                    return;
                }

                if (ActorAddress == 4158 && SoT_Tool.PlayerAddress != 0)
                {
                    var wieldeditemcomponent = rm.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + (uint)SDKService.GetOffset("AthenaCharacter.WieldedItemComponent"));
                    var currentlywieldeditem = rm.ReadULong((UIntPtr)wieldeditemcomponent + (uint)SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem"));
                    if(currentlywieldeditem == 0)
                    {
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        return;
                    }
                    var itemname = rm.ReadRawname(currentlywieldeditem);
                    if (!itemname.ToLower().Contains("fishing") && !itemname.ToLower().Contains("rod"))
                    {
                        //InputHelper.PressKey((byte)Keys.K);
                        //ActiveKey = Keys.K;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                        startedFishing = false;
                    }
                    else
                    {
                        ActorAddress = currentlywieldeditem;
                        ActorId = GetActorId(ActorAddress);
                        Rawname = rm.ReadGname(ActorId);
                        Text = "Fishing rod detected";
                        //HackFish();
                        waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                    }
                    return;
                }

                var id = GetActorId(ActorAddress);
                if (id == 0 || id != ActorId)
                {
                    ActorAddress = 4158;
                    _fishingBotState = FishingBotState.NotFishing;
                    FishingBotActive = false;
                    return;
                }

                if(FishingBotActive)
                {
                    if (ProcessUtils.IsForegroundWindow())
                    {
                        AutoFish();
                    }
                    return;
                }

                if(ActorAddress != 4158)
                {
                    var wieldeditemcomponent = rm.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + (uint)SDKService.GetOffset("AthenaCharacter.WieldedItemComponent"));
                    var currentlywieldeditem = rm.ReadULong((UIntPtr)wieldeditemcomponent + (uint)SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem"));
                    //if(currentlywieldeditem == 0 && FishingBotActive && _fishingBotState == FishingBotState.NotFishing)
                    //{
                    //    _fishingBotState = FishingBotState.StartedFishing;
                    //    InputHelper.PressKey(Keys.K);
                    //    ActiveKey = Keys.K;
                    //    waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                    //    Text = "Bot starting to fish";
                    //    return;
                    //}
                    var itemname = rm.ReadRawname(currentlywieldeditem);
                    if (!itemname.ToLower().Contains("fishing") && !itemname.ToLower().Contains("rod"))
                    {
                        Text = "Waiting";
                        //InputHelper.PressKey((byte)Keys.K);
                        //ActiveKey = Keys.K;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                    }
                    else
                    {
                        ActorAddress = currentlywieldeditem;
                        ActorId = GetActorId(ActorAddress);
                        Rawname = rm.ReadGname(ActorId);
                        //ActiveKey = Keys.K;
                        //FishingBotActive = false;
                        Text = "Wielding fishing rod";
                        var serverState = (EFishingRodServerState)rm.ReadByte(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ServerState"));

                        if(serverState != EFishingRodServerState.NotBeingUsed)
                        {
                            FishingBotActive = true;
                            Text = "Fishing bot active. All systems powering up.";
                            _fishingBotState = FishingBotState.StartedFishing;
                            GetNearestStorageContainer();
                            Coords = new Coordinates() { x = SoT_Tool.my_coords.GetPosition().X, y = SoT_Tool.my_coords.GetPosition().Y, z = SoT_Tool.my_coords.GetPosition().Z };
                            shipdistance = MathHelper.CalculateDistance(Coords, Ship.PlayerShip.Coords);
                            fishingRotation = SoT_Tool.my_coords.GetRotation();
                            GetFishingPosition();

                            waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                        }

                        //if (ActiveKey == Keys.K)
                        //    InputHelper.ReleaseKey(Keys.K);

                        //InputHelper.PressKey((byte)Keys.LButton);
                        //ActiveKey = Keys.LButton;
                        //waitUntil = DateTime.UtcNow.AddMilliseconds(1500).Ticks;
                    }
                    return;
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

        private Vector3 GetFishingPosition()
        {
            var pos = SoT_Tool.my_coords.GetPosition();
            var forward = MathHelper.RotationToVector(fishingRotation);
            var fishingPos = pos + (forward * 10);
            //if (fishingAngleMarker == null)
            //{
            //    var marker = new Marker(rm, "Fishing focus point ", ActorAddress, Rawname, fishingPos);
            //    fishingAngleMarker = marker;
            //    SoT_DataManager.DisplayObjects.Add(marker);
            //}
            //else
            //{
            //    fishingAngleMarker.Coords.SetPosition(fishingPos);
            //}
            fishingAngleMarker.Coords.SetPosition(fishingPos);

            return fishingPos;
        }

        private Vector3 GetBehindFishingPosition()
        {
            var pos = SoT_Tool.my_coords.GetPosition();
            var forward = MathHelper.RotationToVector(fishingRotation);
            var fishingPos = pos + (forward * -10);
            //if (fishingAngleMarker == null)
            //{
            //    var marker = new Marker(rm, "Fishing focus point ", ActorAddress, Rawname, fishingPos);
            //    fishingAngleMarker = marker;
            //    SoT_DataManager.DisplayObjects.Add(marker);
            //}
            //else
            //{
            //    fishingAngleMarker.Coords.SetPosition(fishingPos);
            //}
            return fishingPos;
        }

        private void AutoFish()
        {
            if (waitUntil > 0 && waitUntil > DateTime.UtcNow.Ticks)
            {
                return;
            }
            if (containerOpen)
            {
                if (ActiveKey != Keys.None)
                {
                    InputHelper.ReleaseKey(ActiveKey);
                    ReleaseKeys();
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
            var item = SoT_Tool.GetCurrentlyWieldedItemRawName();
            if(item != "None" && !(item.ToLower().Contains("fishing") && item.ToLower().Contains("rod")) && !item.ToLower().Contains("bp_fod"))
            {
                Text = "Dropping item";
                var behindfishAnglePos = GetBehindFishingPosition();
                if (LookAtPoint(behindfishAnglePos))
                {
                    if (ActiveKey == Keys.X)
                    {
                        ReleaseKey(ActiveKey);
                        ActiveKey = Keys.None;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                        return;
                    }
                    else
                    {
                        SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Dropping comedy item";

                        InputHelper.PressKey(Keys.X);
                        ActiveKey = Keys.X;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                    }
                }
                else
                    return;
                _fishingBotState = FishingBotState.FocusingOnFishAngle;
                _lastState = _fishingBotState;
                stateTimer = 0;
                return;
            }
            if (_fishingBotState == FishingBotState.FocusingOnStorage)
            {
                Text = "Focusing on storage";
                _lastState = _fishingBotState;
                if (LookAtActor(StorageContainer.ActorAddress))
                {
                    if (ActiveKey != Keys.None)
                    {
                        ReleaseKeys();
                        InputHelper.ReleaseKey(ActiveKey);
                        ActiveKey = Keys.None;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(200).Ticks;
                        return;
                    }
                    if (SoT_Tool.GetCurrentlyWieldedItemRawName() != "None")
                    {
                        Text = "Unwielding item";
                        InputHelper.PressKey(Keys.X);
                        ActiveKey = Keys.X;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                        return;
                    }

                    _fishingBotState = FishingBotState.PuttingFishInStorage;
                    _lastState = _fishingBotState;
                    stateTimer = DateTime.UtcNow.AddSeconds(20).Ticks;
                    if (StorageContainer.Rawname.Contains("AnyItem"))
                    {
                        Text = "Opening storage crate";
                        InputHelper.PressKey(Keys.R);
                        ActiveKey = Keys.R;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                    }
                    else
                    {
                        Text = "Opening storage barrel";
                        InputHelper.PressKey(Keys.F);
                        ActiveKey = Keys.F;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                    }
                }
                return;
            }
            else if (_fishingBotState == FishingBotState.PuttingFishInStorage)
            {
                if (ActiveKey != Keys.None)
                {
                    InputHelper.ReleaseKeys();
                    ActiveKey = Keys.None;
                    //waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                }
                if(stateTimer < DateTime.UtcNow.Ticks)
                {
                    _fishingBotState = FishingBotState.StartedFishing;
                    _lastState = _fishingBotState;
                    stateTimer = 0;
                    ReleaseKeys();
                    ActiveKey = Keys.None;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                    return;
                }
                var mpos = InputHelper.GetCursorPositionVector2();
                var center = ProcessUtils.GetScreenCenter();
                var windowPos = ProcessUtils.GetProcessWindowPosition();
                var targetPos = center * new Vector2(0.5f,0.5f) + windowPos;
                
                //if(!LookAtActor(StorageContainer.ActorAddress))
                //{
                //    ReleaseKeys();
                //    _fishingBotState = FishingBotState.FocusingOnFishAngle;
                //    return;
                //}

                if (MouseOver(targetPos))
                {
                    var loadout = SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food"));
                    if (loadout.Items.Count > 0)
                    {
                        InputHelper.PressKey(Keys.F);
                        ActiveKey = Keys.F;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                        return;
                    }
                    _fishingBotState = FishingBotState.FocusingOnFishAngle;
                    containerOpen = true;
                }
                else
                {
                    //MoveMouse(-200, 0);
                    Text = $"Mouse pos {InputHelper.GetCursorPositionVector2()}";
                }
                return;
            }
            else if (_fishingBotState == FishingBotState.FocusingOnFishAngle)
            {
                //_fishingBotState = FishingBotState.FocusingOnStorage;
                //waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                Text = "Focusing on fish angle";
                var fishAnglePos = GetFishingPosition();
                if (LookAtPoint(fishAnglePos))
                //if (LookAtPoint(fishingAngleMarker.Coords))
                {
                    SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Focusing on fish angle";

                    _fishingBotState = FishingBotState.StartedFishing;
                    ActiveKey = Keys.F;
                }
                return;
            }

            if(StorageContainer != null && SoT_Tool.GetPlayerInventoryCount().Any(i => i.Category.Contains("Food")))
            {
                if(SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food")).Items.Count == 5)
                {
                    _fishingBotState = FishingBotState.FocusingOnStorage;
                    Text = "5 food items in inventory. Focusing on storage";
                    if(SoT_Tool.GetCurrentlyWieldedItemRawName() != "None")
                    {
                        InputHelper.PressKey(Keys.X);
                        ActiveKey = Keys.X;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(300).Ticks;
                        return;
                    }
                    return;
                }
            }

            var serverState = (EFishingRodServerState)rm.ReadByte(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ServerState"));
            if (serverState == EFishingRodServerState.WaitingForFishToBite)
            {
                ReleaseKeys();

                _fishingBotState = FishingBotState.StartedFishing;
                Text = "Fishing";
                ReadFish(ActorAddress);
                waitUntil = DateTime.UtcNow.AddMilliseconds(200).Ticks;
                var fishingfloat = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.FishingFloatActor"));
                if (fishingfloat != 0) 
                {
                    var fishingfloatNamePlate = rm.ReadULong(fishingfloat + (ulong)SDKService.GetOffset("BP_FishingFloat_C.FishingFloatNameplate"));
                    fishName = rm.ReadFString(fishingfloatNamePlate + (ulong)SDKService.GetOffset("FishingFloatNameplateComponent.FishName"));
                    if (fishName != "None" && fishName != "NoStringFound")
                    {
                        Text += $"\n{DateTime.Now} : Fish name {fishName}";

                        if(FishToFish.Any() && !FishToFish.Contains(fishName))
                        {
                            _fishingBotState = FishingBotState.FocusingOnFishAngle;
                            InputHelper.PressKey(Keys.X);
                            ActiveKey = Keys.X;
                            SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Fish name {fishName} not on fish to fish list.";
                            fishName = "None";
                            waitUntil = DateTime.UtcNow.AddMilliseconds(500).Ticks;
                        }
                    }
                }
                
                return;
            }
            else if (serverState == EFishingRodServerState.ReelingInAComedyItem || _fishingBotState == FishingBotState.ComedyItemCaught)//(ActiveKey == Keys.KanaMode)
            {
                var comedyItem = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ComedyItemOnFloat"));

                if (comedyItem == 0)
                {
                    _fishingBotState = FishingBotState.FocusingOnFishAngle;
                    return;
                }

                _fishingBotState = FishingBotState.TakingComedyItem;

                // Comedy item caught?
                InputHelper.PressMouseKey(MouseEventF.LeftDown);
                ActiveKey = Keys.LButton;
                waitUntil = DateTime.UtcNow.AddMilliseconds(12000).Ticks;
                SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Reeling comedy item";
                return;
            }
            else if (_fishingBotState == FishingBotState.TakingComedyItem)//(ActiveKey == Keys.KanaMode)
            {
                ReleaseKeys();

                //var comedyItem = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ComedyItemOnFloat"));
                //if (comedyItem == 0)
                //{
                //    _fishingBotState = FishingBotState.FocusingOnFishAngle;
                //    return;
                //}
                //if(ActiveKey == Keys.LButton)da
                //    InputHelper.PressMouseKey(MouseEventF.LeftUp);
                // Comedy item caught?
                InputHelper.PressKey((byte)Keys.F); // F key
                ActiveKey = Keys.F;
                _fishingBotState = FishingBotState.FocusingOnFishAngle;
                waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                //containerOpen = true;
                Text = $"Taking comedy item\"";
                SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Taking comedy item";
                return;
            }
            else if (_fishingBotState == FishingBotState.TakingComedyItem)//(ActiveKey == Keys.Help)
            {
                var comedyItem = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ComedyItemOnFloat"));

                if(comedyItem > 0)
                {
                    // Comedy item caught?
                    InputHelper.PressKey((byte)Keys.X); // F key
                    ActiveKey = Keys.X;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;
                    SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Putting comedy item away";
                    Text = $"Putting comedy item away";
                    return;
                }
                
                if(comedyItem == 0)
                {
                    _fishingBotState = FishingBotState.FocusingOnFishAngle;
                    Text = $"Focusing on fishangle";
                    return;
                }
                return;
            }
            else if(_fishingBotState == FishingBotState.FishCaught) //(ActiveKey == Keys.Home)
            {
                ReleaseKeys();
                // Try picking up fish
                InputHelper.PressKey((byte)Keys.F); // F key
                ActiveKey = Keys.F;

                waitUntil = DateTime.UtcNow.AddMilliseconds(3000).Ticks;
                Text = $"Picking up fish";
                if(StorageContainer != null && SoT_Tool.GetPlayerInventoryCount().First(i => i.Category.Contains("Food")).Items.Count > 3)
                {
                    _fishingBotState = FishingBotState.FocusingOnStorage;
                }
                else
                {
                    _fishingBotState = FishingBotState.StartedFishing;
                }
                //SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Pressing F for 3 seconds?";
                return;
            }
            else if((serverState == EFishingRodServerState.NotBeingUsed || _fishingBotState == FishingBotState.StartedFishing) && !startedFishing)
            {
                ReleaseKeys();
                ActiveKey = Keys.None;
                if(item.ToLower().Contains("fishing") && item.ToLower().Contains("rod"))
                {
                    InputHelper.PressMouseKey(MouseEventF.LeftDown);
                    Text = $"Throwing fish line";
                    //SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Throwing fishpole line";
                    ActiveKey = Keys.LButton;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                    startedFishing = true;
                    return;
                }
                else
                {
                    InputHelper.PressKey((byte)Keys.K);
                    ActiveKey = Keys.K;
                    waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;

                    Text = $"Wielding fishing rod";
                    return;
                }
            }
            else if (ActiveKey == Keys.F || ActiveKey == Keys.X)
            {
                if (ActiveKey == Keys.F)
                    InputHelper.ReleaseKey((byte)Keys.F);
                else if (ActiveKey == Keys.X)
                    InputHelper.ReleaseKey((byte)Keys.X);

                InputHelper.PressKey((byte)Keys.K);
                ActiveKey = Keys.K;
                waitUntil = DateTime.UtcNow.AddMilliseconds(1000).Ticks;

                Text = $"Wielding fishing rod";
                //SoT_DataManager.InfoLog += $"\n{DateTime.Now} : Wielding fishing pole";

                return;
            }
            else if (ActiveKey == Keys.K)
            {
                InputHelper.ReleaseKey((byte)Keys.K);
                InputHelper.PressMouseKey(MouseEventF.LeftDown);
                Text = $"Throwing fish line";
                //SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Throwing fishpole line";
                ActiveKey = Keys.LButton;
                waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                return;
            }

            if (serverState == EFishingRodServerState.Casting 
                || serverState == EFishingRodServerState.PreparingToCast 
                || serverState == EFishingRodServerState.VerifyingCastLocation)
            {
                Text = $"Fishing";
                SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Fishing";
                waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                if (ActiveKey == Keys.LButton)
                {
                    InputHelper.PressMouseKey(MouseEventF.LeftUp);
                    ActiveKey = Keys.None;
                }
                return;
            }

            if (serverState == EFishingRodServerState.NotBeingUsed)
            {
                if (ActiveKey == Keys.LButton)
                    InputHelper.PressMouseKey(MouseEventF.LeftUp);
                else if (ActiveKey == Keys.RButton)
                    InputHelper.PressMouseKey(MouseEventF.RightUp);
                else if (ActiveKey != Keys.None)
                    InputHelper.ReleaseKey((byte)ActiveKey);
                ActiveKey = Keys.None;
                //SoT_DataManager.InfoLog += $"\nFishing rod not being used";
                this.ShowText = false;
                this.ShowIcon = false;
                //ToDelete = true;
                //FishingBotActive = false;
                //_fishingBotState = FishingBotState.NotFishing;
                //SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Fishing bot is not using fishing rod.";
                //SoT_DataManager.InfoLog += $"Fishing bot powers down.";
                _fishingBotState = FishingBotState.StartedFishing;
                InputHelper.PressMouseKey(MouseEventF.LeftUp);
                waitUntil = DateTime.UtcNow.AddMilliseconds(2000).Ticks;
                return;
            }
            else if (serverState == EFishingRodServerState.ComedyItemCaught)
            {
                //"FishingRod.IsReeling": 2385,
                ReleaseKeys();
                ActiveKey = Keys.None;
                var fishingfloat = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.FishingFloatActor"));
                if (fishingfloat != 0)
                {
                    var fishingfloatNamePlate = rm.ReadULong(fishingfloat + (ulong)SDKService.GetOffset("BP_FishingFloat_C.FishingFloatNameplate"));
                    fishName = rm.ReadFString(fishingfloatNamePlate + (ulong)SDKService.GetOffset("FishingFloatNameplateComponent.FishName"));
                    if(fishName.ToLower().Contains("herring"))
                    {
                        _fishingBotState = FishingBotState.ComedyItemCaught;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(5000).Ticks;
                        return;
                    }
                }

                var comedyItem = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.ComedyItemOnFloat"));
                var fish = rm.ReadULong(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.FishOnLine"));
                if(comedyItem > 0)
                {
                    var comedyItemName = rm.ReadRawname(comedyItem);
                    if(comedyItemName.ToLower().Contains("ingredient"))
                    {
                        _fishingBotState = FishingBotState.ComedyItemCaught;
                        waitUntil = DateTime.UtcNow.AddMilliseconds(5000).Ticks;
                        return;
                    }
                }
                _fishingBotState = FishingBotState.ComedyItemCaught;
                //ActiveKey = Keys.KanaMode;
                InputHelper.PressMouseKey(MouseEventF.LeftDown);
                waitUntil = DateTime.UtcNow.AddMilliseconds(12000).Ticks;
                this.Text = $"Reeling comedy caught item";
                SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Fishing";

            }
            else if (serverState == EFishingRodServerState.FishCaught)
            {
                ReleaseKeys();
                _fishingBotState = FishingBotState.FishCaught;
                this.Text = $"Fish caught";
                SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Fish caught.";
                ActiveKey = Keys.None;
                //ActiveKey = Keys.Home;
                InputHelper.PressMouseKey(MouseEventF.LeftDown);
                waitUntil = DateTime.UtcNow.AddMilliseconds(7000).Ticks;
                return;
            }
            /*
             // Enum Athena.EFishingRodBattlingState
enum class EFishingRodBattlingState : uint8_t {
NotBattling,
Battling_Tiring,
Battling_NotTiring,
Battling_Tired,
EFishingRodBattlingState_MAX,
};
             * */
            var battleState = (EFishingRodBattlingState)rm.ReadByte(ActorAddress + (ulong)SDKService.GetOffset("FishingRod.BattlingState"));
            string battleStateString = "";
            switch (battleState)
            {
                case EFishingRodBattlingState.NotBattling:
                    battleStateString = "Not Battling";
                    break;
                case EFishingRodBattlingState.Battling_Tiring:
                    battleStateString = "Battling Tiring";
                    break;
                case EFishingRodBattlingState.Battling_NotTiring:
                    battleStateString = "Battling Not Tiring";
                    break;
                case EFishingRodBattlingState.Battling_Tired:
                    battleStateString = "Battling Tired";
                    break;
                default:
                    battleStateString = "Unknown";
                    break;
            }

            if (battleState == EFishingRodBattlingState.Battling_Tired) //Battling Tired
            {
                InputHelper.PressMouseKey(MouseEventF.LeftDown);
                ActiveKey = Keys.LButton;
                this.Text = $"{battleStateString} Reeling";
                //SoT_DataManager.InfoLog += $"\n{DateTime.Now} :Reeling.";
            }
            else if (battleState == EFishingRodBattlingState.Battling_NotTiring) //Battling Not Tiring
            {
  //              "FishingMiniGamePlayerInput.Enum.EFishingMiniGamePlayerInputBattlingDirection.BattlingDirection": 0,
              //"FishingMiniGamePlayerInput.Enum.EFishingMiniGamePlayerInputDirection.InputDirection": 0,
              //"FishingRod.BattlingState": 2473,
              //"FishingRod.FishingMiniGamePlayerInput": 2416,

                ulong fishingMiniGamePlayerInput = ActorAddress + (ulong)SDKService.GetOffset("FishingRod.FishingMiniGamePlayerInput");
                var inputDirection = (EFishingMiniGamePlayerInputDirection)rm.ReadByte(fishingMiniGamePlayerInput + (ulong)0);
                var battlingDirection = (EFishingMiniGameEscapeDirection)rm.ReadByte(fishingMiniGamePlayerInput + (ulong)1);
                bool isReeling = rm.ReadBool(fishingMiniGamePlayerInput + (ulong)2);

                //InputHelper.PressMouseKey(MouseEventF.RightDown);
                //return;
                Keys newKey = Keys.None;
                if (ActiveKey == Keys.None)
                    newKey = Keys.A;
                else if (ActiveKey == Keys.W) // W is not used for fishing
                    newKey = Keys.A;
                else if (ActiveKey == Keys.A)
                    newKey = Keys.S;
                else if (ActiveKey == Keys.S)
                    newKey = Keys.D;
                else if (ActiveKey == Keys.D)
                    newKey = Keys.A;

                if (ActiveKey == Keys.LButton)
                    InputHelper.PressMouseKey(MouseEventF.LeftUp);
                else if (ActiveKey == Keys.Right)
                    InputHelper.PressMouseKey(MouseEventF.RightUp);
                else if (ActiveKey != Keys.None)
                    InputHelper.ReleaseKey((byte)ActiveKey);

                ActiveKey = newKey;
                InputHelper.PressKey((byte)ActiveKey);

                this.Text += $" Pressing Nothing";
                if (battlingDirection == EFishingMiniGameEscapeDirection.Left)
                {
                    rm.WriteByte(fishingMiniGamePlayerInput + (ulong)0, (byte)EFishingMiniGamePlayerInputDirection.Right);
                    this.Text += $" Pulling right";
                }
                else if(battlingDirection == EFishingMiniGameEscapeDirection.Right)
                {
                    rm.WriteByte(fishingMiniGamePlayerInput + (ulong)0, (byte)EFishingMiniGamePlayerInputDirection.Left);
                    this.Text += $" Pulling left";
                }
                else if (battlingDirection == EFishingMiniGameEscapeDirection.Away)
                {
                    rm.WriteByte(fishingMiniGamePlayerInput + (ulong)0, (byte)EFishingMiniGamePlayerInputDirection.Towards);
                    this.Text += $" Pulling towards";
                }

                waitUntil = DateTime.UtcNow.AddMilliseconds(200).Ticks;
                this.Text = $"{battleStateString} : ";
                if (ActiveKey == Keys.None)
                    this.Text += $" Pressing Nothing";
                else if (ActiveKey == Keys.W)
                    this.Text += $" Pressing W";
                else if (ActiveKey == Keys.A)
                    this.Text += $" Pressing A";
                else if (ActiveKey == Keys.S)
                    this.Text += $" Pressing S";
                else if (ActiveKey == Keys.D)
                    this.Text += $" Pressing D";
                else
                    this.Text += $" Pressing Unknown(mouse?)";
            }
            else if (battleState == EFishingRodBattlingState.Battling_Tiring) //Battling Tiring
            {
                // Keep holding the current key
                InputHelper.PressKey((byte)ActiveKey);
                //InputHelper.ClickKey(ActiveKey);
                this.Text = $"{battleStateString} : ";
                if (ActiveKey == Keys.None)
                    this.Text += $" Pressing Nothing";
                else if (ActiveKey == Keys.W)
                    this.Text += $" Pressing W";
                else if (ActiveKey == Keys.A)
                    this.Text += $" Pressing A";
                else if (ActiveKey == Keys.S)
                    this.Text += $" Pressing S";
                else if (ActiveKey == Keys.D)
                    this.Text += $" Pressing D";
                else
                    this.Text += $" Pressing Unknown(mouse?)";
            }
            else if (battleState == EFishingRodBattlingState.NotBattling && ActiveKey != Keys.Home) // Not Battling
            {
                // No fish hooked.
                if(ActiveKey != Keys.None)
                {
                    ReleaseKeys();
                    ActiveKey = Keys.None;
                }
                this.Text = $"Awaiting fish";
                ReadFish(ActorAddress);
            }
            //if (battleState == 0)
            //InputHelper.ClickKey(KeyList.W);
        }

        private bool MouseOver(Vector2 target, float precision = 7)
        {
            var mpos = InputHelper.GetCursorPositionVector2();
            var relativeTarget = target - mpos;
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
            if (x > 50)
                x = 1;
            if (y > 50)
                y = 1;
            if (x < -50)
                x = -1;
            if (y < -50)
                y = -1;

            if (Math.Abs(relativeTarget.X) > precision || Math.Abs(relativeTarget.Y) > precision)
            {
                MoveMouse(x, y);
                Text = $"Mouse pos {InputHelper.GetCursorPositionVector2()}";
                //waitUntil = DateTime.UtcNow.AddSeconds(1).Ticks;
                return false;
            }

            return true;
        }
        public static void ReadFish(ulong address)
        {
            if (FishData.Count > 0)
                return;
            var selector = address + (ulong)SDKService.GetOffset("FishingRod.FishSelector");
            var availableFish = rm.ReadULong(selector + (ulong)SDKService.GetOffset("FishingFishSelector.AvailableFish"));
            var availableFishForSpawning = rm.ReadULong(availableFish + (ulong)SDKService.GetOffset("AvailableFishForSpawning.AvailableFishToSelectForSpawning"));
            var count = rm.ReadInt(availableFish + (ulong)SDKService.GetOffset("AvailableFishForSpawning.AvailableFishToSelectForSpawning") + 8);
            //Dictionary<string, Dictionary<string, string>> availableFish = new Dictionary<string, Dictionary<string, string>>();
            
            for (int i = 0; i < count; i++)
            {
                var fishData = new Dictionary<string, string>();
                var fishSpawnData = rm.ReadULong(availableFishForSpawning + (ulong)i * 8);
                var fishName = rm.ReadFName(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.FishName"));
                var BaitType = rm.ReadByte(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.BaitType"));
                var sea = rm.ReadULong(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.Sea"));
                var seaName = rm.ReadRawname(sea);
                var inPool = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InPool"));
                var inStorm = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InStorm"));
                var closeToActiveFortOrSkellyShip = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToActiveFortOrSkellyShip"));
                var closeToShipwreck = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToShipwreck"));
                var timeOfDay = rm.ReadByte(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.TimeOfDay"));

                fishData.Add("FishName", fishName);
                fishData.Add("BaitType", BaitType.ToString());
                fishData.Add("Sea", seaName);
                fishData.Add("InPool", inPool.ToString());
                fishData.Add("InStorm", inStorm.ToString());
                fishData.Add("CloseToActiveFortOrSkellyShip", closeToActiveFortOrSkellyShip.ToString());
                fishData.Add("CloseToShipwreck", closeToShipwreck.ToString());
                fishData.Add("TimeOfDay", timeOfDay.ToString());
                FishData.Add(fishName, fishData);
            }
        }

        private void HackFish()
        {
            /*
             "FishingFishSelector.AvailableFish": 0,
              "FishingRod.BattlingState": 2473,
              "FishingRod.FishSelector": 2080,
              "FishingRod.ServerState": 2384,
              "FishSpawnParamsDataAsset.BaitType": 64,
              "FishSpawnParamsDataAsset.CloseToActiveFortOrSkellyShip": 42,
              "FishSpawnParamsDataAsset.CloseToShipwreck": 44,
              "FishSpawnParamsDataAsset.FishName": 72,
              "FishSpawnParamsDataAsset.FishTypes": 80,
              "FishSpawnParamsDataAsset.InPool": 41,
              "FishSpawnParamsDataAsset.InStorm": 43,
              "FishSpawnParamsDataAsset.Sea": 48,
              "FishSpawnParamsDataAsset.SizeWeights": 96,
              "FishSpawnParamsDataAsset.TimeOfDay": 40,
              "AvailableFishForSpawning.AvailableFishToSelectForSpawning": 40,
            */
            var selector = ActorAddress + (ulong)SDKService.GetOffset("FishingRod.FishSelector");
            var availableFish = rm.ReadULong(selector + (ulong)SDKService.GetOffset("FishingFishSelector.AvailableFish"));
            var availableFishForSpawning = rm.ReadULong(availableFish + (ulong)SDKService.GetOffset("AvailableFishForSpawning.AvailableFishToSelectForSpawning"));
            var count = rm.ReadInt(availableFish + (ulong)SDKService.GetOffset("AvailableFishForSpawning.AvailableFishToSelectForSpawning") + 8);
            Dictionary<string, Dictionary<string, string>> avaliableFish = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < count; i++)
            {
                var fishData = new Dictionary<string, string>();
                var fishSpawnData = rm.ReadULong(availableFishForSpawning + (ulong)i * 8);
                var fishName = rm.ReadFName(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.FishName"));
                var BaitType = rm.ReadByte(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.BaitType"));
                var sea = rm.ReadULong(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.Sea"));
                var seaName = rm.ReadRawname(sea);
                var inPool = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InPool"));
                var inStorm = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InStorm"));
                var closeToActiveFortOrSkellyShip = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToActiveFortOrSkellyShip"));
                var closeToShipwreck = rm.ReadBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToShipwreck"));
                var timeOfDay = rm.ReadByte(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.TimeOfDay"));

                fishData.Add("FishName", fishName);
                fishData.Add("BaitType", BaitType.ToString());
                fishData.Add("Sea", seaName);
                fishData.Add("InPool", inPool.ToString());
                fishData.Add("InStorm", inStorm.ToString());
                fishData.Add("CloseToActiveFortOrSkellyShip", closeToActiveFortOrSkellyShip.ToString());
                fishData.Add("CloseToShipwreck", closeToShipwreck.ToString());
                fishData.Add("TimeOfDay", timeOfDay.ToString());
                avaliableFish.Add(fishName, fishData);
                if (fishName == "Shadow Stormfish")
                {
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InPool"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InStorm"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToActiveFortOrSkellyShip"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToShipwreck"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.TimeOfDay"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.Sea"), false);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.BaitType"), false);
                }
                else
                {
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToActiveFortOrSkellyShip"), true);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.BaitType"), true);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.InPool"), true);
                    rm.WriteBool(fishSpawnData + (ulong)SDKService.GetOffset("FishSpawnParamsDataAsset.CloseToShipwreck"), true);
                }
            }
        }
        private bool FaceRotation(Vector3 targetRotation)
        {
            var currentRotation = SoT_Tool.my_coords.GetRotation();
            var relativeTarget = targetRotation - currentRotation;
            if (Math.Abs(relativeTarget.X) > 5 || Math.Abs(relativeTarget.Y) > 5)
            {
                // round vector2 string output to two decimals
                Text = $"Looking for fishing angle {targetRotation.ToString("F2")} cursor at {currentRotation.ToString("F2")} relative target {relativeTarget.ToString("F2")}";

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

                y= y * -1;
                x = x * -1;

                InputHelper.MoveMouse((int)x, (int)y);
                return false;
            }
            return true;
        }

        private bool LookAtPoint(Vector3 point, float accuracy = 7)
        {
            var distance = MathHelper.CalculateDistance(point, SoT_Tool.my_coords);
            //if (distance > 20)
            //{
            //    SoT_DataManager.InfoLog += $"\nFishing bot failed to look at actor. Distance {distance}";
            //    FishingBotActive = false;
            //    _fishingBotState = FishingBotState.NotFishing;
            //    return false;
            //}
            var center = SoT_Tool.GetScreenCenter();
            var target = MathHelper.ObjectTracePointToScreen(point).Value;
            var pos = GetCursorPositionVector2();
            var relativeTarget = target - center;
            if (Math.Abs(relativeTarget.X) > accuracy || Math.Abs(relativeTarget.Y) > accuracy)
            {
                // round vector2 string output to two decimals
                Text = $"Looking for fishing angle {target.ToString("F2")} cursor at {pos.ToString("F2")} relative target {relativeTarget.ToString("F2")}";

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

        private bool LookAtActor(ulong address)
        {
            var rootcompptr = GetRootComponentAddress(address);
            var coords = CoordBuilder(rootcompptr, coord_offset);
            var distance = MathHelper.CalculateDistance(coords, SoT_Tool.my_coords);
            if (distance > 4)
            {
                SoT_DataManager.InfoLog += $"\nFishing bot failed to look at actor. Distance {distance}";
                FishingBotActive = false;
                _fishingBotState = FishingBotState.NotFishing;
                return false;
            }
            var center = SoT_Tool.GetScreenCenter();
            var target = MathHelper.ObjectTracePointToScreen(coords).Value;
            var pos = GetCursorPositionVector2();
            var relativeTarget = target - center;
            var rawname = rm.ReadRawname(address);
            if (Math.Abs(relativeTarget.X) > 5 || Math.Abs(relativeTarget.Y) > 5)
            {
                // round vector2 string output to two decimals
                Text = $"Looking for {rawname} {target.ToString("F2")} cursor at {pos.ToString("F2")} relative target {relativeTarget.ToString("F2")}";

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
            InputHelper.ReleaseKeys();
        }

        public override void DrawGraphics(SoT_Helper.Services.Charm.Renderer renderer)
        {
            if(ToDelete) { return; }

            //if (!bool.Parse(ConfigurationManager.AppSettings["ShowOther"]))
            //    return;

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
                    "Fishing Bot:\n"+Text, Color, 0);
            }
        }

        public override void DrawGraphics(PaintEventArgs renderer)
        {
            if (ToDelete) { return; }

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
                    "Fishing Bot:\n" + Text, Color, 0);
            }
        }
    }
}
