using Newtonsoft.Json;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoT_Helper.Forms
{
    public partial class KeybindingsForm : Form
    {
        public static Dictionary<string, Action> availableActions = new Dictionary<string, Action>();  // The list of available actions
        private CheckedListBox actionList = new CheckedListBox();
        public static string keybindingSettingsFile = "Keybindings.json";
        private DataGridView dataGridView;

        public KeybindingsForm()
        {
            InitializeComponent();
            LoadKeybindings();

            dataGridView = new DataGridView
            {
                Dock = DockStyle.Left,
                Width = this.ClientSize.Width / 2,
                AutoGenerateColumns = false,
                DataSource = SoT_DataManager.KeyBindings
            };

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Action",
                DataPropertyName = "Action"
            });

            dataGridView.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "Key",
                DataSource = Enum.GetValues(typeof(Keys)),
                DataPropertyName = "Key"
            });

            dataGridView.SelectionChanged += DataGridView_SelectionChanged;

            CreateActions();

            // Setup the CheckedListBox
            actionList.Dock = DockStyle.Right;
            actionList.Width = this.ClientSize.Width / 2;
            actionList.Items.AddRange(availableActions.Keys.ToArray());
            actionList.ItemCheck += ActionList_ItemCheck;

            // Add the DataGridView and CheckedListBox to your form
            Controls.Add(dataGridView);
            Controls.Add(actionList);
            Controls.Add(menuStrip1);
            enableKeybindingsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["EnableKeybindings"]);
        }

        public static void CreateActions()
        {
            availableActions.TryAdd("Activate Fishing Bot", () => { SoTHelper.RunFishingBot(); });
            availableActions.TryAdd("Activate Cooking Bot", () => { SoTHelper.RunCookingBot(); });
            availableActions.TryAdd("Toggle Ships", () => { SoTHelper.ToggleShips(); });
            availableActions.TryAdd("Toggle Ship Status", () => { SoTHelper.ToggleShipStatus(); });
            availableActions.TryAdd("Toggle Overlay", () => { SoTHelper.ToggleOverlay(); });
            availableActions.TryAdd("Toggle TreasureMap", () => { SoTHelper.ToggleTreasureMap(); });
            availableActions.TryAdd("Toggle Riddles", () => { SoTHelper.ToggleRiddles(); });
            availableActions.TryAdd("Toggle Compass", () => { SoTHelper.ToggleCompass(); });
            availableActions.TryAdd("Toggle Crosshair", () => { SoTHelper.ToggleCrosshair(); });
            availableActions.TryAdd("Toggle Map Pins", () => { SoTHelper.ToggleMapPins(); });
            availableActions.TryAdd("Toggle Crew Tracking", () => { SoTHelper.ToggleCrewTracking(); });
            availableActions.TryAdd("Toggle Container Items", () => { SoTHelper.ToggleContainerItems(); });
            availableActions.TryAdd("Toggle Players", () => { SoTHelper.TogglePlayers(); });
            availableActions.TryAdd("Toggle Player Tracelines", () => { SoTHelper.ToggleEnemyPlayerTracelines(); });
            availableActions.TryAdd("Toggle Contrainers", () => { SoTHelper.ToggleContainers(); });
            availableActions.TryAdd("Toggle Other", () => { SoTHelper.ToggleOther(); });
            availableActions.TryAdd("Reset all app data", () => { SoT_Tool.FullReset(); });
            availableActions.TryAdd("Restart app", () => { SoTHelper.RestartApp(); });
            availableActions.TryAdd("Toggle Tracking All Islands", () => { SoTHelper.ToggleAllIslands(); });
            availableActions.TryAdd("Toggle Tracking All Seaposts", () => { SoTHelper.ToggleAllSeaposts(); });
            availableActions.TryAdd("Toggle Tracking All Outposts", () => { SoTHelper.ToggleAllOutposts(); });
            availableActions.TryAdd("Toggle Island Tracking Range", () => { SoTHelper.ToggleIslandTrackingRange(); });
        }

        private void KeybindingsForm_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void DataGridView_SelectionChanged(object sender, EventArgs e)
        {
            actionList.ItemCheck -= ActionList_ItemCheck;

            DataGridView dataGridView = (DataGridView)sender;
            if (dataGridView.CurrentRow == null || dataGridView.CurrentRow.DataBoundItem == null)
            {
                for (int i = 0; i < actionList.Items.Count; i++)
                {
                    actionList.SetItemChecked(i, false);
                }
                return;
            }

            KeyBinding keyBinding = (KeyBinding)dataGridView.CurrentRow.DataBoundItem;

            for (int i = 0; i < actionList.Items.Count; i++)
            {
                string actionName = (string)actionList.Items[i];
                actionList.SetItemChecked(i, keyBinding.Delegates.Contains(availableActions[actionName]));
            }
            actionList.ItemCheck += ActionList_ItemCheck;
        }

        private void ActionList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (dataGridView.CurrentRow == null)
                return;

            KeyBinding keyBinding = (KeyBinding)dataGridView.CurrentRow.DataBoundItem;
            string actionName = (string)actionList.Items[e.Index];
            Action action = availableActions[actionName];

            if (e.NewValue == CheckState.Checked)
            {
                if (!keyBinding.Delegates.Contains(action))
                {
                    keyBinding.Delegates.Add(action);
                }
            }
            else
            {
                if (keyBinding.Delegates.Contains(action))
                {
                    keyBinding.Delegates.Remove(action);
                }
            }
        }

        private void saveKeybindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var savedBindings = SoT_DataManager.KeyBindings.Select(b =>
                new SerializableKeyBinding()
                {
                    Key = b.Key,
                    Action = b.Action,
                    DelegateIdentifiers = b.Delegates.Select(d => availableActions.First(a => a.Value == d).Key).ToList()
                }).ToList();

            // Parse the keybindings data to a json string
            var json = JsonConvert.SerializeObject(savedBindings);

            File.WriteAllText(keybindingSettingsFile, json);
        }

        public static void LoadKeybindings()
        {
            CreateActions();
            string json;
            if (File.Exists(keybindingSettingsFile))
            {
                json = File.ReadAllText(keybindingSettingsFile);
                // Parse the JSON string into a list
                //SoT_DataManager.KeyBindings = JsonConvert.DeserializeObject<BindingList<KeyBinding>>(json);
                var keyBindings = JsonConvert.DeserializeObject<List<SerializableKeyBinding>>(json);

                if(keyBindings != null && keyBindings.Any())
                {
                    SoT_DataManager.KeyBindings = new BindingList<KeyBinding>(keyBindings.Select(b =>
                    new KeyBinding()
                    {
                        Key = b.Key,
                        Action = b.Action,
                        Delegates = b.DelegateIdentifiers.Select(d => availableActions.First(a => a.Key == d).Value).ToList()
                    }).ToList());
                }
            }
        }

        private void loadKeybindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadKeybindings();
        }

        private void enableKeybindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableKeybindingsToolStripMenuItem.Checked = !enableKeybindingsToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["EnableKeybindings"] = enableKeybindingsToolStripMenuItem.Checked ? "True" : "False";
            if (enableKeybindingsToolStripMenuItem.Checked)
                InterceptKeys.RunKeyInterception();
            else
                InterceptKeys.Unload();
        }
    }

    public class KeyBinding
    {
        public string Action { get; set; }
        public Keys Key { get; set; }
        public List<Action> Delegates { get; set; } = new List<Action>();
    }

    public class SerializableKeyBinding
    {
        public string Action { get; set; }
        public Keys Key { get; set; }
        public List<string> DelegateIdentifiers { get; set; }
    }
}
