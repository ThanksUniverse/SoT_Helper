using Newtonsoft.Json;
using SoT_Helper.Extensions;
using SoT_Helper.Models;
using SoT_Helper.Models.SDKClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Linq;
using static SoT_Helper.Models.Crews;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SoT_Helper.Services
{
    public struct Coordinates
    {
        public float x;
        public float y;
        public float z;
        public float rot_x;
        public float rot_y;
        public float rot_z;
        public float fov;

        public override string ToString()
        {
            return //base.ToString() + 
@$"PlayerX: {x}
PlayerY:    {y}
PlayerZ:    {z}
CamX:       {rot_x}, CamY: {rot_y}, CamZ: {rot_z}
";
        }

        public Vector3 GetPosition()
        {
            return new Vector3(x, y, z);
        }

        public void SetPosition(Vector3 pos)
        {
            x = pos.X;
            y = pos.Y;
            z = pos.Z;
        }

        public Vector3 GetRotation()
        {
            return new Vector3(rot_x, rot_y, rot_z);
        }

        public void SetRotation(Vector3 rot)
        {
            rot_x = rot.X;
            rot_y = rot.Y;
            rot_z = rot.Z;
        }
    }

    public struct ItemLoadout
    {
        public string Category;
        public int Capacity;
        public List<string> Items;
    }

    public class SoT_Tool
    {
        public static int SOT_WINDOW_W = 2560;
        public static int SOT_WINDOW_H = 1440;

        static string UWORLDPATTERN = "48 8B 05 ? ? ? ? 48 8B 88 ? ? ? ? 48 85 C9 74 06 48 8B 49 70";
        static string GNAMEPATTERN = "48 8B 1D ? ? ? ? 48 85 DB 75 ? B9 08 04 00 00";
        static string GOBJECTPATTERN = "89 0D ? ? ? ? 48 8B DF 48 89 5C 24";

        /*
         uWorldPtr = IgroWidgets::FindPatternExternal(hProcess, gameBaseAddress, 
        reinterpret_cast<const unsigned char*>("\x48\x8B\x0D\x00\x00\x00\x00\x48\x8B\x01\xFF\x90\x00\x00\x00\x00\x48\x8B\xF8\x33\xD2\x48\x8D\x4E"), 
        "xxx????xxxxx????xxxxxxxx");

        GNamesPtr = IgroWidgets::FindPatternExternal(hProcess, gameBaseAddress, 
        reinterpret_cast<const unsigned char*>("\x48\x8B\x3D\x00\x00\x00\x00\x48\x85\xFF\x75\x00\xB9\x00\x00\x00\x00\xE8\x00\x00\x00\x00\x48\x8B\xF8\x48\x89\x44"), "xxx????xxxx?x????x????xxxxxx");

         */

        static string offsetsfile = "offsets.json";
        static string actorsfile = "actors.json";
        static string islandsfile = "islands.json";
        static string ignorefile = "RawnameIgnorePatterns.json";

        static UIntPtr u_world_base;
        static UIntPtr g_object_base;
        static UIntPtr g_name_base;

        public static ulong minmemaddress;
        public static ulong maxmemaddress;
        public static ulong g_name_start_address;
        public static ulong g_objects;
        public static ulong u_level;
        public static ulong world_address;
        public static List<ulong> u_levels;

        private static IntPtr idledisconnect;
        public static ulong islandService;
        public static ulong captainedSessionService;
        public static ulong animationLODParameters;
        public static ulong crewService;
        public static ulong shipService;
        public static ulong allianceService;
        public static ulong u_local_player;

        public static ImmutableDictionary<string, int>? offsets;
        private static ConcurrentDictionary<int, string>? actorNameMapTemp = new ConcurrentDictionary<int, string>();

        public static MemoryReader mem;
        public static Coordinates my_coords;
        public static Wayfinder my_wayfinder;
        public static bool idle_dissconnect = true;
        public static bool useTreasureMapScalingFactor = false;
        public static string PlayerName = "NewPlayer";
        public static ulong PlayerAddress = 0;
        public static Guid LocalPlayerCrewId;
        public static bool GameRunning = false;

        public static bool ReadingAllActors = false;

        public static int DebugActorId = 0;
        public static string DebugRawName = "";

        /*
Features to add:

AItemProxy->ItemInfo->Desc->Title which is an FText.

Track treasure maps and dig spots

AAllianceService
ACaptainedSessionService

AAthenaGameState
UAthenaGameSettings

UPlayerNameService
AEmissaryLevelService
AReapersMarkService

Try testing for ALODActor on islands
UGameplayLODSettings.AnimationLODSettings
FAnimationLODParameters

UPlayerCrewComponent

APawn.LastHitBy -> might help log who killed the player?

        player inventory:
    AthenaCharacter.ItemLoadoutComponent->ItemLoadoutComponent.LoadoutSlots
        ->ItemLoadoutComponentSlots.Slots->


        Gold is:
         AAthenaPlayerController.PlayerWalletComponent
UPlayerWalletComponent.LastKnownBalance
FWalletBalanceItem.CurrencyId = 1
FWalletBalanceItem.Balance


*/
        public static void Init()
        {
            //var files = Assembly.GetExecutingAssembly().GetFiles();
            //var testy = Assembly.GetExecutingAssembly().GetManifestResourceStream("SoT_Helper."+offsetsfile);
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //var files = Directory.GetFiles(executingDirectory);

            if (File.Exists(Path.Combine(executingDirectory, offsetsfile))) 
            {
                using (StreamReader reader = File.OpenText(Path.Combine(executingDirectory, offsetsfile)))
                {
                    //JObject myObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                    string jsonString = reader.ReadToEnd();
                    //JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                    //offsets = jsonDocument.RootElement.EnumerateObject().ToImmutableDictionary(property => property.Name, property =>  property.Value.GetInt32());
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonString);
                    offsets = dict.ToImmutableDictionary();
                }
            }
            //if (File.Exists(Path.Combine(executingDirectory, islandsfile)))
            //{
            //    SoT_DataManager.Islands = JsonConvert.DeserializeObject<List<Island>>(islandsfile);
            //}
            if (File.Exists(Path.Combine(executingDirectory, actorsfile)))
            {
                using (StreamReader reader = File.OpenText(Path.Combine(executingDirectory, actorsfile)))
                {
                    //JObject myObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                    string jsonString = reader.ReadToEnd();
                    JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                    //.DistinctBy(o => o.Value.ToString())
                    SoT_DataManager.ActorName_keys = jsonDocument.RootElement.EnumerateObject().ToDictionary(property => property.Value.ToString(), property => property.Name);
                }
            }
            if (File.Exists(Path.Combine(executingDirectory, ignorefile)))
            {
                using (StreamReader reader = File.OpenText(Path.Combine(executingDirectory, ignorefile)))
                {
                    //JObject myObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                    string jsonString = reader.ReadToEnd();

                    SoT_DataManager.IgnorePatternList = JsonConvert.DeserializeObject<List<string>>(jsonString);
                }
            }
            //if (File.Exists(Path.Combine(executingDirectory, "StorageOffsetMap.json")))
            //{
            //    using (StreamReader reader = File.OpenText(Path.Combine(executingDirectory, "StorageOffsetMap.json")))
            //    {
            //        //JObject myObject = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
            //        string jsonString = reader.ReadToEnd();

            //        SoT_DataManager.RawName_StorageOffset_map = JsonConvert.DeserializeObject<Dictionary<string,int>>(jsonString);
            //    }
            //}
            if (!ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle, out SOT_WINDOW_W, out SOT_WINDOW_H, out int x, out int y))
            {
                if(GameRunning)
                {
                    GameRunning = false;
                }
                //MessageBox.Show("Could not find SoTGame window");
                SoT_DataManager.InfoLog += "Could not find SoTGame window\n";
                return;
            }

            if (offsets == null)
            {
                throw new Exception("Failed to load offsets");
            }

            // Check if Sea of Thieves is running, get the process and find pointers and offsets
            if (System.Diagnostics.Process.GetProcessesByName("SoTGame").Any())
            {
                GameRunning = true;
                var game = System.Diagnostics.Process.GetProcessesByName("SoTGame").First();

                var startadress = game.MainModule.BaseAddress;
                var base_address = (ulong)startadress.ToInt64();
                minmemaddress = base_address;
                var gameModule = game.MainModule;
                maxmemaddress = minmemaddress + (ulong)gameModule.ModuleMemorySize;
                //minmemaddress = 0;
                SigScan sigScan = new SigScan(game, gameModule.BaseAddress, gameModule.ModuleMemorySize);
                
                u_world_base = sigScan.FindPattern(UWORLDPATTERN, 0); //0x00007ff7c218453b //7FF7C218453B
                g_object_base = sigScan.FindPattern(GOBJECTPATTERN, 0); //0x00007ff7c31d0d34
                g_name_base = sigScan.FindPattern(GNAMEPATTERN, 0); //0x00007ff7c30daeda

                var u_world_base_int = u_world_base.ToUInt64();
                var g_object_base_int = g_object_base.ToUInt64();
                var g_name_base_int = g_name_base.ToUInt64();
                var u_world_base_offset = (long)u_world_base.ToUInt64() - (long)base_address;

                mem = new MemoryReader(game);

                var u_world_offset = mem.ReadUInt((UIntPtr)(long)u_world_base.ToUInt64() + 3);
                var g_name_offset = mem.ReadUInt((UIntPtr)g_name_base.ToUInt64() + 3); //110490759
                var gnameptr = g_name_base.ToUInt64() + g_name_offset + 7;
                //= mem.ReadUInt((UIntPtr)g_name);

                var u_world = (ulong)u_world_base.ToUInt64() + (ulong)u_world_offset + 7; //140701787430416
                world_address = mem.ReadULong((UIntPtr)u_world);
                minmemaddress = 5000000;//(world_address/2);

                g_name_start_address = mem.ReadULong((UIntPtr)gnameptr);

                var g_objects_offset = mem.ReadUInt((UIntPtr)(g_object_base.ToUInt64() + 2));
                g_objects = g_object_base.ToUInt64() + g_objects_offset + 22;

                if(bool.Parse(ConfigurationManager.AppSettings["ReadOffsetsFromMemory"]))
                    SDKService.ReadGObjects();

                //var offsetTest = offsets["World.PersistentLevel"];
                u_level = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.PersistentLevel"));  // 1734309104640 //offsets["World.PersistentLevel"]

                var levels_ptr = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.Levels")); //offsets["World.Levels"]);
                var u_level_count = mem.ReadUInt((UIntPtr)world_address + (uint)SDKService.GetOffset("World.Levels")+8); // offsets["World.Levels"] + 8);

                List<ulong> levels = new List<ulong>();
                
                for(int i = 0; i< u_level_count; i++)
                {
                    var nextUlevelAdress = mem.ReadULong((UIntPtr)levels_ptr + (uint)i *8); 
                    levels.Add(nextUlevelAdress);
                }
                u_levels = levels;

                var game_instance = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.OwningGameInstance")); //offsets["World.OwningGameInstance"]);
                var local_player = mem.ReadULong((UIntPtr)game_instance + (uint)SDKService.GetOffset("GameInstance.LocalPlayers")); //offsets["GameInstance.LocalPlayers"]);

                int localplayerCount = mem.ReadInt(game_instance + (ulong)SDKService.GetOffset("GameInstance.LocalPlayers")+8); //(ulong)offsets["GameInstance.LocalPlayers"])+8;

                u_local_player = mem.ReadULong(local_player); //1734301543424
                var playercontroller = mem.ReadULong((UIntPtr)u_local_player + (uint)SDKService.GetOffset("Player.PlayerController")); //offsets["Player.PlayerController"]); //1734320111616

                //Test();

                //my_coords = CoordBuilder(u_local_player, 120, true, true);
                GetMyCoordinates();
                if(my_coords.fov == 0)
                {
                    SoT_DataManager.InfoLog += "FoV not found\n";
                    my_coords.fov = 90;
                }

                var pawn = mem.ReadULong((UIntPtr)playercontroller + (uint)SDKService.GetOffset("PlayerController.AcknowledgedPawn")); //+ offsets["PlayerController.AcknowledgedPawn"]);
                var playerstate = mem.ReadULong((UIntPtr)pawn + (uint)SDKService.GetOffset("Pawn.PlayerState")); //offsets["Pawn.PlayerState"]);
                PlayerName = mem.ReadFString(playerstate + (ulong)SDKService.GetOffset("PlayerState.PlayerName")); //offsets["PlayerState.PlayerName"]);

                if (ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle2, out SOT_WINDOW_W, out SOT_WINDOW_H, out int x2, out int y2))
                {
                    SoT_DataManager.InfoLog += $"SoT Found!\nResolution detected as width:{SOT_WINDOW_W} and height:{SOT_WINDOW_H}, FoV seen as {my_coords.fov}\n";
                }
                //var nameptr = mem.ReadULong((UIntPtr)playerstate + offsets["PlayerState.PlayerName"]);
                //var name = mem.ReadString(nameptr, 32);
                //if(name.Any(c => Char.IsLetterOrDigit(c)))
                //    PlayerName = new string(name.Where(c => Char.IsLetterOrDigit(c)).ToArray());
                idledisconnect = (IntPtr)playercontroller + (int)SDKService.GetOffset("OnlineAthenaPlayerController.IdleDisconnectEnabled"); //offsets["OnlineAthenaPlayerController.IdleDisconnectEnabled"];
                idle_dissconnect = mem.ReadBool(idledisconnect);
                //mem.WriteBool((long)playercontroller + 0x1591, false);
                //var test = offsets["OnlineAthenaPlayerController.IdleDisconnectEnabled"];
                //var test2 = 0x1591;

                //var idle_dissconnect2 = mem.ReadBool((ulong)idledisconnect);
                //ReadAllActors();
            }
        }

        public static void ReadRewards()
        {
            ulong gameinstance = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.OwningGameInstance"));
            ulong gamecontext = mem.ReadULong((UIntPtr)gameinstance + (uint)SDKService.GetOffset("AthenaGameInstance.GameContext"));
            ulong ServiceCoordinator = gamecontext + (ulong)SDKService.GetOffset("AthenaGameContext.ServiceCoordinator"); //servicecoordinator->Services array
            ulong services = mem.ReadULong(gamecontext + (ulong)SDKService.GetOffset("AthenaGameContext.ServiceCoordinator")); //servicecoordinator->Services array
            int servicesCount = mem.ReadInt(gamecontext + (ulong)SDKService.GetOffset("AthenaGameContext.ServiceCoordinator") +8);
            int gameservicecoordniatorunitsize = (int)SDKService.GetOffset("GameServiceCoordinatorUnit.Size");
            var rewardservice = mem.ReadULong( services + ((ulong)gameservicecoordniatorunitsize * 4));

            Dictionary<int, Dictionary<string,int>> Rewards = new Dictionary<int, Dictionary<string,int>>();

            var RewardDefinitionAssets = mem.ReadULong((UIntPtr)rewardservice + (uint)SDKService.GetOffset("RewardService.RewardDefinitionAssets"));
            var RewardDefinitionAssetsCount = mem.ReadInt(rewardservice + (ulong)SDKService.GetOffset("RewardService.RewardDefinitionAssets")+8);
            for(int i = 0; i< RewardDefinitionAssetsCount; i++)
            {
                var RewardDefinitionAsset = mem.ReadULong(RewardDefinitionAssets + (ulong)i*8);
                var RewardDefinitions = mem.ReadULong((UIntPtr)RewardDefinitionAsset + (uint)SDKService.GetOffset("RewardDefinitionAsset.RewardDefinitions"));
                var RewardDefinitionsCount = mem.ReadInt(RewardDefinitionAsset + (ulong)SDKService.GetOffset("RewardDefinitionAsset.RewardDefinitions")+8);
                for(int j = 0; j < RewardDefinitionsCount; j++)
                {
                    var rewardDefinition = RewardDefinitions + (ulong)j* (ulong)SDKService.GetOffset("RewardDefinition.Size");
                    var rewardIdentifier = mem.ReadFName(rewardDefinition);
                    var rewardIdentifierId = mem.ReadInt(rewardDefinition);
                    var rewards = mem.ReadULong((UIntPtr)rewardDefinition + (uint)SDKService.GetOffset("RewardDefinition.Rewards"));
                    var rewardsCount = mem.ReadInt(rewardDefinition + (ulong)SDKService.GetOffset("RewardDefinition.Rewards")+8);
                    Rewards.TryAdd(rewardIdentifierId, new Dictionary<string, int>());
                    for (int r = 0; r < rewardsCount; r++)
                    {
                        var reward = mem.ReadULong(rewards + (ulong)r * 8);
                        var rewardType = mem.ReadRawname(reward);
                        if(rewardType == "GoldReward")
                        {
                            int minGold = mem.ReadInt(reward + (ulong)SDKService.GetOffset("GoldReward.MinGold"));
                            int maxGold = mem.ReadInt(reward + (ulong)SDKService.GetOffset("GoldReward.MaxGold"));
                            Rewards[rewardIdentifierId].TryAdd("GoldRewardMin", minGold);
                            Rewards[rewardIdentifierId].TryAdd("GoldRewardMax", minGold);
                        }
                        else if(rewardType == "XPReward")
                        {
                            int XP = mem.ReadInt(reward + (ulong)SDKService.GetOffset("XPReward.Xp"));
                            Rewards[rewardIdentifierId].TryAdd("XPReward", XP);
                        }
                        else if(rewardType == "SeasonXPReward")
                        {
                            int SeasonXP = mem.ReadInt(reward + (ulong)SDKService.GetOffset("SeasonXPReward.SeasonXP"));
                            Rewards[rewardIdentifierId].TryAdd("SeasonXPReward", SeasonXP);
                        }
                    }
                }
            }
            SoT_DataManager.Rewards = Rewards;
        }

        public static int GetRewardGoldValue(ulong bootyInfoAddress)
        {
            var rewardId = mem.ReadInt(bootyInfoAddress + (ulong)SDKService.GetOffset("BootyItemInfo.HandInRewardId"));
            if (rewardId == 0)
            {
                var companyrewardArray = mem.ReadULong(bootyInfoAddress + (ulong)SDKService.GetOffset("BootyItemInfo.HandInRewardIdCompanySpecific"));
                var companyrewardArrayCount = mem.ReadInt(bootyInfoAddress + (ulong)SDKService.GetOffset("BootyItemInfo.HandInRewardIdCompanySpecific") + 8);
                for (int i = 0; i < companyrewardArrayCount; i++)
                {
                    var companyreward = companyrewardArray + (ulong)i * SDKService.GetOffset("CompanySpecificBootyReward.Size");
                    var companyrewardId = mem.ReadInt(companyreward + (ulong)SDKService.GetOffset("CompanySpecificBootyReward.RewardId"));
                    if (companyrewardId == 0)
                        continue;
                    rewardId = companyrewardId;
                    break;
                }
            }
            var gold = GetRewardGoldValue(rewardId);
            return gold;
        }

        public static int GetRewardGoldValue(int rewardId)
        {
            int gold = 0;
            if(SoT_DataManager.Rewards.ContainsKey(rewardId))
            {
                if (SoT_DataManager.Rewards[rewardId].ContainsKey("GoldRewardMin"))
                {
                    var min = SoT_DataManager.Rewards[rewardId]["GoldRewardMin"];
                    var max = SoT_DataManager.Rewards[rewardId]["GoldRewardMax"];
                    gold = (min + max) / 2;
                    return gold;
                }
            }
            return 0;
        }

        public static void FullReset()
        {
            SoT_DataManager.DisplayObjects.Clear();
            SoT_DataManager.IgnoreActors.Clear();
            SoT_DataManager.IgnoreBasicActors.Clear();
            SoT_DataManager.Actors.Clear();
            SoT_DataManager.Actor_name_map.Clear();
            SoT_DataManager.RawName_StorageOffset_map.Clear();
            SoT_Tool.LocalPlayerCrewId = Guid.Empty;
            Ship.PlayerShip = null;
            Crews.Reset();
            SoT_DataManager.CrewData = null;
        }

        public static bool IsIdleDisconnectEnabled()
        {
            if(!CharmService.ShowOverlay)
            {
                return false;
            }
            var playercontroller = mem.ReadULong((UIntPtr)u_local_player + (uint)SDKService.GetOffset("Player.PlayerController")); //offsets["LocalPlayer.PlayerController"]);
            idledisconnect = (IntPtr)playercontroller + (int)SDKService.GetOffset("OnlineAthenaPlayerController.IdleDisconnectEnabled"); //offsets["OnlineAthenaPlayerController.IdleDisconnectEnabled"];
            if (idledisconnect == IntPtr.Zero)
                return false;
            idle_dissconnect = mem.ReadBool(idledisconnect);

            return idle_dissconnect;
        }

        public static void UpdateMyCoords()
        {
            if (System.Diagnostics.Process.GetProcessesByName("SoTGame").Any())
            {
                if(SoT_Tool.LocalPlayerCrewId == Guid.Empty)
                {
                    if(Crews.CrewInfoDetails.Count == 1)
                        SoT_Tool.LocalPlayerCrewId = Crews.CrewInfoDetails.First().Key;
                    else
                    {
                        foreach(var crew in Crews.CrewInfoDetails)
                        {
                            if(crew.Value.CrewMembers.Values.Any())
                            {
                                if(SoT_Tool.PlayerName == "NoStringFound")
                                {
                                    if(crew.Value.CrewMembers.Values.Any(x => x != "NoStringFound" && x != ""))
                                    {
                                        SoT_Tool.PlayerName = crew.Value.CrewMembers.Values.First(x => x != "NoStringFound" && x != "");
                                        SoT_Tool.LocalPlayerCrewId = crew.Key;
                                    }
                                }
                                else if(crew.Value.CrewMembers.Values.Any(x => x == SoT_Tool.PlayerName))
                                {
                                    SoT_Tool.LocalPlayerCrewId = crew.Key;
                                    break;
                                }
                            }
                        }
                    }
                }

                if(SoT_Tool.PlayerAddress == 0)
                {
                    if(SoT_DataManager.Actors.Any(x => x.Value.RawName == "BP_PlayerPirate_C"))
                        SoT_Tool.PlayerAddress = SoT_DataManager.Actors.FirstOrDefault(x => x.Value.RawName == "BP_PlayerPirate_C").Key;
                }
                if (SoT_Tool.PlayerName == "NoStringFound")
                {
                    foreach (var crew in Crews.CrewInfoDetails)
                        if (crew.Value.CrewMembers.Values.Any(x => x != "NoStringFound" && x != ""))
                        {
                            SoT_Tool.PlayerName = crew.Value.CrewMembers.Values.First(x => x != "NoStringFound" && x != "");
                        }
                }
                //my_coords = CoordBuilder(u_local_player, 120, true, true);
                GetMyCoordinates();
                if(my_coords.fov == 0)
                    my_coords.fov = 90;
            }
        }

        public static bool ChangeIdleDisconnect()
        {
            if(idledisconnect == IntPtr.Zero)
            {
                var u_world_offset = mem.ReadUInt((UIntPtr)(long)u_world_base.ToUInt64() + 3);
                var u_world = (ulong)u_world_base.ToUInt64() + (ulong)u_world_offset + 7; //140701787430416
                var world_address = mem.ReadULong((UIntPtr)u_world);
                var game_instance = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.OwningGameInstance")); //offsets["World.OwningGameInstance"]);
                var local_player = mem.ReadULong((UIntPtr)game_instance + (uint)SDKService.GetOffset("GameInstance.LocalPlayers")); //offsets["GameInstance.LocalPlayers"]);
                var u_local_player = mem.ReadULong(local_player); //1734301543424
                var playercontroller = mem.ReadULong((UIntPtr)u_local_player + (uint)SDKService.GetOffset("Player.PlayerController")); //offsets["Player.PlayerController"]); //1734320111616
                idledisconnect = (IntPtr)playercontroller + (int)SDKService.GetOffset("OnlineAthenaPlayerController.IdleDisconnectEnabled"); //offsets["OnlineAthenaPlayerController.IdleDisconnectEnabled"];
            }
            //var test = offsets["OnlineAthenaPlayerController.IdleDisconnectEnabled"];
            //var test2 = 0x1591;
            bool currentSetting = idle_dissconnect;
            idle_dissconnect = mem.ReadBool(idledisconnect);
            //var idle_dissconnect = mem.ReadBool((IntPtr)playercontroller + 0x1591);

            mem.WriteBool(idledisconnect.ToInt64(), !currentSetting);
            idle_dissconnect = mem.ReadBool(idledisconnect);

            return idle_dissconnect;
        }

        public static async Task ReadAllActors()
        {
            if(!ReadingAllActors)
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                ReadingAllActors =true;
                if(!ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle, out SOT_WINDOW_W, out SOT_WINDOW_H, out int x, out int y))
                {
                    ReadingAllActors = false;
                    CharmService.ShowOverlay = false;
                    if(GameRunning)
                    {
                        GameRunning = false;
                        SoT_DataManager.InfoLog += $"Sea of Thieves game not found!\n";
                        FullReset();
                    }
                    return;
                }
                if(!GameRunning)
                {
                    GameRunning = true;
                    SoT_DataManager.InfoLog += $"Sea of Thieves game found!\n";
                    FullReset();
                    Init();
                }
                try
                {
                    var u_world_offset = mem.ReadUInt((UIntPtr)(long)u_world_base.ToUInt64() + 3);
                    var u_world = (ulong)u_world_base.ToUInt64() + (ulong)u_world_offset + 7; //140701787430416
                    var world_address = mem.ReadULong((UIntPtr)u_world);
                    //var game_instance = mem.ReadULong((UIntPtr)world_address + offsets["World.OwningGameInstance"]);
                    var levels_ptr = mem.ReadULong((UIntPtr)world_address + (uint)SDKService.GetOffset("World.Levels")); //offsets["World.Levels"]);
                    var u_level_count = mem.ReadUInt((UIntPtr)world_address + (uint)SDKService.GetOffset("World.Levels")+8); //offsets["World.Levels"] + 8);

                    if(u_local_player == 0 && u_level_count > 1)
                    {
                        var game_instance = mem.ReadULong((UIntPtr)world_address + SDKService.GetOffset("World.OwningGameInstance")); //offsets["World.OwningGameInstance"]);
                        var local_player = mem.ReadULong((UIntPtr)game_instance + SDKService.GetOffset("GameInstance.LocalPlayers")); //offsets["GameInstance.LocalPlayers"]);
                        u_local_player = mem.ReadULong(local_player); //1734301543424
                    }

                    List<ulong> levels = new List<ulong>();

                    for (int i = 0; i < u_level_count; i++)
                    {
                        var nextUlevelAdress = mem.ReadULong((UIntPtr)levels_ptr + (uint)i * 8);
                        levels.Add(nextUlevelAdress);
                    }
                    u_levels = levels;

                    if(SoT_DataManager.Rewards.Count == 0)
                    {
                        ReadRewards();
                    }
                }
                catch(Exception ex)
                {
                   ReadingAllActors = false;
                    return;
                }
                
                foreach (ulong level in u_levels)
                {
                    try
                    {
                        await ReadActors(level);
                    }
                    catch(Exception ex) 
                    {
                        var debugTest = DebugRawName;
                        var debugTest2 = DebugActorId;
                        //ReadingAllActors = false;
                    }
                    SoT_Tool.DebugActorId = 0;
                    SoT_Tool.DebugRawName = "";
                }

                //var elapsed = stopwatch.Elapsed;
                //SoT_DataManager.InfoLog += $"ReadAllActors took {elapsed.TotalSeconds} seconds\n";
                //stopwatch.Restart();

                try
                {
                    var toRemove = new List<ulong> { };
                    foreach (var actor in SoT_DataManager.IgnoreBasicActors)
                    {
                        try
                        {
                            var actorId = mem.ReadInt((IntPtr)actor.Key + (int)SDKService.GetOffset("Actor.actorId")); //SoT_Tool.offsets["Actor.actorId"]);
                            if (actor.Value.ActorId != actorId)
                            {
                                toRemove.Add(actor.Value.ActorAddress);
                            }
                        }
                        catch
                        {
                            toRemove.Add(actor.Value.ActorAddress);
                        }
                    }
                    //List<ulong> toRemove = SoT_DataManager.Actors.Where(a => mem.ReadInt((IntPtr)a.Key + SoT_Tool.offsets["Actor.actorId"]) != a.Value.ActorId).Select(a => a.Key).ToList();
                    
                    foreach (ulong actor in toRemove)
                    {
                        BasicActor value;
                        SoT_DataManager.IgnoreBasicActors.TryRemove(actor, out value);
                        SoT_DataManager.Actors.TryRemove(actor, out BasicActor value2);
                    }
                    var NotDeleted = SoT_DataManager.DisplayObjects.Where(d => !d.ToDelete).ToList();
                    var Deleted = SoT_DataManager.DisplayObjects.Where(d => d.ToDelete).ToList();
                    foreach (var d in Deleted)
                    {
                        SoT_DataManager.Actors.TryRemove(d.ActorAddress, out BasicActor value);
                    }
                    SoT_DataManager.DisplayObjects = new ConcurrentBag<DisplayObject>(NotDeleted);

                    //elapsed= stopwatch.Elapsed;
                    //SoT_DataManager.InfoLog += $"Cleaning up lists took {elapsed.TotalSeconds} seconds\n";
                    //stopwatch.Stop();
                }
                catch (Exception ex)
                {
                    var test1 = SoT_Tool.DebugActorId;
                    var test2 = SoT_Tool.DebugRawName;
                }
                
                ReadingAllActors = false;
            }
        }

        public static async Task ReadActors(ulong level = 0)
        {
            if (level == 0)
                level = u_level;

            //this.display_objects = new List<DisplayObject>();
            UpdateMyCoords();

            byte[] actor_raw = mem.ReadBytes((UIntPtr)level + 0xa0, 0xC);
            //var actor_data = (BitConverter.ToUInt64(actor_raw, 0), BitConverter.ToInt32(actor_raw, 8));
            //var actor_data = new Tuple<long, long>(BitConverter.ToInt64(actor_raw, 0), BitConverter.ToInt64(actor_raw, 8));
            var actor_data_Item1 = BitConverter.ToUInt64(actor_raw, 0);
            var actor_data_Item2 = BitConverter.ToInt32(actor_raw, 8);
            //var actor_data_Item2_2 = BitConverter.ToUInt32(actor_raw, 8);
            //var actor_data_Item2_3 = BitConverter.ToInt16(actor_raw, 8);

            // Credit @mogistink
            // https://www.unknowncheats.me/forum/members/3434160.html
            // One very large read for all the actors addresses to save us 1000+ reads every read_all
            //byte[] level_actors_raw = mem.ReadBytes(actor_data.Item1, (int)actor_data.Item2 * 8);
            byte[] level_actors_raw = mem.ReadBytes((UIntPtr)actor_data_Item1, (int)actor_data_Item2 * 8);
            if (level_actors_raw.Where(b => b == 0).Count() == level_actors_raw.Length)
                return;

            mem.ThrowExceptions = false;
            //this.server_players = new List<ServerPlayer>();
            for (int x = 0; x < actor_data_Item2; x++)
            {
                // We start by getting the ActorID for a given actor, and comparing
                // that ID to a list of "known" id's we cache in self.actor_name_map
                var raw_name = "";
                ulong actor_address = BitConverter.ToUInt64(level_actors_raw, x * 8);
                if (actor_address == 0)
                    continue;

                //if (SoT_DataManager.Actor_address_map.ContainsKey(actor_address))
                //    continue;
                if (SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == actor_address))
                    continue;
                if (SoT_DataManager.IgnoreBasicActors.Any(iba => iba.Key == actor_address))
                    continue;

                int actor_id = 0;
                try
                {
                    actor_id = mem.ReadInt((IntPtr)actor_address + (int)SDKService.GetOffset("Actor.actorId"));

                    DebugActorId = actor_id;

                    //Ignore actors on the ignore list
                    if (actor_id == 0 || SoT_DataManager.IgnoreActors.ContainsKey(actor_id))
                        continue;

                    if (SoT_DataManager.Actors.ContainsKey(actor_address) && SoT_DataManager.Actors[actor_address].ActorId == actor_id)
                        continue;
                    raw_name = mem.ReadGname(actor_id);
                    DebugRawName = raw_name;
                }
                catch (Exception e)
                {
                    SoT_DataManager.InfoLog += $"Unable to find actor name for actor address: {actor_address} with ActorId {actor_id}\n";
                }
                BasicActor currentActor = new BasicActor();
                try
                {
                    currentActor = new BasicActor() { ActorAddress = actor_address, ActorId = actor_id, RawName = raw_name };
                    if (SoT_DataManager.Actors.ContainsKey(actor_address))
                    {
                        SoT_DataManager.Actors[actor_address] = currentActor;
                    }
                    else
                    {
                        SoT_DataManager.Actors.TryAdd(actor_address, currentActor);
                    }
                    if (!SoT_DataManager.Actor_name_map.ContainsKey(actor_id))
                    {
                        SoT_DataManager.Actor_name_map.TryAdd(actor_id, raw_name);
                        //SoT_DataManager.Actor_name_map[actor_id] = raw_name;
                    }
                }
                catch (Exception ex)
                {
                    SoT_DataManager.InfoLog += "\nFailed to create or add basic actor with rawname: " + raw_name;
                    SoT_DataManager.InfoLog += "\nException thrown: " + ex.Message;
                }

                AddDisplayActorIfRelevant(raw_name, actor_address, actor_id);
            }
        }

        public static void AddDisplayActorIfRelevant(string raw_name, ulong actor_address, int actor_id)
        {
            if (raw_name == "IslandService")
            {
                islandService = actor_address;
                if (SoT_DataManager.Islands == null || !SoT_DataManager.Islands.Any())
                    Task.Run(async () =>
                    {
                        GetAllIslands();
                    });
            }
            else if (raw_name == "BP_TornMap_Wieldable_C")
            {
                //"IslandServiceIslandTextureProviderComponent.UnknownData_C8[0x8]": 200,
                //"BP_TornMap_Wieldable_C.IslandServiceIslandTextureProvider": 2376,
                //ulong IslandServiceIslandTextureProvider = mem.ReadULong(actor_address + (ulong)SoT_Tool.offsets["BP_TornMap_Wieldable_C.IslandServiceIslandTextureProvider"]);
                //ulong IslandServiceIslandTextureProvider2 = mem.ReadULong(IslandServiceIslandTextureProvider);
                //var test = GetActorId(IslandServiceIslandTextureProvider2);
                //"RenderToTextureMapBase.RenderData": 2120,
                ulong RenderData = mem.ReadULong(actor_address + (ulong)SDKService.GetOffset("RenderToTextureMapBase.RenderData")); //SoT_Tool.offsets["RenderToTextureMapBase.RenderData"]);
                int count = mem.ReadInt(actor_address + (ulong)SDKService.GetOffset("RenderToTextureMapBase.RenderData") + 8);
                for (int i = 0; i < count; i++)
                {
                    ulong renderressource = mem.ReadULong(RenderData + (ulong)(i * 32) + (ulong)0);
                    // islandname
                    var fname = mem.ReadFName(renderressource + (ulong)(i * 32) + (ulong)0x28);
                }
            }
            //else if (raw_name == "BP_WorldMarkerManager_C")
            //{
            //    //test
            //}
            else if (raw_name == "ShipService")
            {
                shipService = actor_address;
            }
            else if (raw_name == "CaptainedSessionService")
            {
                captainedSessionService = actor_address;
            }
            //else if (raw_name.EndsWith("_LOD1"))
            //{
            //    //animationLODParameters = actor_address;
            //    var parents = GetActorParentComponents(actor_address);
            //    foreach(var parent in parents)
            //    {
            //        var id = GetActorId(parent);
            //        var raw = mem.ReadGname(id);
            //        TestLOD(parent, raw);
            //    }
            //    var owners = GetActorOwners(actor_address);
            //    foreach (var parent in owners)
            //    {
            //        var id = GetActorId(parent);
            //        var raw = mem.ReadGname(id);
            //        TestLOD(parent, raw);
            //    }
            //    TestLOD(actor_address, raw_name);
            //}
            //else if (raw_name.Contains("GameplayLODSettings"))
            //{
            //    animationLODParameters = actor_address;
            //}
            //else if (raw_name.Contains("AnimationLODParameters"))
            //{
            //    animationLODParameters = actor_address;
            //}
            else if (bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]) && (raw_name == "BP_PlayerBuriedTreasureMap_C" || raw_name == "BP_TreasureMap_C"
                || raw_name == "BP_TreasureMap_DVR_C")) //|| raw_name == "BP_TreasureMap_ItemInfo_XMarksTheSpot_C")
            {
                var map = new TreasureMap(mem, actor_id, actor_address, raw_name);
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == map.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(map);
                }
            }
            else if (raw_name == "BP_RiddleMap_C" || raw_name.ToLower().Contains("riddlemap")) //|| raw_name == "BP_TreasureMap_ItemInfo_XMarksTheSpot_C")
            {
                var map = new RiddleMap(mem, actor_id, actor_address, raw_name);
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == map.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(map);
                }
            }
            else if (raw_name == "BP_Wayfinder_MultiTargetCompass_Wieldable_C" || raw_name.ToLower().Contains("MultiTargetCompass_Wieldable_C")) //|| raw_name == "BP_TreasureMap_ItemInfo_XMarksTheSpot_C")
            {
                if (my_wayfinder != null)
                {
                    my_wayfinder.SetActorAddress(actor_address);
                    if (!SoT_DataManager.DisplayObjects.Any(dob => dob is Wayfinder))
                    {
                        SoT_DataManager.DisplayObjects.Add(my_wayfinder);
                    }
                }
                else
                {
                    my_wayfinder = new Wayfinder(mem, actor_id, actor_address, raw_name);
                    if (!SoT_DataManager.DisplayObjects.Any(dob => dob is Wayfinder))
                    {
                        SoT_DataManager.DisplayObjects.Add(my_wayfinder);
                    }
                }
            }
            else if (raw_name.ToLower().Contains("bp_projectile")) //bool.Parse(ConfigurationManager.AppSettings["ShowProjectiles"]) && 
            {
                var projectile = new Actor(mem, actor_id, actor_address, raw_name);
                projectile.Color = Color.Orange;
                SoT_DataManager.DisplayObjects.Add(projectile);
            }
            else if (raw_name.ToLower() == "bp_cannon_c" || raw_name == "BP_Cannon_ShipPartMMC_C")
            {
                var cannon = new Cannon(mem, actor_id, actor_address, raw_name);
                cannon.Color = Color.Orange;
                SoT_DataManager.DisplayObjects.Add(cannon);
            }
            // If we have Ship ESP enabled in helpers.py, and the name of the
            // actor is in our mapping.py ship_keys object, interpret the actor
            // as a ship
            //if (CONFIG.Get("SHIPS_ENABLED") && SoT_DataManager.Ships.ContainsKey(raw_name))
            else if (SoT_DataManager.Ship_keys.ContainsKey(raw_name)) //bool.Parse(ConfigurationManager.AppSettings["ShowShips"]) && 
            {
                var ship = new Ship(mem, actor_id, (ulong)actor_address, my_coords, raw_name);

                SoT_DataManager.DisplayObjects.Add(ship);
                if (!SoT_DataManager.Ships.Any(s => s.ActorAddress == ship.ActorAddress))
                    SoT_DataManager.Ships.Add(ship);
            }
            else if (raw_name == "BP_SmallMapTable_C" || raw_name == "MapTable_C" || raw_name == "BP_MediumMapTable_C")
            {
                var MapTable = new MapTable(mem, actor_id, actor_address);
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == MapTable.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(MapTable);
                }
            }
            else if (raw_name == "BP_SkeletonPawnBase_C")
            {
                var skeleton = new Skeleton(mem, actor_id, actor_address, raw_name);
                SoT_DataManager.DisplayObjects.Add(skeleton);
            }
            // Storage checks:
            else if ((raw_name.Contains("StorageBarrel")
            || raw_name.Contains("bar_plen")
            || raw_name.Contains("bar_hide")
            || raw_name.Contains("bar_stor")
            || raw_name.Contains("bar_salt")
            || raw_name.Contains("barrel_large")
            || raw_name.Contains("cmn_wooden_crate_")
            || raw_name.Contains("GhostBarrel")
            || raw_name.Contains("FoodBarrel")
            || raw_name.Contains("CannonballBarrel")
            || raw_name.Contains("WoodBarrel")
            || raw_name.Contains("BP_OutpostCrate")
            || raw_name.Contains("BuoyantStorageBarrel")
            //"Storage Crate": "BP_MerchantCrate_AnyItemCrate_ItemInfo_C",
            //|| raw_name == "BP_MerchantCrate_AnyItemCrate_ItemInfo_C"
            || raw_name.Contains("BP_FeatureIslandCrate_cmn_looted_box"))
            && !raw_name.Contains("Grog")
            && !raw_name.Contains("Clue")
            && !raw_name.Contains("Single")
            && !raw_name.Contains("Double")
            //&& !raw_name.Contains("cmn_wooden_crate_01_b")
            && raw_name != "BP_ShipStorageBarrel_C" && raw_name != "BP_ShipStorageBarrelTwo_C"
            && raw_name != "BP_ShipStorageBarrelThree_C"
            && raw_name != "BP_OutpostCrate_bld_shop_wood_box_01_a_C")
            {
                var storageContainer = new StorageContainer(mem, actor_id, actor_address, raw_name);
                SoT_DataManager.DisplayObjects.Add(storageContainer);
                //if (storageContainer.ContainerNodesPtr != null && storageContainer.ContainerNodesPtr > 0)
                //    SoT_DataManager.DisplayObjects.Add(storageContainer);
                //else
                //{
                //    if(!SoT_DataManager.IgnoreBasicActors.ContainsKey(currentActor.ActorAddress))
                //        SoT_DataManager.IgnoreBasicActors.AddOrUpdate(currentActor.ActorAddress, currentActor, (k, v) => currentActor);

                //    //if (!SoT_DataManager.IgnoreActors.ContainsKey(actor_id))
                //    //    SoT_DataManager.IgnoreActors.Add(actor_id, raw_name);
                //}
            }
            else if (raw_name == "BP_PlayerPirate_C") //bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"])
            {
                var pirate = new Player(mem, actor_id, actor_address, raw_name);
                SoT_DataManager.DisplayObjects.Add(pirate);
            }
            else if (raw_name.ToLower().Contains("lorebook")) //bool.Parse(ConfigurationManager.AppSettings["ShowTomes"])
            {
                var lorebook = new Actor(mem, actor_id, actor_address, raw_name);
                lorebook.Color = Color.LightYellow;
                SoT_DataManager.DisplayObjects.Add(lorebook);
            }
            else if (SoT_DataManager.ActorName_keys.ContainsKey(raw_name) // || raw_name.ToLower().Contains("lorebook")) //bool.Parse(ConfigurationManager.AppSettings["ShowOther"])
                || IsPatternMatched(raw_name, SoT_DataManager.ActorName_keys.Keys.ToList()))
            {
                var actor = new Actor(mem, actor_id, (ulong)actor_address, raw_name);
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == actor.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(actor);
                }
            }
            else if (raw_name == "CrewService")
            {
                crewService = actor_address;

                if (SoT_DataManager.CrewData == null || SoT_DataManager.CrewData.ToDelete)
                    SoT_DataManager.CrewData = new Crews(mem, actor_id, (IntPtr)actor_address);
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == SoT_DataManager.CrewData.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(SoT_DataManager.CrewData);
                }
            }
            else if (raw_name == "AllianceService")
            {
                allianceService = actor_address;
            }
            else
            {
                // do something with unmatched actors?

            }
            // If we have the crews data enabled in helpers.py and the name
            // of the actor is CrewService, we create a class based on that Crew
            // data to generate information about people on the server
            // NOTE: This will NOT give us information on nearby players for the
            // sake of ESP
        }

        public static List<BasicActor> GetAllActors()
        {
            if (!ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle, out SOT_WINDOW_W, out SOT_WINDOW_H, out int x, out int y))
            {
                return null;
            }

            var actors = new List<BasicActor>();

            try
            {
                var u_world_offset = mem.ReadUInt((UIntPtr)(long)u_world_base.ToUInt64() + 3);
                var u_world = (ulong)u_world_base.ToUInt64() + (ulong)u_world_offset + 7; //140701787430416
                var world_address = mem.ReadULong((UIntPtr)u_world);
                //var game_instance = mem.ReadULong((UIntPtr)world_address + offsets["World.OwningGameInstance"]);
                var levels_ptr = mem.ReadULong((UIntPtr)world_address + SDKService.GetOffset("World.Levels"));
                var u_level_count = mem.ReadUInt((UIntPtr)world_address + SDKService.GetOffset("World.Levels")+8);

                List<ulong> levels = new List<ulong>();

                for (int i = 0; i < u_level_count; i++)
                {
                    var nextUlevelAdress = mem.ReadULong((UIntPtr)levels_ptr + (uint)i * 8);
                    levels.Add(nextUlevelAdress);
                }
                foreach (ulong level in levels)
                {
                    var newActors = GetActors(level);
                    if (newActors != null)
                        actors.AddRange(newActors);
                }
                actors = actors.DistinctBy(a => a.ActorAddress).ToList();
            }
            catch (Exception ex)
            {
                return actors;
            }
            return actors;
        }

        public static bool IsPatternMatched(string input, List<string> list)
        {
            foreach (string pattern in list)
            {
                // If the pattern contains %, pattern match against the input
                if (pattern.Contains('%'))
                {
                    string patternRegex = pattern.Replace("%", ".*");
                    if (System.Text.RegularExpressions.Regex.IsMatch(input, patternRegex))
                    {
                        return true;
                    }
                }
                // Otherwise, compare the pattern and input directly
                else if (pattern == input)
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetMatch(string input, Dictionary<string,string> list)
        {
            foreach (KeyValuePair<string, string> pattern in list)
            {
                // If the pattern contains %, pattern match against the input
                if (pattern.Key.Contains('%'))
                {
                    string patternRegex = pattern.Key.Replace("%", ".*");
                    if (System.Text.RegularExpressions.Regex.IsMatch(input, patternRegex))
                    {
                        return pattern.Value;
                    }
                }
                // Otherwise, compare the pattern and input directly
                else if (pattern.Key == input)
                {
                    return pattern.Value;
                }
            }
            return "";
        }

        private static List<BasicActor> GetActors(ulong level)
        {
            List<BasicActor> actors = new List<BasicActor>();
            if (level == 0)
                level = u_level;

            byte[] actor_raw = mem.ReadBytes((UIntPtr)level + 0xa0, 0xC);

            var actor_data_Item1 = BitConverter.ToUInt64(actor_raw, 0);
            var actor_data_Item2 = BitConverter.ToInt32(actor_raw, 8);

            // Credit @mogistink
            // https://www.unknowncheats.me/forum/members/3434160.html
            // One very large read for all the actors addresses to save us 1000+ reads every read_all
            //byte[] level_actors_raw = mem.ReadBytes(actor_data.Item1, (int)actor_data.Item2 * 8);
            byte[] level_actors_raw = mem.ReadBytes((UIntPtr)actor_data_Item1, (int)actor_data_Item2 * 8);

            if (level_actors_raw.Where(b => b == 0).Count() == level_actors_raw.Length)
                return actors;
            for (int x = 0; x < actor_data_Item2; x++)
            {
                // We start by getting the ActorID for a given actor, and comparing
                // that ID to a list of "known" id's we cache in self.actor_name_map
                var raw_name = "";
                ulong actor_address = BitConverter.ToUInt64(level_actors_raw, x * 8);
                if (actor_address == 0)
                    continue;

                int actor_id = 0;
                try
                {
                    actor_id = mem.ReadInt((IntPtr)actor_address + (int)SDKService.GetOffset("Actor.actorId")); //offsets["Actor.actorId"]);

                    //Ignore actors on the ignore list
                    if (actor_id == 0)
                        continue;

                    raw_name = mem.ReadGname(actor_id);
                    var currentActor = new BasicActor() { ActorAddress = actor_address, ActorId = actor_id, RawName = raw_name };
                    actors.Add(currentActor);
                }
                catch (Exception e)
                {
                    SoT_DataManager.InfoLog += $"Unable to find actor name for actor address: {actor_address} with ActorId {actor_id}\n";
                }
            }
            return actors;
        }

        

        public static void DumpSkeletonMeshNamesAsJSON()
        {
            string fileName = "SkeletonMeshNames.json";
            Dictionary<string, string> dataList = SoT_DataManager.SkeletonMeshNames;

            // Parse the JSON string into a list
            Dictionary<string, string> existingData = new Dictionary<string, string>();
            // Check if the file exists and load its contents into a string
            string json;
            if (File.Exists(fileName))
            {
                json = File.ReadAllText(fileName);
                // Parse the JSON string into a list
                existingData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                json = "[]";
            }

            // Add the new data to the existing list
            foreach(KeyValuePair<string,string> data in dataList)
            {
                if (!existingData.ContainsKey(data.Key))
                {
                    existingData.Add(data.Key, data.Value);
                }
            }
            SoT_DataManager.SkeletonMeshNames = existingData;
            // Convert the updated list to a JSON string and write it back to the file
            string updatedJson = JsonConvert.SerializeObject(existingData, Formatting.Indented);

            var updatedJson2 = OffsetFinder.OrderJsonLines(updatedJson);
            File.WriteAllLines(fileName, updatedJson2);
        }

        public static void DumpRawNamesAsJSON()
        {
            string fileName = "RawnameDump.json";
            List<string> dataList = SoT_DataManager.ActorName_List.Where(a => !SoT_DataManager.IgnoreActors.ContainsKey(a.Key)).Select(a => a.Value).ToList();
            // Check if the file exists and load its contents into a string
            string json;
            if (File.Exists(fileName))
            {
                json = File.ReadAllText(fileName);
            }
            else
            {
                json = "[]";
            }

            // Parse the JSON string into a list
            List<string> existingData = JsonConvert.DeserializeObject<List<string>>(json);

            // Add the new data to the existing list
            existingData.AddRange(dataList);
            existingData = existingData.Distinct().ToList();

            // Convert the updated list to a JSON string and write it back to the file
            string updatedJson = JsonConvert.SerializeObject(existingData);
            File.WriteAllText(fileName, updatedJson);

            fileName = "IgnoreDump.json";
            dataList = SoT_DataManager.IgnoreActors.Select(a => a.Value).ToList();

            // Check if the file exists and load its contents into a string
            if (File.Exists(fileName))
            {
                json = File.ReadAllText(fileName);
            }
            else
            {
                json = "[]";
            }

            // Parse the JSON string into a list
            existingData = JsonConvert.DeserializeObject<List<string>>(json);

            // Add the new data to the existing list
            existingData.AddRange(dataList);
            existingData = existingData.Distinct().ToList();

            // Convert the updated list to a JSON string and write it back to the file
            updatedJson = JsonConvert.SerializeObject(existingData);
            File.WriteAllText(fileName, updatedJson);
        }

        public static void TestLOD(ulong address, string rawname)
        {
            try
            {
                float drawDistance = mem.ReadFloat(address + (ulong)SDKService.GetOffset("LODActor.LODDrawDistance")); //SoT_Tool.offsets["LODActor.LODDrawDistance"]);
                var LodLevel = mem.ReadInt(address + (ulong)SDKService.GetOffset("LODActor.LodLevel")); //SoT_Tool.offsets["LODActor.LodLevel"]);
                var SubActors = mem.ReadULong(address + (ulong)SDKService.GetOffset("LODActor.SubActors")); //SoT_Tool.offsets["LODActor.SubActors"]);
                var SubActorsCount = mem.ReadInt(address + (ulong)SDKService.GetOffset("LODActor.SubActors")+8); //SoT_Tool.offsets["LODActor.SubActors"] + 8);
                var SubObjects = mem.ReadULong(address + (ulong)SDKService.GetOffset("LODActor.SubObjects")); //SoT_Tool.offsets["LODActor.SubObjects"]);
                var SubObjectsCount = mem.ReadInt(address + (ulong)SDKService.GetOffset("LODActor.SubObjects")+8); //SoT_Tool.offsets["LODActor.SubObjects"] + 8);

                if (drawDistance > 0.000001f && drawDistance < 10000 && LodLevel >= 0 && LodLevel < 10000 
                    && SubActorsCount >= 0 && SubActorsCount < 30000
                    && SubObjectsCount >= 0 && SubObjectsCount < 30000)
                {

                }
            }
            catch (Exception e) 
            {
            }
            /*
               "LODActor.LODDrawDistance": 992,
                "LODActor.LodLevel": 996,
                "LODActor.SubActors": 976,
                "LODActor.SubObjects": 1000,
             */
        }

        public static void Test()
        {
            // AthenaGameState-> PersistentUserGameSettings -> ActionBindings > BindingName, KeyName

            //"PlayerController.PlayerInput": 1224,
            //"PlayerInput.DebugExecBindings": 288,
            //"Key.KeyName": 0,
            //"KeyBind.Command": 32,
            //"KeyBind.Key": 0,
            var controller = mem.ReadULong((UIntPtr)u_local_player + SDKService.GetOffset("Player.PlayerController")); //offsets["Player.PlayerController"]);
            var input = mem.ReadULong(controller + (ulong)SDKService.GetOffset("PlayerController.PlayerInput")); //offsets["PlayerController.PlayerInput"]);
            var bindings = mem.ReadULong(input + (ulong)SDKService.GetOffset("PlayerInput.DebugExecBindings")); //offsets["PlayerInput.DebugExecBindings"]);
            var bindingsCount = mem.ReadInt(input + (ulong)SDKService.GetOffset("PlayerInput.DebugExecBindings")+8); //offsets["PlayerInput.DebugExecBindings"]+8);
            for (int i = 0; i< bindingsCount; i++)
            {
                var binding = bindings + (ulong)(i * 56);
                var key = mem.ReadFName(binding + (ulong)SDKService.GetOffset("KeyBind.Key")); //offsets["KeyBind.Key"]);
                var command = mem.ReadFString(binding + (ulong)SDKService.GetOffset("KeyBind.Command")); //offsets["KeyBind.Command"]);
            }
        }

        public static List<ItemLoadout> GetPlayerInventoryCount()
        {
            List<ItemLoadout> itemLoadouts = new List<ItemLoadout>();
            var ItemLoadoutComponent = mem.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + SDKService.GetOffset("AthenaCharacter.ItemLoadoutComponent")); //SoT_Tool.offsets["AthenaCharacter.ItemLoadoutComponent"]);
            var loadoutSlots = mem.ReadULong(ItemLoadoutComponent + 
                (ulong)SDKService.GetOffset("ItemLoadoutComponent.LoadoutSlots") + (ulong)SDKService.GetOffset("ItemLoadoutComponentSlots.Slots")); //SoT_Tool.offsets["ItemLoadoutComponentSlots.Slots"]);
            var loadoutSlotsCount = mem.ReadInt(ItemLoadoutComponent +
                (ulong)SDKService.GetOffset("ItemLoadoutComponent.LoadoutSlots") + (ulong)SDKService.GetOffset("ItemLoadoutComponentSlots.Slots")+8); //SoT_Tool.offsets["ItemLoadoutComponentSlots.Slots"] + 8);
            for (int i = 0; i < loadoutSlotsCount; i++)
            {
                var ItemLoadoutSlot = loadoutSlots + (ulong)i * 32;
                var ItemCategory = mem.ReadULong(ItemLoadoutSlot + (ulong)SDKService.GetOffset("ItemLoadoutSlot.ItemCategory")); //SoT_Tool.offsets["ItemLoadoutSlot.ItemCategory"]);
                var category = mem.ReadRawname(ItemCategory);
                var Items = mem.ReadULong(ItemLoadoutSlot + (ulong)SDKService.GetOffset("ItemLoadoutSlot.Items")); //SoT_Tool.offsets["ItemLoadoutSlot.Items"]);
                var ItemsCount = mem.ReadInt(ItemLoadoutSlot + (ulong)SDKService.GetOffset("ItemLoadoutSlot.Items")+8); //SoT_Tool.offsets["ItemLoadoutSlot.Items"] + 8);
                var itemCapacity = mem.ReadInt(ItemLoadoutSlot + (ulong)SDKService.GetOffset("ItemLoadoutSlot.Capacity")); //SoT_Tool.offsets["ItemLoadoutSlot.Capacity"]);

                List<string> itemsInLoadOut = new List<string>();

                for(int j = 0; j < ItemsCount; j++)
                {
                    var Item = Items + (ulong)j * 8;
                    var ItemAddress = mem.ReadULong(Item);
                    itemsInLoadOut.Add(mem.ReadRawname(ItemAddress));
                }
                itemLoadouts.Add(new ItemLoadout() { Capacity = itemCapacity, Category = category, Items = itemsInLoadOut});
            }
            return itemLoadouts;
        }

        public static List<string> GetActorParentRawnames(ulong actor_address)
        {
            List<ulong> parents = new List<ulong>();
            GetActorOwners(actor_address, ref parents);
            List<string> parentRawNames = parents.Select(p => mem.ReadGname(GetActorId(p))).ToList();
            return parentRawNames;
        }

        public static List<ulong> GetActorParentComponents(ulong actor_address)
        {
            List<ulong> parents = new List<ulong>();
            GetActorParentComponents(actor_address, ref parents);
            return parents;
        }

        public static List<ulong> GetActorChildren(ulong actor_address)
        {
            List<ulong> children = new List<ulong>();
            var child = mem.ReadULong((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.Children")); //offsets["Actor.Children"]);
            var count = mem.ReadInt((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.Children")+8); //offsets["Actor.Children.Count"]);
            for (int i = 0; i < count; i++)
            {
                child = mem.ReadULong(child + (ulong)(8*i));
                children.Add(child);
            }

            return children;
        }

        public static List<Actor> GetActorChildrenAsActors(ulong actor_address)
        {
            List<Actor> children = new List<Actor>();
            var child = mem.ReadULong((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.Children"));
            var count = mem.ReadInt((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.Children")+8);
            for (int i = 0; i < count; i++)
            {
                children.Add(new Actor(mem, child));
                child = mem.ReadULong(child + 8);
            }

            return children;
        }
        /*
         "Actor.ChildComponentActors.Count": 432,
  "Actor.ChildComponentActors": 424,
  "Actor.Children.Count": 352,
  "Actor.Children": 344,
         */

        public static void GetActorParentComponents(ulong actor_address, ref List<ulong> parents)
        {
            var parent = mem.ReadULong((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.ParentComponentActor")); //offsets["Actor.ParentComponentActor"]);
            if (parent > 0)
            {
                parents.Add(parent);
                GetActorParentComponents(parent, ref parents);
            }
        }

        public static List<ulong> GetActorOwners(ulong actor_address)
        {
            List<ulong> owners = new List<ulong>();
            GetActorOwners(actor_address, ref owners);
            return owners;
        }

        public static void GetActorOwners(ulong actor_address, ref List<ulong> parents)
        {
            var parent = mem.ReadULong((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.Owner")); //offsets["Actor.Owner"]);
            if (parent > 0)
            {
                parents.Add(parent);
                GetActorOwners(parent, ref parents);
            }
        }

        public static ulong GetActorParentComponent(ulong actor_address)
        {
            var parent = mem.ReadULong((ulong)actor_address + (ulong)SDKService.GetOffset("Actor.ParentComponentActor")); //offsets["Actor.ParentComponentActor"]);
            //var parent = mem.ReadULong((ulong)actor_address + (ulong)offsets["Actor.Owner"]);
            return parent;
        }

        public static int GetActorId(ulong address)
        {
            /*
            Function to get the AActor's ID, used to validate the ID hasn't changed
            while running a "quick" scan
            :param int address: the base address for a given AActor
            :rtype: int
            :return: The AActors ID
            */
            var id = mem.ReadInt((IntPtr)address + 24); //SoT_Tool.offsets["Actor.actorId"]);
            //var id = mem.ReadInt((IntPtr)address + SDKService.GetOffset("Actor.actorId")); //SoT_Tool.offsets["Actor.actorId"]);
            return id;
        }

        public static async Task<Coordinates> GetActorCoords(ulong address)
        {
            var rootCompPtr = mem.ReadULong(address + (ulong)SDKService.GetOffset("Actor.RootComponent")); //SoT_Tool.offsets["Actor.RootComponent"]);
            Coordinates coords = new Coordinates();
            if (rootCompPtr > 0)
                coords = MathHelper.CoordBuilder(rootCompPtr, (int)SDKService.GetOffset("SceneComponent.ActorCoordinates")); //SoT_Tool.offsets["SceneComponent.ActorCoordinates"]);
            //var distance = MathHelper.CalculateDistance(coords, my_coords);
            await Task.Delay(0);
            return coords;
        }

        public static int GetDistanceFromActor(ulong address)
        {
            var rootCompPtr = mem.ReadULong(address + (ulong)SDKService.GetOffset("Actor.RootComponent")); //SoT_Tool.offsets["Actor.RootComponent"]);
            var coords = MathHelper.CoordBuilder(rootCompPtr, (int)SDKService.GetOffset("SceneComponent.ActorCoordinates")); //SoT_Tool.offsets["SceneComponent.ActorCoordinates"]);
            var distance = MathHelper.CalculateDistance(coords, my_coords);
            return distance;
        }

        public static string GetCurrentlyWieldedItemRawName()
        {
            var wieldeditemcomponent = mem.ReadULong((UIntPtr)SoT_Tool.PlayerAddress + SDKService.GetOffset("AthenaCharacter.WieldedItemComponent")); //SoT_Tool.offsets["AthenaCharacter.WieldedItemComponent"]);
            var currentlywieldeditem = mem.ReadULong((UIntPtr)wieldeditemcomponent + SDKService.GetOffset("WieldedItemComponent.CurrentlyWieldedItem")); //SoT_Tool.offsets["WieldedItemComponent.CurrentlyWieldedItem"]);
            if (currentlywieldeditem == 0)
            {
                return "None";
            }
            var itemname = mem.ReadRawname(currentlywieldeditem);
            return itemname;
        }

        private static void GetAllIslands()
        {
            SoT_DataManager.Islands = new List<Island>();
            var islandDataAsset = mem.ReadULong((ulong)SoT_Tool.islandService + (ulong)SDKService.GetOffset("IslandService.IslandDataAsset")); //SoT_Tool.offsets["IslandService.IslandDataAsset"]);
            //islandDataAsset = 0;
            var islandDataEntriesArray = mem.ReadULong((ulong)islandDataAsset + (ulong)SDKService.GetOffset("IslandDataAsset.IslandDataEntries")); //SoT_Tool.offsets["IslandDataAsset.IslandDataEntries"]);
            var islandDataEntriesCount = mem.ReadInt((IntPtr)islandDataAsset + (int)SDKService.GetOffset("IslandDataAsset.IslandDataEntries") +8); //SoT_Tool.offsets["IslandDataAsset.IslandDataEntries"] + 8);

            //List<IslandInfo> list = new List<IslandInfo>();

            for (int i = 0; i < islandDataEntriesCount; i++)
            {
                var islandDataEntry = mem.ReadULong((UIntPtr)islandDataEntriesArray + (uint)i * 8);

                var island = new Island(mem, islandDataEntry);

                SoT_DataManager.Islands.Add(island);
                //IslandInfo islandInfo = new IslandInfo() { Rawname = island.Rawname, Position = new PointF(island.Coords.x, island.Coords.y) };
                //list.Add(islandInfo);
            }
            //var json = JsonConvert.SerializeObject(list);
            //File.WriteAllText("islands.json", json);
        }
        public static Coordinates GetMyCoordinates()
        {
            if(u_local_player == 0)
            {
                var game_instance = mem.ReadULong((UIntPtr)world_address + SDKService.GetOffset("World.OwningGameInstance")); //offsets["World.OwningGameInstance"]);
                var local_player = mem.ReadULong((UIntPtr)game_instance + SDKService.GetOffset("GameInstance.LocalPlayers")); //offsets["GameInstance.LocalPlayers"]);

                int localplayerCount = mem.ReadInt(game_instance + (ulong)SDKService.GetOffset("GameInstance.LocalPlayers") + 8); //(ulong)offsets["GameInstance.LocalPlayers"])+8;

                u_local_player = mem.ReadULong(local_player);
            }
            var controller = mem.ReadULong((UIntPtr)u_local_player + SDKService.GetOffset("Player.PlayerController")); //offsets["Player.PlayerController"]);
            var manager = mem.ReadULong(controller + (ulong)SDKService.GetOffset("PlayerController.PlayerCameraManager")); //SoT_Tool.offsets["PlayerController.PlayerCameraManager"]);
            var coords = MathHelper.CoordBuilder(manager, (int)SDKService.GetOffset("PlayerCameraManager.CameraCache") + (int)SDKService.GetOffset("CameraCacheEntry.POV"), true, true);
            my_coords = coords;
            return coords;
        }
        

        public static Vector2 GetScreenCenter()
        {
            return new Vector2(SOT_WINDOW_W / 2, SOT_WINDOW_H / 2);
        }

        

        //public static int CalculateDistance(Coordinates obj_to, Coordinates obj_from)
        //{
        //    double dx = obj_to["x"] - obj_from["x"];
        //    double dy = obj_to["y"] - obj_from["y"];
        //    double dz = obj_to["z"] - obj_from["z"];
        //    double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        //    return (int)Math.Round(distance);
        //}
    }
}
