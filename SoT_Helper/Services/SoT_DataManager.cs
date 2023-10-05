using SoT_Helper.Forms;
using SoT_Helper.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Services
{
    public static class SoT_DataManager
    {
        public static BindingList<KeyBinding> KeyBindings { get; set; } = new BindingList<KeyBinding>();

        public static List<Ship> Ships { get; set; } = new List<Ship>();
        //public static Dictionary<Guid, Ship> Ships { get; set; } = new Dictionary<Guid, Ship>();
        
        public static ConcurrentDictionary<int, string> Actor_name_map { get; set; } = new ConcurrentDictionary<int, string>();
        public static Dictionary<int, string> IgnoreActors { get; set; } = new Dictionary<int, string>();
        public static ConcurrentDictionary<ulong, BasicActor> IgnoreBasicActors { get; set; } = new ConcurrentDictionary<ulong, BasicActor>();
        //public static Dictionary<int, int> ActorId_StorageOffset_map { get; set; } = new Dictionary<int, int>();
        public static Dictionary<string, int> RawName_StorageOffset_map { get; set; } = new Dictionary<string, int>();
        public static ConcurrentDictionary<ulong, BasicActor> Actors { get; set; } = new ConcurrentDictionary<ulong, BasicActor>();
        public static Crews CrewData { get; set; }
        public static ConcurrentDictionary<ulong, Coordinates> Actor_Coordinates_map { get; set; } = new ConcurrentDictionary<ulong, Coordinates>();
        public static List<Island> Islands { get; set; }

        public static ConcurrentDictionary<int, string> NotStorage { get; set; } = new ConcurrentDictionary<int, string>();

        // Stores all objects that need to be displayed on the overlay
        public static ConcurrentBag<DisplayObject> DisplayObjects { get; set; } = new ConcurrentBag<DisplayObject>();

        // Maps rawnames to their display names for the "other" category based on the actors.json file
        public static Dictionary<string, string> ActorName_keys = new Dictionary<string, string>();
        public static List<KeyValuePair<int, string>> ActorName_List = new List<KeyValuePair<int, string>>();
        public static Dictionary<string, string> SkeletonMeshNames = new Dictionary<string, string>();
        public static Dictionary<int, Dictionary<string, int>> Rewards = new Dictionary<int, Dictionary<string, int>>();
        public static List<string> IgnorePatternList = new List<string>();

        public static Dictionary<string, string> Ship_keys = new Dictionary<string, string>()
        {
            // ------------ SHIPS / AI SHIPS ------------
            {"BP_SmallShipTemplate_C", "Sloop (Near)"},
            {"BP_SmallShipNetProxy_C", "Sloop"},
            {"BP_MediumShipTemplate_C", "Brig (Near)"},
            {"BP_MediumShipNetProxy_C", "Brig"},
            {"BP_LargeShipTemplate_C", "Galleon (Near)"},
            {"BP_LargeShipNetProxy_C", "Galleon"},
            {"BP_AISmallShipTemplate_C", "Skeleton Sloop (Near)"},
            {"BP_AISmallShipNetProxy_C", "Skeleton Sloop"},
            {"BP_AILargeShipTemplate_C", "Skeleton Galleon (Near)"},
            {"BP_AILargeShipNetProxy_C", "Skeleton Galleon"},
        };

        public static string InfoLog { get; set; }

    }
}
