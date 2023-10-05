using SoT_Helper.Models;
using SoT_Helper.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Configuration;
using System.Collections.Generic;
using static System.Windows.Forms.DataFormats;
using SoT_Helper.Forms;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SoT_Helper
{
    public partial class SoTHelper : Form
    {
        bool textFilter = false;
        bool rangeFilter = false;
        public static Task thread;
        public static SoTHelper Instance { get; set; }

        public SoTHelper()
        {
            Instance = this;
            InitializeComponent();

            RefreshUISettings();

            SoT_Tool.Init();
            var task = Task.Run(async () =>
            {
                await SoT_Tool.ReadAllActors();
            });
            if (SoT_Tool.idle_dissconnect)
                IdleDC_Button.Text = "Disable IdleDisconnect";
            else
                IdleDC_Button.Text = "Enable IdleDisconnect";

            if (bool.Parse(ConfigurationManager.AppSettings["StartOverlayOnLaunch"]))
            {
                if (Process.GetProcessesByName("SoTGame").Any())
                {
                    if (bool.Parse(ConfigurationManager.AppSettings["FormsOverlay"]))
                    {
                        OverlayForm.ShowOverlay = true;
                        var overlay = new OverlayForm();
                        OverlayForm.Instance = overlay;
                        overlay.Show();
                        if (OverlayForm.ShowOverlay)
                            AddOverlayButton.Text = "Disable Overlay";
                    }
                    else
                    {
                        CharmService.ShowOverlay = true;
                        CharmService.RunCharm();
                        if (CharmService.ShowOverlay)
                            AddOverlayButton.Text = "Disable Overlay";
                    }
                }
            }
        }

        public void RefreshUISettings()
        {
            showStorageToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowContainers"]);
            showStorageItemsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowContainerItems"]);
            showPlayersToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"]);
            onlyEnemyPlayersToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["HideFriendlyPlayers"]);
            enemyPlayerTracelinesToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowPlayerTracelines"]);

            compassToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowCompass"]);
            crosshairToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowCrosshair"]);

            showShipsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowShips"]);
            lessShipInfoToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["LessShipInfo"]);
            showShipStatusToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]);
            showItemsOnYourShipToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowItemsOnShips"]);
            showResourcesOfShipsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowResourcesOnShips"]);
            showMapPinsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowMapPins"]);
            showMapShipsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowMapShips"]);
            showMapTreasureToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowMapLoot"]);
            showOtherStuffToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowOther"]);
            showSkeletonsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowSkeletons"]);
            showProjectilesToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowProjectiles"]);
            showCrewsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowCrews"]);
            showTreasureSpotsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]);
            showRiddleSpotsToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["TrackRiddleSpots"]);
            showTomesToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowTomes"]);
            startOverlayOnLaunchToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["StartOverlayOnLaunch"]);
            showCannonTrajectoryToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"]);
            readOffsetsFromMemoryToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["ReadOffsetsFromMemory"]);
            useFormsOverlayToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["FormsOverlay"]);

            keepIdleDisconnectDisabledToolStripMenuItem.Checked =
                bool.Parse(ConfigurationManager.AppSettings["KeepIdleDisconnectDisabled"]);

            displayOverlayFPSToolStripMenuItem.Checked = bool.Parse(ConfigurationManager.AppSettings["OverlayFPS"]);
            SoT_Tool.useTreasureMapScalingFactor = bool.Parse(ConfigurationManager.AppSettings["TreasureMapIslandScaling"]);
            disableTreasureMapScalingToolStripMenuItem.Checked = SoT_Tool.useTreasureMapScalingFactor;

            CharmService.TextSize = int.Parse(ConfigurationManager.AppSettings["TextSize"]);

            if (bool.Parse(ConfigurationManager.AppSettings["EnableKeybindings"]))
            {
                KeybindingsForm.LoadKeybindings();
                InterceptKeys.RunKeyInterception();
            }
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["ShowContainers"].Value = showStorageToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowContainerItems"].Value = showStorageItemsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowPlayers"].Value = showPlayersToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["HideFriendlyPlayers"].Value = onlyEnemyPlayersToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowPlayerTracelines"].Value = enemyPlayerTracelinesToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowCompass"].Value = compassToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowCrosshair"].Value = crosshairToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowShips"].Value = showShipsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["LessShipInfo"].Value = lessShipInfoToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowShipStatus"].Value = showShipStatusToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowItemsOnShips"].Value = showItemsOnYourShipToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowResourcesOnShips"].Value = showResourcesOfShipsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowMapPins"].Value = showMapPinsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowMapShips"].Value = showMapShipsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowMapLoot"].Value = showMapTreasureToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowOther"].Value = showOtherStuffToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowTomes"].Value = showTomesToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowCrews"].Value = showCrewsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowSkeletons"].Value = showSkeletonsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ShowProjectiles"].Value = showProjectilesToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["TrackTreasureMapSpots"].Value = showTreasureSpotsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["TreasureMapIslandScaling"].Value = SoT_Tool.useTreasureMapScalingFactor ? "True" : "False";
            config.AppSettings.Settings["TrackRiddleSpots"].Value = showRiddleSpotsToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["StartOverlayOnLaunch"].Value = startOverlayOnLaunchToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["KeepIdleDisconnectDisabled"].Value = keepIdleDisconnectDisabledToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["OverlayFPS"].Value = displayOverlayFPSToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["ReadOffsetsFromMemory"].Value = readOffsetsFromMemoryToolStripMenuItem.Checked ? "True" : "False";
            config.AppSettings.Settings["EnableKeybindings"].Value = ConfigurationManager.AppSettings["EnableKeybindings"];
            config.AppSettings.Settings["ShowCannonTrajectoryPrediction"].Value = ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"];
            config.AppSettings.Settings["TextSize"].Value = CharmService.TextSize.ToString();
            config.AppSettings.Settings["FormsOverlay"].Value = useFormsOverlayToolStripMenuItem.Checked ? "True" : "False";

            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SDKPath"]))
                config.AppSettings.Settings["SDKPath"].Value = ConfigurationManager.AppSettings["SDKPath"];
            //config.AppSettings.Settings["ShowContainers"].Value = showStorageToolStripMenuItem.Checked ? "True" : "False";
            //ShowResourcesOnShips
            config.Save(ConfigurationSaveMode.Modified, true);

            ConfigurationManager.RefreshSection("appSettings");

            RefreshUISettings();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 10000; // 1000 / 60 = refresh 60 times per second
                                     //300000;//5 minutes

            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Start();
        }
        static int ActorCount = 0;
        private DateTime NextListUpdateTime { get; set; }
        int updateInterval = 15000;

        private void timer1_Tick(object sender, EventArgs e)
        {
            RefreshMyForm();

            if (SoT_DataManager.Actor_name_map.Any() && DateTime.Now > NextListUpdateTime)
            {
                NextListUpdateTime = DateTime.Now.AddMilliseconds(updateInterval);
                if (ActorCount != SoT_DataManager.Actor_name_map.Count)
                {
                    ActorCount = SoT_DataManager.Actor_name_map.Count;
                    UpdateLists();
                }
                if (!CharmService.ShowOverlay)
                    AddOverlayButton.Text = "Enable Overlay";
            }
        }

        private void RefreshMyForm()
        {
            //update form with latest Data
            //if (string.IsNullOrWhiteSpace(SoT_Tool.PlayerName) || SoT_Tool.PlayerName == "NewPlayer")
            //    SoT_Tool.Init();
            //SoT_Tool.UpdateMyCoords();
            if (CharmService.ShowOverlay && Ship.PlayerShip != null)
                MigrationButton.Text = Ship.PlayerShip.GetShipMigrationBasedOnVelocityEnabled() ? "Migration based on velocity: ON" : "Migration based on velocity: OFF";
            richTextBox1.Text =
$"Playername: {SoT_Tool.PlayerName}\n" + SoT_DataManager.InfoLog;

            if (SoT_Tool.IsIdleDisconnectEnabled())
                IdleDC_Button.Text = "Disable IdleDisconnect";
            else
                IdleDC_Button.Text = "Enable IdleDisconnect";

            if (keepIdleDisconnectDisabledToolStripMenuItem.Checked && SoT_Tool.IsIdleDisconnectEnabled())
            {
                SoT_Tool.ChangeIdleDisconnect();
                if (SoT_Tool.IsIdleDisconnectEnabled())
                    IdleDC_Button.Text = "Disable IdleDisconnect";
                else
                    IdleDC_Button.Text = "Enable IdleDisconnect";


            }

            //if (IslandComboBox.DataSource == null && SoT_DataManager.Islands != null && SoT_DataManager.Islands.Any())
            //{
            //    IslandComboBox.DataSource = SoT_DataManager.Islands;
            //    IslandComboBox.DisplayMember = "IslandName";
            //}

            //{SoT_Tool.my_coords.ToString()}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SoT_Tool.Init();

            if (SoT_Tool.idle_dissconnect)
                IdleDC_Button.Text = "Disable IdleDisconnect";
            else
                IdleDC_Button.Text = "Enable IdleDisconnect";

            //            richTextBox1.Text =
            //@$"Playername: {SoT_Tool.PlayerName}" + SoT_DataManager.InfoLog;
            //{SoT_Tool.my_coords.ToString()}
        }

        private void IdleDC_Button_Click(object sender, EventArgs e)
        {
            bool state = SoT_Tool.ChangeIdleDisconnect();
            if (state)
                IdleDC_Button.Text = "Disable IdleDisconnect";
            else
                IdleDC_Button.Text = "Enable IdleDisconnect";
        }

        private void AddOverlayButton_Click(object sender, EventArgs e)
        {
            OverlayToggle();
        }

        public void OverlayToggle()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["FormsOverlay"]))
            {
                if (OverlayForm.Instance == null || OverlayForm.Instance.IsDisposed)
                {
                    OverlayForm.ShowOverlay = true;
                    var overlay = new OverlayForm();
                    OverlayForm.Instance = overlay;
                    overlay.Show();
                    if (OverlayForm.ShowOverlay)
                        AddOverlayButton.Text = "Disable Overlay";
                }
                else if (OverlayForm.ShowOverlay)
                {
                    OverlayForm.ShowOverlay = false;
                    OverlayForm.Instance.Hide();
                    AddOverlayButton.Text = "Enable Overlay";
                }
                else
                {
                    OverlayForm.ShowOverlay = true;
                    OverlayForm.Instance.Show();
                    AddOverlayButton.Text = "Disable Overlay";
                }
            }
            else
            {
                if (CharmService.ShowOverlay)
                {
                    CharmService.ShowOverlay = false;
                    AddOverlayButton.Text = "Enable Overlay";
                }
                else
                {
                    CharmService.RunCharm();
                    CharmService.ShowOverlay = true;
                    AddOverlayButton.Text = "Disable Overlay";
                }
            }


        }

        public static void ToggleOverlay()
        {
            Instance.OverlayToggle();
        }

        private async void TestButton_Click(object sender, EventArgs e)
        {
            //var names = SoT_Tool.ReadGnames();
            //InputHelper.ReleaseKeys();
            var form = new OverlayForm();
            form.Show();
        }

        private void comboBoxActors_SelectedIndexChanged(object sender, EventArgs e)
        {
            var actor = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource)[comboBoxActors.SelectedIndex];
            int selectedIndex = comboBoxActors.SelectedIndex;
            //var addresses = SoT_DataManager.Actor_address_map.Where(a => a.Value== actor.Key).ToList();

            //SoT_DataManager.InfoLog = $"{actor.Key} {actor.Value}";
            //foreach(var address in addresses)
            //{
            //    SoT_DataManager.InfoLog += $"\n{address}";
            //}
        }

        private void AddActorButton_Click(object sender, EventArgs e)
        {
            var actorRaw = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource)[comboBoxActors.SelectedIndex];
            var actor_id = actorRaw.Key;
            var raw_name = actorRaw.Value;

            //var actor_address = SoT_DataManager.Actors.Where(a => a.Value.RawName == raw_name).Select(a => a.Key).ToList();
            //SoT_DataManager.Actor_address_map.Where(a => a.Value == actorRaw.Key).Select(a => a.Key).ToList();
            foreach (var basicActor in SoT_DataManager.Actors.Where(a => a.Value.RawName == raw_name))
            {
                var actor = new Actor(SoT_Tool.mem, actor_id, basicActor.Key, raw_name);
                actor.Color = Color.Red;
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == actor.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(actor);
                }
            }
        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            textFilter = !textFilter;
            if (textFilter)
                FilterButton.Text = "Filter: ON";
            else
                FilterButton.Text = "Filter: OFF";

            UpdateLists();
        }

        private async void UpdateLists()
        {
            SoT_DataManager.ActorName_List = SoT_DataManager.Actor_name_map.Where(a => !SoT_DataManager.IgnoreActors.ContainsKey(a.Key)).ToList();
            comboBoxActors.DataSource = SoT_DataManager.ActorName_List;

            if (rangeFilter)
            {
                List<BasicActor> actors = new List<BasicActor>();

                int range = (int)numericUpDown1.Value;

                foreach (var actor in SoT_DataManager.Actors)
                {
                    var coordinates = await SoT_Tool.GetActorCoords(actor.Key);
                    float distance = MathHelper.CalculateDistance(coordinates, SoT_Tool.my_coords);
                    actors.Add(new BasicActor() { ActorAddress = actor.Key, ActorId = actor.Value.ActorId, Coords = coordinates, Distance = distance, RawName = actor.Value.RawName });
                }
                var inRange = actors.Where(a => a.Distance < range).ToList();

                List<int> actorIds = inRange.Select(a => a.ActorId).Distinct().ToList();
                List<KeyValuePair<int, string>> actorList = SoT_DataManager.Actor_name_map.Where(a => actorIds.Contains(a.Key)).ToList();
                comboBoxActors.DataSource = actorList;
            }

            if (textFilter)
            {
                List<KeyValuePair<int, string>> list = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource).Where(a => a.Value.ToLower().Contains(FilterTextBox.Text.ToLower())).ToList();
                //SoT_DataManager.ActorName_List.Where(a => a.Value.ToLower().Contains(FilterTextBox.Text.ToLower())).ToList();
                comboBoxActors.DataSource = list;

                List<Island> islandList = SoT_DataManager.Islands.Where(a => a.IslandName.ToLower().Contains(FilterTextBox.Text.ToLower())).ToList();
                if (islandList.Any())
                    IslandComboBox.DataSource = islandList;
                else
                    IslandComboBox.DataSource = SoT_DataManager.Islands;
            }
            else
                IslandComboBox.DataSource = SoT_DataManager.Islands;
            IslandComboBox.DisplayMember = "IslandName";

            if (((List<KeyValuePair<int, string>>)comboBoxActors.DataSource).Any())
                comboBoxActors.SelectedIndex = 0;
            RefreshMyForm();
        }

        private async void RangeFilterButton_Click(object sender, EventArgs e)
        {
            rangeFilter = !rangeFilter;

            UpdateLists();

            if (rangeFilter)
            {
                if (((List<KeyValuePair<int, string>>)comboBoxActors.DataSource).Select(a => a.Key).Any())
                {
                    SoT_DataManager.InfoLog = "\n";
                    foreach (int id in ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource).Select(a => a.Key).ToList())
                    {
                        SoT_DataManager.InfoLog += SoT_DataManager.Actor_name_map[id] + "\n";
                    }
                    RefreshMyForm();
                }
            }

            if (rangeFilter)
                RangeFilterButton.Text = "RangeFilter: ON";
            else
                RangeFilterButton.Text = "RangeFilter: OFF";
        }

        private void AddAllbutton_Click(object sender, EventArgs e)
        {
            //var actorRaw = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource)[comboBoxActors.SelectedIndex];
            //var actor_id = actorRaw.Key;
            //var raw_name = actorRaw.Value;

            var rawnames = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource).Select(a => a.Value).ToList();

            //var actor_address = SoT_DataManager.Actors.Where(a => a.Value.RawName == raw_name).Select(a => a.Key).ToList();
            //SoT_DataManager.Actor_address_map.Where(a => a.Value == actorRaw.Key).Select(a => a.Key).ToList();
            foreach (var basicActor in SoT_DataManager.Actors.Where(a => rawnames.Contains(a.Value.RawName)))
            {
                var actor = new Actor(SoT_Tool.mem, basicActor.Value.ActorId, basicActor.Key, basicActor.Value.RawName);
                actor.Color = Color.Red;
                if (!SoT_DataManager.DisplayObjects.Any(dob => dob.ActorAddress == actor.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.Add(actor);
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            var actorRaw = ((List<KeyValuePair<int, string>>)comboBoxActors.DataSource)[comboBoxActors.SelectedIndex];
            var actor_id = actorRaw.Key;
            var raw_name = actorRaw.Value;

            var newBag = new ConcurrentBag<DisplayObject>(
            SoT_DataManager.DisplayObjects.Where(a => a.ActorId != actor_id));
            SoT_DataManager.DisplayObjects = newBag;
        }

        private void RemoveAllButton_Click(object sender, EventArgs e)
        {
            SoT_Tool.FullReset();
            SoT_Tool.Init();
        }

        private void showStorageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleContainers();
        }

        public static void ToggleContainers()
        {
            ConfigurationManager.AppSettings["ShowContainers"] = !bool.Parse(ConfigurationManager.AppSettings["ShowContainers"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showStorageItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleContainerItems();
        }

        public static void ToggleContainerItems()
        {
            ConfigurationManager.AppSettings["ShowContainerItems"] = !bool.Parse(ConfigurationManager.AppSettings["ShowContainerItems"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showItemsOnYourShipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showItemsOnYourShipToolStripMenuItem.Checked = !showItemsOnYourShipToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowItemsOnShips"] = showItemsOnYourShipToolStripMenuItem.Checked ? "True" : "False";
        }

        private void showPlayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TogglePlayers();
        }

        public static void TogglePlayers()
        {
            ConfigurationManager.AppSettings["ShowPlayers"] = !bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showShipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleShips();
        }

        public static void ToggleShips()
        {
            ConfigurationManager.AppSettings["ShowShips"] = !bool.Parse(ConfigurationManager.AppSettings["ShowShips"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showCrewsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleCrewTracking();
        }

        public static void ToggleCrewTracking()
        {
            ConfigurationManager.AppSettings["ShowCrews"] = !bool.Parse(ConfigurationManager.AppSettings["ShowCrews"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showOtherStuffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleOther();
        }

        public static void ToggleOther()
        {
            ConfigurationManager.AppSettings["ShowOther"] = !bool.Parse(ConfigurationManager.AppSettings["ShowOther"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showTreasureSpotsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ToggleTreasureMap();
        }

        public static void ToggleTreasureMap()
        {
            ConfigurationManager.AppSettings["TrackTreasureMapSpots"] = !bool.Parse(ConfigurationManager.AppSettings["TrackTreasureMapSpots"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationManager.RefreshSection("appSettings");
            RefreshUISettings();
        }

        private void dumpJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoT_Tool.DumpRawNamesAsJSON();
        }

        private void ShowIslandToggleButton_Click(object sender, EventArgs e)
        {
            if (IslandComboBox.SelectedItem != null)
            {
                var selectedIsland = (Island)IslandComboBox.SelectedItem;
                SoT_DataManager.InfoLog += "\n" + selectedIsland.IslandName + " : " + selectedIsland.Rawname;
                if (SoT_DataManager.DisplayObjects.Where(d => d.ActorAddress == selectedIsland.ActorAddress).Any() && !selectedIsland.ToDelete)
                    selectedIsland.ToDelete = true;
                else
                {
                    selectedIsland.ToDelete = false;
                    if (!SoT_DataManager.DisplayObjects.Where(d => d.ActorAddress == selectedIsland.ActorAddress).Any())
                        SoT_DataManager.DisplayObjects.Add(selectedIsland);
                }
            }
        }

        private void ClearLogButton_Click(object sender, EventArgs e)
        {
            SoT_DataManager.InfoLog = "";
            RefreshMyForm();
        }

        private void showTomesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showTomesToolStripMenuItem.Checked = !showTomesToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowTomes"] = showTomesToolStripMenuItem.Checked ? "True" : "False";
        }

        private async void updateOffsetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;

            SoT_DataManager.InfoLog += "Updating offsets, this will probably take a few minutes...\n";
            richTextBox1.Text = $"Playername: {SoT_Tool.PlayerName}\n" + SoT_DataManager.InfoLog;

            if (!Directory.Exists(ConfigurationManager.AppSettings["SDKPath"]))
            {
                // Show the FolderBrowserDialog.  
                DialogResult result = folderDlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Environment.SpecialFolder root = folderDlg.RootFolder;
                    await Task.Run(async () =>
                    {
                        await OffsetFinder.UpdateOffsets(folderDlg.SelectedPath);
                    });
                }
            }
            else
            {
                await Task.Run(async () =>
                {
                    await OffsetFinder.UpdateOffsets(ConfigurationManager.AppSettings["SDKPath"]);
                });
            }
        }

        private void resetTextSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NumberInputDialog dialog = new NumberInputDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    ConfigurationManager.AppSettings["TextSize"] = dialog.Number.ToString();
                    CharmService.TextSize = dialog.Number;
                    //decimal number = dialog.Number;
                    //MessageBox.Show($"You entered: {number}");
                }
            }
        }

        private async void sDKToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;

            SoT_DataManager.InfoLog += "Updating offsets, this will probably take a few minutes...\n";
            richTextBox1.Text = $"Playername: {SoT_Tool.PlayerName}\n" + SoT_DataManager.InfoLog;

            if (!Directory.Exists(ConfigurationManager.AppSettings["SDKPath"]))
            {
                // Show the FolderBrowserDialog.  
                DialogResult result = folderDlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Environment.SpecialFolder root = folderDlg.RootFolder;
                    await Task.Run(async () =>
                    {
                        await SDKService.ScanSDK(folderDlg.SelectedPath);
                    });
                    RefreshMyForm();
                    var myForm = new SDK_ExploreerForm();
                    myForm.Show();
                }
            }
            else
            {
                await Task.Run(async () =>
                {
                    await SDKService.ScanSDK(ConfigurationManager.AppSettings["SDKPath"]);
                });
                RefreshMyForm();
                var myForm = new SDK_ExploreerForm();
                myForm.Show();
            }
        }

        private void UpdateFilterButton_Click(object sender, EventArgs e)
        {
            UpdateLists();
        }

        async void updateSDKDumpPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;

            // Show the FolderBrowserDialog.  
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                Environment.SpecialFolder root = folderDlg.RootFolder;
                DirectoryInfo directoryInfo = new DirectoryInfo(folderDlg.SelectedPath);
                var SDKfiles = directoryInfo.GetFiles().Where(f => f.Extension == ".h").ToList();
                if (SDKfiles.Count > 0)
                {
                    ConfigurationManager.AppSettings["SDKPath"] = folderDlg.SelectedPath;
                    MessageBox.Show($"{SDKfiles.Count} .h files found in selected folder. SDK path has been updated\nRemember to save settings if you want to app to keep using this path.");
                }
                else
                {
                    MessageBox.Show("No .h files found in selected folder");
                }
            }
        }

        private void showShipStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleShipStatus();
        }

        public static void ToggleShipStatus()
        {
            ConfigurationManager.AppSettings["ShowShipStatus"] = !bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void startOverlayOnLaunchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startOverlayOnLaunchToolStripMenuItem.Checked = !startOverlayOnLaunchToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["StartOverlayOnLaunch"] = startOverlayOnLaunchToolStripMenuItem.Checked ? "True" : "False";
        }

        private void showSkeletonMeshNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SoT_DataManager.DisplayObjects.Any(a => a is Skeleton))
            {
                var skeletons = SoT_DataManager.DisplayObjects.Where(a => a is Skeleton).Select(s => (Skeleton)s).ToList();
                foreach (var skeleton in skeletons)
                {
                    skeleton.Name = skeleton.MeshName;
                }
            }
        }

        private void showRiddleSpotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleRiddles();
        }

        public static void ToggleRiddles()
        {
            ConfigurationManager.AppSettings["TrackRiddleSpots"] = !bool.Parse(ConfigurationManager.AppSettings["TrackRiddleSpots"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void onlyEnemyPlayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFriendlyPlayers();
        }

        public static void ToggleFriendlyPlayers()
        {
            ConfigurationManager.AppSettings["HideFriendlyPlayers"] = !bool.Parse(ConfigurationManager.AppSettings["HideFriendlyPlayers"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showSkeletonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showSkeletonsToolStripMenuItem.Checked = !showSkeletonsToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowSkeletons"] = showSkeletonsToolStripMenuItem.Checked ? "True" : "False";
        }

        private void displayOverlayFPSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayOverlayFPSToolStripMenuItem.Checked = !displayOverlayFPSToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["OverlayFPS"] = displayOverlayFPSToolStripMenuItem.Checked ? "True" : "False";

            MessageBox.Show("Save settings and restart app to have this setting take effect");
        }

        private void lessShipInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lessShipInfoToolStripMenuItem.Checked = !lessShipInfoToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["LessShipInfo"] = lessShipInfoToolStripMenuItem.Checked ? "True" : "False";
        }

        private void disableTreasureMapScalingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableTreasureMapScalingToolStripMenuItem.Checked = !disableTreasureMapScalingToolStripMenuItem.Checked;
            SoT_Tool.useTreasureMapScalingFactor = !disableTreasureMapScalingToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["TreasureMapIslandScaling"] = lessShipInfoToolStripMenuItem.Checked ? "True" : "False";

            if (SoT_DataManager.DisplayObjects.Any(x => x.GetType() == typeof(TreasureMap)))
            {
                var maps = SoT_DataManager.DisplayObjects.Where(x => x.GetType() == typeof(TreasureMap)).Select(m => (TreasureMap)m).ToList();
                maps.ForEach(m => m.UpdateMarks());
            }
        }

        private void crosshairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleCrosshair();
        }

        public static void ToggleCrosshair()
        {
            ConfigurationManager.AppSettings["ShowCrosshair"] = !bool.Parse(ConfigurationManager.AppSettings["StartOverlayOnLaunch"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void compassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleCompass();
        }

        public static void ToggleCompass()
        {
            ConfigurationManager.AppSettings["ShowCompass"] = !bool.Parse(ConfigurationManager.AppSettings["ShowCompass"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showProjectilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showProjectilesToolStripMenuItem.Checked = !showProjectilesToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowProjectiles"] = showProjectilesToolStripMenuItem.Checked ? "True" : "False";
        }

        private void enemyPlayerTracelinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleEnemyPlayerTracelines();
        }

        public static void ToggleEnemyPlayerTracelines()
        {
            ConfigurationManager.AppSettings["ShowPlayerTracelines"] = !bool.Parse(ConfigurationManager.AppSettings["ShowPlayerTracelines"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void sDKToolToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            sDKToolToolStripMenuItem_Click(sender, e);
        }

        private void showMapPinsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMapPins();
        }

        public static void ToggleMapPins()
        {
            ConfigurationManager.AppSettings["ShowMapPins"] = !bool.Parse(ConfigurationManager.AppSettings["ShowMapPins"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showMapShipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMapShips();
        }

        public static void ToggleMapShips()
        {
            ConfigurationManager.AppSettings["ShowMapShips"] = !bool.Parse(ConfigurationManager.AppSettings["ShowMapShips"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void showMapTreasureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleMapLoot();
        }

        public static void ToggleMapLoot()
        {
            ConfigurationManager.AppSettings["ShowMapLoot"] = !bool.Parse(ConfigurationManager.AppSettings["ShowMapLoot"]) ? "True" : "False";
            Instance.RefreshUISettings();
        }

        private void resetOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestartApp();
        }

        private void DebugTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void cookBotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ptr = ProcessUtils.FindWindow("Sea of Thieves");
            ProcessUtils.SetForegroundWindow(ptr);
            RunCookingBot();
        }

        private void fishingBotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ptr = ProcessUtils.FindWindow("Sea of Thieves");
            ProcessUtils.SetForegroundWindow(ptr);
            RunFishingBot();
        }

        public static void RunCookingBot()
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
            var cookpot = cookpots.OrderBy(a => (float)SoT_Tool.GetDistanceFromActor(a.ActorAddress)).ToList().First();
            if (SoT_Tool.GetDistanceFromActor(cookpot.ActorAddress) > 5)
            {
                MessageBox.Show("Cooking pot needs to be in view and in range before the cooking bot can begin.");
            }
            else
            {
                if (SoT_DataManager.DisplayObjects.Any(x => x.ActorAddress == cookpot.ActorAddress))
                {
                    MessageBox.Show("Cooking bot already running");
                    return;
                }
                else
                {
                    var ptr = ProcessUtils.FindWindow("Sea of Thieves");
                    if (ptr == IntPtr.Zero)
                    {
                        ptr = ProcessUtils.FindWindow("Sea of Thieves Insider");
                    }

                    ProcessUtils.SetForegroundWindow(ptr);
                    var cookingpotActor = new CookPot(SoT_Tool.mem, cookpot.ActorAddress, SoT_Tool.PlayerAddress);
                    SoT_DataManager.DisplayObjects.Add(cookingpotActor);
                }
            }
        }

        public static void RunFishingBot()
        {
            if (!SoT_Tool.GameRunning)
            {
                MessageBox.Show("Game is not running or not detected.");
                return;
            }
            if (SoT_DataManager.DisplayObjects.Any(x => x is FishingBot))
            {
                //MessageBox.Show("Fishing bot already running. Try fishing.");
                var b = (FishingBot)SoT_DataManager.DisplayObjects.First(x => x is FishingBot);

                if (b.ActorAddress < 6)
                {
                    b.ToDelete = true;
                }
                else
                    b.SelfDestruct();

                return;
            }
            var bot = new FishingBot(SoT_Tool.mem);
            SoT_DataManager.DisplayObjects.Add(bot);
        }

        public static void RestartApp()
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void keepIdleDisconnectDisabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            keepIdleDisconnectDisabledToolStripMenuItem.Checked = !keepIdleDisconnectDisabledToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["KeepIdleDisconnectDisabled"] = keepIdleDisconnectDisabledToolStripMenuItem.Checked ? "True" : "False";
        }

        private void setKeybindingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var myForm = new KeybindingsForm();
            myForm.Show();
        }

        private void showAllIslandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAllIslandsToolStripMenuItem.Checked = !showAllIslandsToolStripMenuItem.Checked;

            foreach (var island in SoT_DataManager.Islands)
            {
                ToggleIsland(island, showAllIslandsToolStripMenuItem.Checked);
            }
        }

        private void showAllOutpostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAllOutpostsToolStripMenuItem.Checked = !showAllOutpostsToolStripMenuItem.Checked;

            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().Contains("outpost")
            || i.IslandName.ToLower().Contains("merrick") || i.IslandName.ToLower().Contains("port")))
            {
                ToggleIsland(island, showAllOutpostsToolStripMenuItem.Checked);
            }
        }

        private void ToggleIsland(Island island, bool show)
        {
            if (show)
            {
                if (!SoT_DataManager.DisplayObjects.Any(d => d.ActorAddress == island.ActorAddress))
                {
                    island.ToDelete = false;
                    SoT_DataManager.DisplayObjects.Add(island);
                }
            }
            else
            {
                if (SoT_DataManager.DisplayObjects.Any(d => d.ActorAddress == island.ActorAddress))
                {
                    SoT_DataManager.DisplayObjects.First(d => d.ActorAddress == island.ActorAddress).ToDelete = true;
                    island.ToDelete = true;
                }
            }
        }



        private void showAllSeaPostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAllSeaPostsToolStripMenuItem.Checked = !showAllSeaPostsToolStripMenuItem.Checked;

            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().Contains("post")
            || i.IslandName.ToLower().Contains("traders") || i.IslandName.ToLower().Contains("store")
            || i.IslandName.ToLower().Contains("bazaar") || i.IslandName.ToLower().Contains("spoils")))
            {
                ToggleIsland(island, showAllSeaPostsToolStripMenuItem.Checked);
            }
        }

        private void showOnlyIslandsWith3kmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showOnlyIslandsWith3kmToolStripMenuItem.Checked = !showOnlyIslandsWith3kmToolStripMenuItem.Checked;

            foreach (var island in SoT_DataManager.Islands)
            {
                if (showOnlyIslandsWith3kmToolStripMenuItem.Checked)
                    island.TrackingRange = 3000;
                else
                    island.TrackingRange = 0;
            }
        }

        private void useFormsOverlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            useFormsOverlayToolStripMenuItem.Checked = !useFormsOverlayToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["FormsOverlay"] = displayOverlayFPSToolStripMenuItem.Checked ? "True" : "False";
            MessageBox.Show("Please save settings and restart the application for the changes to take effect.");

        }

        public static void ToggleAllIslands()
        {
            foreach (var island in SoT_DataManager.Islands)
            {
                ToggleIsland(island);
            }
        }

        public static void ToggleAllOutposts()
        {
            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().Contains("outpost")
            || i.IslandName.ToLower().Contains("merrick") || i.IslandName.ToLower().Contains("port")))
            {
                ToggleIsland(island);
            }
        }

        public static void ToggleAllSeaposts()
        {
            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().Contains("post")
            || i.IslandName.ToLower().Contains("traders") || i.IslandName.ToLower().Contains("store")
            || i.IslandName.ToLower().Contains("bazaar") || i.IslandName.ToLower().Contains("spoils")
            || i.IslandName.ToLower().Contains("s rest")))
            {
                ToggleIsland(island);
            }
        }

        public static void ToggleIslandTrackingRange()
        {
            foreach (var island in SoT_DataManager.Islands)
            {
                if (island.TrackingRange == 0)
                    island.TrackingRange = 3000;
                else
                    island.TrackingRange = 0;
            }
        }

        private static void ToggleIsland(Island island)
        {
            if (!SoT_DataManager.DisplayObjects.Any(d => d.ActorAddress == island.ActorAddress))
            {
                island.ToDelete = false;
                SoT_DataManager.DisplayObjects.Add(island);
            }
            else
            {
                SoT_DataManager.DisplayObjects.First(d => d.ActorAddress == island.ActorAddress).ToDelete = true;
                island.ToDelete = true;
            }
        }

        private void showAllSeaFortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().EndsWith("fortress")
            && !i.IslandName.ToLower().Contains("crow") && !i.IslandName.ToLower().Contains("coral")
            && !i.IslandName.ToLower().Contains("molten")))
            {
                ToggleIsland(island);
            }
        }

        private void showAllSkeletonFortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var island in SoT_DataManager.Islands.Where(i => i.IslandName.ToLower().EndsWith("fort")
            || i.IslandName.ToLower().EndsWith("stronghold") || i.IslandName.ToLower().EndsWith("nest fortress")
            || i.IslandName.ToLower().EndsWith("sands fortress") || i.IslandName.ToLower().Contains("sands fortress")))
            {
                ToggleIsland(island);
            }
        }

        private void showResourcesOfShipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showResourcesOfShipsToolStripMenuItem.Checked = !showResourcesOfShipsToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowResourcesOnShips"] = showResourcesOfShipsToolStripMenuItem.Checked ? "True" : "False";
        }

        private void removeMarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SoT_DataManager.DisplayObjects.Where(d => d is Marker).ToList().ForEach(d => d.ToDelete = true);
        }

        private void MigrationButton_Click(object sender, EventArgs e)
        {
            if (!SoT_Tool.GameRunning)
                return;
            if (Ship.PlayerShip != null && !Ship.PlayerShip.ToDelete)
            {
                Ship.PlayerShip.FlipMigrationBasedOnVelocityEnabled();
                MigrationButton.Text = Ship.PlayerShip.GetShipMigrationBasedOnVelocityEnabled() ? "Migration based on velocity: ON" : "Migration based on velocity: OFF";
            }
        }

        private void showCannonTrajectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showCannonTrajectoryToolStripMenuItem.Checked = !showCannonTrajectoryToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ShowCannonTrajectoryPrediction"] = showCannonTrajectoryToolStripMenuItem.Checked ? "True" : "False";
        }

        private void readOffsetsFromMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            readOffsetsFromMemoryToolStripMenuItem.Checked = !readOffsetsFromMemoryToolStripMenuItem.Checked;
            ConfigurationManager.AppSettings["ReadOffsetsFromMemory"] = readOffsetsFromMemoryToolStripMenuItem.Checked ? "True" : "False";
        }

        private async void updateOffsetsFromMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await OffsetFinder.UpdateOffsetsFromMemory();
        }

        private void fishToFishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!FishingBot.FishData.Any())
            {
                if (SoT_DataManager.Actors.Any(a => a.Value.RawName.ToLower().Contains("fishing") && a.Value.RawName.ToLower().Contains("rod")))
                {
                    var fishingrod = SoT_DataManager.Actors.First(a => a.Value.RawName.ToLower().Contains("fishing") && a.Value.RawName.ToLower().Contains("rod")).Key;
                    FishingBot.ReadFish(fishingrod);
                    if (!FishingBot.FishData.Any())
                    {
                        MessageBox.Show("No fish data found. Please go fishing and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("No fish data found. Please go fishing and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            var fishData = FishingBot.FishData.Keys.Where(f => f.Contains(" ")).ToList();
            fishData.AddRange(fishData.Select(f => "Trophy " + f).ToList());
            var form = GenerateForm(FishingBot.FishToFish, fishData);
            form.Show();
        }

        public Form GenerateForm(List<string> selectedOptions, List<string> options)
        {
            // Create a new form
            Form form = new Form();

            // Set form properties
            form.Text = "Dynamic Form";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoSize = true;
            form.AutoSizeMode = AutoSizeMode.GrowOnly;
            form.Size = new Size(500, 500);
            // Create a new FlowLayoutPanel
            FlowLayoutPanel panel = new FlowLayoutPanel();

            // Set FlowLayoutPanel properties
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            // Add the FlowLayoutPanel to the form
            form.Controls.Add(panel);

            // Create two columns of checkboxes
            FlowLayoutPanel[] columns = new FlowLayoutPanel[2];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new FlowLayoutPanel();
                columns[i].FlowDirection = FlowDirection.TopDown;
                columns[i].AutoSize = true;
                columns[i].AutoSizeMode = AutoSizeMode.GrowAndShrink;
                panel.Controls.Add(columns[i]);
            }

            // Add options to the columns
            int col = 0;

            foreach (var option in options)
            {
                // Create a new CheckBox
                CheckBox checkBox = new CheckBox();

                // Set CheckBox properties
                checkBox.AutoSize = true;
                checkBox.Text = option;
                checkBox.Checked = selectedOptions.Contains(option);

                // Add event handler for CheckedChanged event
                checkBox.CheckedChanged += (s, e) =>
                {
                    if (checkBox.Checked && !selectedOptions.Contains(checkBox.Text))
                    {
                        selectedOptions.Add(checkBox.Text);
                    }
                    else if (!checkBox.Checked && selectedOptions.Contains(checkBox.Text))
                    {
                        selectedOptions.Remove(checkBox.Text);
                    }
                };

                // Add the CheckBox to the current column
                columns[col].Controls.Add(checkBox);

                // Switch to the other column for the next CheckBox
                //col ^= 1;
            }

            return form;
        }

        private void RescanButton_Click(object sender, EventArgs e)
        {
            foreach (var actor in SoT_DataManager.Actors)
                SoT_Tool.AddDisplayActorIfRelevant(actor.Value.RawName, actor.Value.ActorAddress, actor.Value.ActorId);
        }

        private async void inspectorToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;

            if (!Directory.Exists(ConfigurationManager.AppSettings["SDKPath"]))
            {
                // Show the FolderBrowserDialog.  
                DialogResult result = folderDlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    Environment.SpecialFolder root = folderDlg.RootFolder;
                    await Task.Run(async () =>
                    {
                        await SDKService.ScanSDK(ConfigurationManager.AppSettings["SDKPath"]);
                    });
                }
            }
            else
                await Task.Run(async () =>
                {
                    await SDKService.ScanSDK(ConfigurationManager.AppSettings["SDKPath"]);
                });
            RefreshMyForm();
            var myForm = new InspectorForm();
            myForm.Show();
        }
    }

    public class NumberInputDialog : Form
    {
        private NumericUpDown numberInput;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        public int Number { get; private set; }

        public NumberInputDialog()
        {
            this.Text = "Select new text size";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(240, 80);

            numberInput = new NumericUpDown()
            {
                Location = new System.Drawing.Point(12, 12),
                Width = 210,
                DecimalPlaces = 0,
            };
            this.Controls.Add(numberInput);
            numberInput.Value = int.Parse(ConfigurationManager.AppSettings["TextSize"]);
            okButton = new System.Windows.Forms.Button()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(150, 40),
            };
            okButton.Click += new EventHandler(okButton_Click);
            this.Controls.Add(okButton);
            this.AcceptButton = okButton;

            cancelButton = new System.Windows.Forms.Button()
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(75, 40),
            };
            this.Controls.Add(cancelButton);
            this.CancelButton = cancelButton;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Number = (int)numberInput.Value;
        }
    }
}