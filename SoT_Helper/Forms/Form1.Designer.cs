namespace SoT_Helper
{
    partial class SoTHelper
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DoStuff = new Button();
            richTextBox1 = new RichTextBox();
            IdleDC_Button = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            AddOverlayButton = new Button();
            TestButton = new Button();
            process1 = new System.Diagnostics.Process();
            comboBoxActors = new ComboBox();
            AddActorButton = new Button();
            FilterTextBox = new TextBox();
            FilterButton = new Button();
            RangeFilterButton = new Button();
            numericUpDown1 = new NumericUpDown();
            AddAllbutton = new Button();
            RemoveButton = new Button();
            RemoveAllButton = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            dumpJsonToolStripMenuItem = new ToolStripMenuItem();
            loadSettingsToolStripMenuItem = new ToolStripMenuItem();
            saveSettingsToolStripMenuItem = new ToolStripMenuItem();
            updateOffsetsToolStripMenuItem = new ToolStripMenuItem();
            updateOffsetsFromMemoryToolStripMenuItem = new ToolStripMenuItem();
            updateSDKDumpPathToolStripMenuItem = new ToolStripMenuItem();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            showStorageToolStripMenuItem = new ToolStripMenuItem();
            showStorageItemsToolStripMenuItem = new ToolStripMenuItem();
            showPlayersToolStripMenuItem = new ToolStripMenuItem();
            enemyPlayerTracelinesToolStripMenuItem = new ToolStripMenuItem();
            onlyEnemyPlayersToolStripMenuItem = new ToolStripMenuItem();
            showSkeletonsToolStripMenuItem = new ToolStripMenuItem();
            showShipsToolStripMenuItem = new ToolStripMenuItem();
            lessShipInfoToolStripMenuItem = new ToolStripMenuItem();
            showShipStatusToolStripMenuItem = new ToolStripMenuItem();
            showItemsOnYourShipToolStripMenuItem = new ToolStripMenuItem();
            showResourcesOfShipsToolStripMenuItem = new ToolStripMenuItem();
            showCrewsToolStripMenuItem = new ToolStripMenuItem();
            showTomesToolStripMenuItem = new ToolStripMenuItem();
            showProjectilesToolStripMenuItem = new ToolStripMenuItem();
            showOtherStuffToolStripMenuItem = new ToolStripMenuItem();
            showCannonTrajectoryToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            resetTextSizeToolStripMenuItem = new ToolStripMenuItem();
            startOverlayOnLaunchToolStripMenuItem = new ToolStripMenuItem();
            crosshairToolStripMenuItem = new ToolStripMenuItem();
            compassToolStripMenuItem = new ToolStripMenuItem();
            useFormsOverlayToolStripMenuItem = new ToolStripMenuItem();
            displayOverlayFPSToolStripMenuItem = new ToolStripMenuItem();
            keepIdleDisconnectDisabledToolStripMenuItem = new ToolStripMenuItem();
            setKeybindingsToolStripMenuItem = new ToolStripMenuItem();
            readOffsetsFromMemoryToolStripMenuItem = new ToolStripMenuItem();
            debugToolStripMenuItem = new ToolStripMenuItem();
            sDKToolToolStripMenuItem = new ToolStripMenuItem();
            inspectorToolToolStripMenuItem = new ToolStripMenuItem();
            showSkeletonMeshNamesToolStripMenuItem = new ToolStripMenuItem();
            disableTreasureMapScalingToolStripMenuItem = new ToolStripMenuItem();
            resetOverlayToolStripMenuItem = new ToolStripMenuItem();
            questsToolStripMenuItem = new ToolStripMenuItem();
            showTreasureSpotsToolStripMenuItem = new ToolStripMenuItem();
            showRiddleSpotsToolStripMenuItem = new ToolStripMenuItem();
            mapToolStripMenuItem = new ToolStripMenuItem();
            showMapPinsToolStripMenuItem = new ToolStripMenuItem();
            showMapShipsToolStripMenuItem = new ToolStripMenuItem();
            showMapTreasureToolStripMenuItem = new ToolStripMenuItem();
            showAllIslandsToolStripMenuItem = new ToolStripMenuItem();
            showAllOutpostsToolStripMenuItem = new ToolStripMenuItem();
            showAllSeaPostsToolStripMenuItem = new ToolStripMenuItem();
            showAllSeaFortsToolStripMenuItem = new ToolStripMenuItem();
            showOnlyIslandsWith3kmToolStripMenuItem = new ToolStripMenuItem();
            showAllSkeletonFortsToolStripMenuItem = new ToolStripMenuItem();
            removeMarkersToolStripMenuItem = new ToolStripMenuItem();
            botsToolStripMenuItem = new ToolStripMenuItem();
            cookBotToolStripMenuItem = new ToolStripMenuItem();
            fishingBotToolStripMenuItem = new ToolStripMenuItem();
            fishToFishToolStripMenuItem = new ToolStripMenuItem();
            IslandComboBox = new ComboBox();
            ShowIslandToggleButton = new Button();
            ClearLogButton = new Button();
            UpdateFilterButton = new Button();
            DebugTextBox = new TextBox();
            MigrationButton = new Button();
            RescanButton = new Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // DoStuff
            // 
            DoStuff.Location = new Point(858, 532);
            DoStuff.Name = "DoStuff";
            DoStuff.Size = new Size(75, 23);
            DoStuff.TabIndex = 0;
            DoStuff.Text = "Find SoT";
            DoStuff.UseVisualStyleBackColor = true;
            DoStuff.Click += button1_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(30, 82);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(508, 471);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "";
            // 
            // IdleDC_Button
            // 
            IdleDC_Button.Location = new Point(648, 532);
            IdleDC_Button.Name = "IdleDC_Button";
            IdleDC_Button.Size = new Size(204, 23);
            IdleDC_Button.TabIndex = 2;
            IdleDC_Button.Text = "Disable IdleDisconnect";
            IdleDC_Button.UseVisualStyleBackColor = true;
            IdleDC_Button.Click += IdleDC_Button_Click;
            // 
            // AddOverlayButton
            // 
            AddOverlayButton.Location = new Point(939, 532);
            AddOverlayButton.Name = "AddOverlayButton";
            AddOverlayButton.Size = new Size(108, 23);
            AddOverlayButton.TabIndex = 3;
            AddOverlayButton.Text = "Add Overlay";
            AddOverlayButton.UseVisualStyleBackColor = true;
            AddOverlayButton.Click += AddOverlayButton_Click;
            // 
            // TestButton
            // 
            TestButton.Location = new Point(972, 486);
            TestButton.Name = "TestButton";
            TestButton.Size = new Size(75, 23);
            TestButton.TabIndex = 4;
            TestButton.Text = "Test Button";
            TestButton.UseVisualStyleBackColor = true;
            TestButton.Click += TestButton_Click;
            // 
            // process1
            // 
            process1.StartInfo.Domain = "";
            process1.StartInfo.LoadUserProfile = false;
            process1.StartInfo.Password = null;
            process1.StartInfo.StandardErrorEncoding = null;
            process1.StartInfo.StandardInputEncoding = null;
            process1.StartInfo.StandardOutputEncoding = null;
            process1.StartInfo.UserName = "";
            process1.SynchronizingObject = this;
            // 
            // comboBoxActors
            // 
            comboBoxActors.FormattingEnabled = true;
            comboBoxActors.Location = new Point(555, 30);
            comboBoxActors.Name = "comboBoxActors";
            comboBoxActors.Size = new Size(316, 23);
            comboBoxActors.TabIndex = 5;
            comboBoxActors.SelectedIndexChanged += comboBoxActors_SelectedIndexChanged;
            // 
            // AddActorButton
            // 
            AddActorButton.Location = new Point(877, 30);
            AddActorButton.Name = "AddActorButton";
            AddActorButton.Size = new Size(56, 23);
            AddActorButton.TabIndex = 6;
            AddActorButton.Text = "Add";
            AddActorButton.UseVisualStyleBackColor = true;
            AddActorButton.Click += AddActorButton_Click;
            // 
            // FilterTextBox
            // 
            FilterTextBox.Location = new Point(358, 29);
            FilterTextBox.Name = "FilterTextBox";
            FilterTextBox.Size = new Size(107, 23);
            FilterTextBox.TabIndex = 7;
            // 
            // FilterButton
            // 
            FilterButton.Location = new Point(471, 29);
            FilterButton.Name = "FilterButton";
            FilterButton.Size = new Size(78, 23);
            FilterButton.TabIndex = 8;
            FilterButton.Text = "Filter";
            FilterButton.UseVisualStyleBackColor = true;
            FilterButton.Click += FilterButton_Click;
            // 
            // RangeFilterButton
            // 
            RangeFilterButton.Location = new Point(763, 59);
            RangeFilterButton.Name = "RangeFilterButton";
            RangeFilterButton.Size = new Size(103, 23);
            RangeFilterButton.TabIndex = 9;
            RangeFilterButton.Text = "RangeFilter";
            RangeFilterButton.UseVisualStyleBackColor = true;
            RangeFilterButton.Click += RangeFilterButton_Click;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(690, 59);
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(67, 23);
            numericUpDown1.TabIndex = 10;
            numericUpDown1.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // AddAllbutton
            // 
            AddAllbutton.Location = new Point(872, 59);
            AddAllbutton.Name = "AddAllbutton";
            AddAllbutton.Size = new Size(74, 23);
            AddAllbutton.TabIndex = 11;
            AddAllbutton.Text = "Add All";
            AddAllbutton.UseVisualStyleBackColor = true;
            AddAllbutton.Click += AddAllbutton_Click;
            // 
            // RemoveButton
            // 
            RemoveButton.Location = new Point(953, 29);
            RemoveButton.Name = "RemoveButton";
            RemoveButton.Size = new Size(60, 23);
            RemoveButton.TabIndex = 12;
            RemoveButton.Text = "Remove";
            RemoveButton.UseVisualStyleBackColor = true;
            RemoveButton.Click += RemoveButton_Click;
            // 
            // RemoveAllButton
            // 
            RemoveAllButton.Location = new Point(953, 61);
            RemoveAllButton.Name = "RemoveAllButton";
            RemoveAllButton.Size = new Size(94, 23);
            RemoveAllButton.TabIndex = 13;
            RemoveAllButton.Text = "Full Reset";
            RemoveAllButton.UseVisualStyleBackColor = true;
            RemoveAllButton.Click += RemoveAllButton_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, optionsToolStripMenuItem, settingsToolStripMenuItem, debugToolStripMenuItem, questsToolStripMenuItem, mapToolStripMenuItem, botsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1059, 24);
            menuStrip1.TabIndex = 14;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { dumpJsonToolStripMenuItem, loadSettingsToolStripMenuItem, saveSettingsToolStripMenuItem, updateOffsetsToolStripMenuItem, updateOffsetsFromMemoryToolStripMenuItem, updateSDKDumpPathToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // dumpJsonToolStripMenuItem
            // 
            dumpJsonToolStripMenuItem.Name = "dumpJsonToolStripMenuItem";
            dumpJsonToolStripMenuItem.Size = new Size(243, 22);
            dumpJsonToolStripMenuItem.Text = "Dump Json";
            dumpJsonToolStripMenuItem.Click += dumpJsonToolStripMenuItem_Click;
            // 
            // loadSettingsToolStripMenuItem
            // 
            loadSettingsToolStripMenuItem.Name = "loadSettingsToolStripMenuItem";
            loadSettingsToolStripMenuItem.Size = new Size(243, 22);
            loadSettingsToolStripMenuItem.Text = "Load Settings";
            loadSettingsToolStripMenuItem.Click += loadSettingsToolStripMenuItem_Click;
            // 
            // saveSettingsToolStripMenuItem
            // 
            saveSettingsToolStripMenuItem.Name = "saveSettingsToolStripMenuItem";
            saveSettingsToolStripMenuItem.Size = new Size(243, 22);
            saveSettingsToolStripMenuItem.Text = "Save Settings";
            saveSettingsToolStripMenuItem.Click += saveSettingsToolStripMenuItem_Click;
            // 
            // updateOffsetsToolStripMenuItem
            // 
            updateOffsetsToolStripMenuItem.Name = "updateOffsetsToolStripMenuItem";
            updateOffsetsToolStripMenuItem.Size = new Size(243, 22);
            updateOffsetsToolStripMenuItem.Text = "Update Offsets using SDK dump";
            updateOffsetsToolStripMenuItem.Click += updateOffsetsToolStripMenuItem_Click;
            // 
            // updateOffsetsFromMemoryToolStripMenuItem
            // 
            updateOffsetsFromMemoryToolStripMenuItem.Name = "updateOffsetsFromMemoryToolStripMenuItem";
            updateOffsetsFromMemoryToolStripMenuItem.Size = new Size(243, 22);
            updateOffsetsFromMemoryToolStripMenuItem.Text = "Update Offsets from memory";
            updateOffsetsFromMemoryToolStripMenuItem.Click += updateOffsetsFromMemoryToolStripMenuItem_Click;
            // 
            // updateSDKDumpPathToolStripMenuItem
            // 
            updateSDKDumpPathToolStripMenuItem.Name = "updateSDKDumpPathToolStripMenuItem";
            updateSDKDumpPathToolStripMenuItem.Size = new Size(243, 22);
            updateSDKDumpPathToolStripMenuItem.Text = "Update SDK Dump path";
            updateSDKDumpPathToolStripMenuItem.Click += updateSDKDumpPathToolStripMenuItem_Click;
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showStorageToolStripMenuItem, showStorageItemsToolStripMenuItem, showPlayersToolStripMenuItem, enemyPlayerTracelinesToolStripMenuItem, onlyEnemyPlayersToolStripMenuItem, showSkeletonsToolStripMenuItem, showShipsToolStripMenuItem, lessShipInfoToolStripMenuItem, showShipStatusToolStripMenuItem, showItemsOnYourShipToolStripMenuItem, showResourcesOfShipsToolStripMenuItem, showCrewsToolStripMenuItem, showTomesToolStripMenuItem, showProjectilesToolStripMenuItem, showOtherStuffToolStripMenuItem, showCannonTrajectoryToolStripMenuItem });
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(61, 20);
            optionsToolStripMenuItem.Text = "Options";
            // 
            // showStorageToolStripMenuItem
            // 
            showStorageToolStripMenuItem.Checked = true;
            showStorageToolStripMenuItem.CheckState = CheckState.Checked;
            showStorageToolStripMenuItem.Name = "showStorageToolStripMenuItem";
            showStorageToolStripMenuItem.Size = new Size(204, 22);
            showStorageToolStripMenuItem.Text = "Show Storage";
            showStorageToolStripMenuItem.Click += showStorageToolStripMenuItem_Click;
            // 
            // showStorageItemsToolStripMenuItem
            // 
            showStorageItemsToolStripMenuItem.Checked = true;
            showStorageItemsToolStripMenuItem.CheckState = CheckState.Checked;
            showStorageItemsToolStripMenuItem.Name = "showStorageItemsToolStripMenuItem";
            showStorageItemsToolStripMenuItem.Size = new Size(204, 22);
            showStorageItemsToolStripMenuItem.Text = "Show Storage Items";
            showStorageItemsToolStripMenuItem.Click += showStorageItemsToolStripMenuItem_Click;
            // 
            // showPlayersToolStripMenuItem
            // 
            showPlayersToolStripMenuItem.Checked = true;
            showPlayersToolStripMenuItem.CheckState = CheckState.Checked;
            showPlayersToolStripMenuItem.Name = "showPlayersToolStripMenuItem";
            showPlayersToolStripMenuItem.Size = new Size(204, 22);
            showPlayersToolStripMenuItem.Text = "Show Players";
            showPlayersToolStripMenuItem.Click += showPlayersToolStripMenuItem_Click;
            // 
            // enemyPlayerTracelinesToolStripMenuItem
            // 
            enemyPlayerTracelinesToolStripMenuItem.Checked = true;
            enemyPlayerTracelinesToolStripMenuItem.CheckState = CheckState.Checked;
            enemyPlayerTracelinesToolStripMenuItem.Name = "enemyPlayerTracelinesToolStripMenuItem";
            enemyPlayerTracelinesToolStripMenuItem.Size = new Size(204, 22);
            enemyPlayerTracelinesToolStripMenuItem.Text = "Enemy Player Tracelines";
            enemyPlayerTracelinesToolStripMenuItem.Click += enemyPlayerTracelinesToolStripMenuItem_Click;
            // 
            // onlyEnemyPlayersToolStripMenuItem
            // 
            onlyEnemyPlayersToolStripMenuItem.Name = "onlyEnemyPlayersToolStripMenuItem";
            onlyEnemyPlayersToolStripMenuItem.Size = new Size(204, 22);
            onlyEnemyPlayersToolStripMenuItem.Text = "Only enemy players";
            onlyEnemyPlayersToolStripMenuItem.Click += onlyEnemyPlayersToolStripMenuItem_Click;
            // 
            // showSkeletonsToolStripMenuItem
            // 
            showSkeletonsToolStripMenuItem.Checked = true;
            showSkeletonsToolStripMenuItem.CheckState = CheckState.Checked;
            showSkeletonsToolStripMenuItem.Name = "showSkeletonsToolStripMenuItem";
            showSkeletonsToolStripMenuItem.Size = new Size(204, 22);
            showSkeletonsToolStripMenuItem.Text = "Show Skeletons";
            showSkeletonsToolStripMenuItem.Click += showSkeletonsToolStripMenuItem_Click;
            // 
            // showShipsToolStripMenuItem
            // 
            showShipsToolStripMenuItem.Checked = true;
            showShipsToolStripMenuItem.CheckState = CheckState.Checked;
            showShipsToolStripMenuItem.Name = "showShipsToolStripMenuItem";
            showShipsToolStripMenuItem.Size = new Size(204, 22);
            showShipsToolStripMenuItem.Text = "Show Ships";
            showShipsToolStripMenuItem.Click += showShipsToolStripMenuItem_Click;
            // 
            // lessShipInfoToolStripMenuItem
            // 
            lessShipInfoToolStripMenuItem.Name = "lessShipInfoToolStripMenuItem";
            lessShipInfoToolStripMenuItem.Size = new Size(204, 22);
            lessShipInfoToolStripMenuItem.Text = "Less Ship Info";
            lessShipInfoToolStripMenuItem.Click += lessShipInfoToolStripMenuItem_Click;
            // 
            // showShipStatusToolStripMenuItem
            // 
            showShipStatusToolStripMenuItem.Checked = true;
            showShipStatusToolStripMenuItem.CheckState = CheckState.Checked;
            showShipStatusToolStripMenuItem.Name = "showShipStatusToolStripMenuItem";
            showShipStatusToolStripMenuItem.Size = new Size(204, 22);
            showShipStatusToolStripMenuItem.Text = "Show Ship Status";
            showShipStatusToolStripMenuItem.Click += showShipStatusToolStripMenuItem_Click;
            // 
            // showItemsOnYourShipToolStripMenuItem
            // 
            showItemsOnYourShipToolStripMenuItem.Checked = true;
            showItemsOnYourShipToolStripMenuItem.CheckState = CheckState.Checked;
            showItemsOnYourShipToolStripMenuItem.Name = "showItemsOnYourShipToolStripMenuItem";
            showItemsOnYourShipToolStripMenuItem.Size = new Size(204, 22);
            showItemsOnYourShipToolStripMenuItem.Text = "Show Items on Ships";
            showItemsOnYourShipToolStripMenuItem.Click += showItemsOnYourShipToolStripMenuItem_Click;
            // 
            // showResourcesOfShipsToolStripMenuItem
            // 
            showResourcesOfShipsToolStripMenuItem.Name = "showResourcesOfShipsToolStripMenuItem";
            showResourcesOfShipsToolStripMenuItem.Size = new Size(204, 22);
            showResourcesOfShipsToolStripMenuItem.Text = "Show Resources of Ships";
            showResourcesOfShipsToolStripMenuItem.Click += showResourcesOfShipsToolStripMenuItem_Click;
            // 
            // showCrewsToolStripMenuItem
            // 
            showCrewsToolStripMenuItem.Checked = true;
            showCrewsToolStripMenuItem.CheckState = CheckState.Checked;
            showCrewsToolStripMenuItem.Name = "showCrewsToolStripMenuItem";
            showCrewsToolStripMenuItem.Size = new Size(204, 22);
            showCrewsToolStripMenuItem.Text = "Show Crews";
            showCrewsToolStripMenuItem.Click += showCrewsToolStripMenuItem_Click;
            // 
            // showTomesToolStripMenuItem
            // 
            showTomesToolStripMenuItem.Checked = true;
            showTomesToolStripMenuItem.CheckState = CheckState.Checked;
            showTomesToolStripMenuItem.Name = "showTomesToolStripMenuItem";
            showTomesToolStripMenuItem.Size = new Size(204, 22);
            showTomesToolStripMenuItem.Text = "Show Lore tomes";
            showTomesToolStripMenuItem.Click += showTomesToolStripMenuItem_Click;
            // 
            // showProjectilesToolStripMenuItem
            // 
            showProjectilesToolStripMenuItem.Checked = true;
            showProjectilesToolStripMenuItem.CheckState = CheckState.Checked;
            showProjectilesToolStripMenuItem.Name = "showProjectilesToolStripMenuItem";
            showProjectilesToolStripMenuItem.Size = new Size(204, 22);
            showProjectilesToolStripMenuItem.Text = "Show Projectiles";
            showProjectilesToolStripMenuItem.Click += showProjectilesToolStripMenuItem_Click;
            // 
            // showOtherStuffToolStripMenuItem
            // 
            showOtherStuffToolStripMenuItem.Name = "showOtherStuffToolStripMenuItem";
            showOtherStuffToolStripMenuItem.Size = new Size(204, 22);
            showOtherStuffToolStripMenuItem.Text = "Show other stuff";
            showOtherStuffToolStripMenuItem.Click += showOtherStuffToolStripMenuItem_Click;
            // 
            // showCannonTrajectoryToolStripMenuItem
            // 
            showCannonTrajectoryToolStripMenuItem.Checked = true;
            showCannonTrajectoryToolStripMenuItem.CheckState = CheckState.Checked;
            showCannonTrajectoryToolStripMenuItem.Name = "showCannonTrajectoryToolStripMenuItem";
            showCannonTrajectoryToolStripMenuItem.Size = new Size(204, 22);
            showCannonTrajectoryToolStripMenuItem.Text = "Show Cannon Trajectory";
            showCannonTrajectoryToolStripMenuItem.Click += showCannonTrajectoryToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { resetTextSizeToolStripMenuItem, startOverlayOnLaunchToolStripMenuItem, crosshairToolStripMenuItem, compassToolStripMenuItem, useFormsOverlayToolStripMenuItem, displayOverlayFPSToolStripMenuItem, keepIdleDisconnectDisabledToolStripMenuItem, setKeybindingsToolStripMenuItem, readOffsetsFromMemoryToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // resetTextSizeToolStripMenuItem
            // 
            resetTextSizeToolStripMenuItem.Name = "resetTextSizeToolStripMenuItem";
            resetTextSizeToolStripMenuItem.Size = new Size(229, 22);
            resetTextSizeToolStripMenuItem.Text = "Adjust Text Size";
            resetTextSizeToolStripMenuItem.Click += resetTextSizeToolStripMenuItem_Click;
            // 
            // startOverlayOnLaunchToolStripMenuItem
            // 
            startOverlayOnLaunchToolStripMenuItem.Checked = true;
            startOverlayOnLaunchToolStripMenuItem.CheckState = CheckState.Checked;
            startOverlayOnLaunchToolStripMenuItem.Name = "startOverlayOnLaunchToolStripMenuItem";
            startOverlayOnLaunchToolStripMenuItem.Size = new Size(229, 22);
            startOverlayOnLaunchToolStripMenuItem.Text = "Start overlay on launch";
            startOverlayOnLaunchToolStripMenuItem.Click += startOverlayOnLaunchToolStripMenuItem_Click;
            // 
            // crosshairToolStripMenuItem
            // 
            crosshairToolStripMenuItem.Checked = true;
            crosshairToolStripMenuItem.CheckState = CheckState.Checked;
            crosshairToolStripMenuItem.Name = "crosshairToolStripMenuItem";
            crosshairToolStripMenuItem.Size = new Size(229, 22);
            crosshairToolStripMenuItem.Text = "Crosshair";
            crosshairToolStripMenuItem.Click += crosshairToolStripMenuItem_Click;
            // 
            // compassToolStripMenuItem
            // 
            compassToolStripMenuItem.Checked = true;
            compassToolStripMenuItem.CheckState = CheckState.Checked;
            compassToolStripMenuItem.Name = "compassToolStripMenuItem";
            compassToolStripMenuItem.Size = new Size(229, 22);
            compassToolStripMenuItem.Text = "Compass";
            compassToolStripMenuItem.Click += compassToolStripMenuItem_Click;
            // 
            // useFormsOverlayToolStripMenuItem
            // 
            useFormsOverlayToolStripMenuItem.Name = "useFormsOverlayToolStripMenuItem";
            useFormsOverlayToolStripMenuItem.Size = new Size(229, 22);
            useFormsOverlayToolStripMenuItem.Text = "Use Forms Overlay";
            useFormsOverlayToolStripMenuItem.Click += useFormsOverlayToolStripMenuItem_Click;
            // 
            // displayOverlayFPSToolStripMenuItem
            // 
            displayOverlayFPSToolStripMenuItem.Name = "displayOverlayFPSToolStripMenuItem";
            displayOverlayFPSToolStripMenuItem.Size = new Size(229, 22);
            displayOverlayFPSToolStripMenuItem.Text = "Display Overlay FPS";
            displayOverlayFPSToolStripMenuItem.Click += displayOverlayFPSToolStripMenuItem_Click;
            // 
            // keepIdleDisconnectDisabledToolStripMenuItem
            // 
            keepIdleDisconnectDisabledToolStripMenuItem.Checked = true;
            keepIdleDisconnectDisabledToolStripMenuItem.CheckState = CheckState.Checked;
            keepIdleDisconnectDisabledToolStripMenuItem.Name = "keepIdleDisconnectDisabledToolStripMenuItem";
            keepIdleDisconnectDisabledToolStripMenuItem.Size = new Size(229, 22);
            keepIdleDisconnectDisabledToolStripMenuItem.Text = "Keep IdleDisconnect Disabled";
            keepIdleDisconnectDisabledToolStripMenuItem.Click += keepIdleDisconnectDisabledToolStripMenuItem_Click;
            // 
            // setKeybindingsToolStripMenuItem
            // 
            setKeybindingsToolStripMenuItem.Name = "setKeybindingsToolStripMenuItem";
            setKeybindingsToolStripMenuItem.Size = new Size(229, 22);
            setKeybindingsToolStripMenuItem.Text = "Set keybindings";
            setKeybindingsToolStripMenuItem.Click += setKeybindingsToolStripMenuItem_Click;
            // 
            // readOffsetsFromMemoryToolStripMenuItem
            // 
            readOffsetsFromMemoryToolStripMenuItem.Name = "readOffsetsFromMemoryToolStripMenuItem";
            readOffsetsFromMemoryToolStripMenuItem.Size = new Size(229, 22);
            readOffsetsFromMemoryToolStripMenuItem.Text = "Read Offsets from Memory";
            readOffsetsFromMemoryToolStripMenuItem.Click += readOffsetsFromMemoryToolStripMenuItem_Click;
            // 
            // debugToolStripMenuItem
            // 
            debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sDKToolToolStripMenuItem, inspectorToolToolStripMenuItem, showSkeletonMeshNamesToolStripMenuItem, disableTreasureMapScalingToolStripMenuItem, resetOverlayToolStripMenuItem });
            debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            debugToolStripMenuItem.Size = new Size(54, 20);
            debugToolStripMenuItem.Text = "Debug";
            // 
            // sDKToolToolStripMenuItem
            // 
            sDKToolToolStripMenuItem.Name = "sDKToolToolStripMenuItem";
            sDKToolToolStripMenuItem.Size = new Size(225, 22);
            sDKToolToolStripMenuItem.Text = "SDK Tool";
            sDKToolToolStripMenuItem.Click += sDKToolToolStripMenuItem_Click_1;
            // 
            // inspectorToolToolStripMenuItem
            // 
            inspectorToolToolStripMenuItem.Name = "inspectorToolToolStripMenuItem";
            inspectorToolToolStripMenuItem.Size = new Size(225, 22);
            inspectorToolToolStripMenuItem.Text = "Inspector Tool";
            inspectorToolToolStripMenuItem.Click += inspectorToolToolStripMenuItem_Click;
            // 
            // showSkeletonMeshNamesToolStripMenuItem
            // 
            showSkeletonMeshNamesToolStripMenuItem.Name = "showSkeletonMeshNamesToolStripMenuItem";
            showSkeletonMeshNamesToolStripMenuItem.Size = new Size(225, 22);
            showSkeletonMeshNamesToolStripMenuItem.Text = "Show SkeletonMeshNames";
            showSkeletonMeshNamesToolStripMenuItem.Click += showSkeletonMeshNamesToolStripMenuItem_Click;
            // 
            // disableTreasureMapScalingToolStripMenuItem
            // 
            disableTreasureMapScalingToolStripMenuItem.Checked = true;
            disableTreasureMapScalingToolStripMenuItem.CheckState = CheckState.Checked;
            disableTreasureMapScalingToolStripMenuItem.Name = "disableTreasureMapScalingToolStripMenuItem";
            disableTreasureMapScalingToolStripMenuItem.Size = new Size(225, 22);
            disableTreasureMapScalingToolStripMenuItem.Text = "Disable Treasure Map scaling";
            disableTreasureMapScalingToolStripMenuItem.Click += disableTreasureMapScalingToolStripMenuItem_Click;
            // 
            // resetOverlayToolStripMenuItem
            // 
            resetOverlayToolStripMenuItem.Name = "resetOverlayToolStripMenuItem";
            resetOverlayToolStripMenuItem.Size = new Size(225, 22);
            resetOverlayToolStripMenuItem.Text = "Restart app";
            resetOverlayToolStripMenuItem.Click += resetOverlayToolStripMenuItem_Click;
            // 
            // questsToolStripMenuItem
            // 
            questsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showTreasureSpotsToolStripMenuItem, showRiddleSpotsToolStripMenuItem });
            questsToolStripMenuItem.Name = "questsToolStripMenuItem";
            questsToolStripMenuItem.Size = new Size(55, 20);
            questsToolStripMenuItem.Text = "Quests";
            // 
            // showTreasureSpotsToolStripMenuItem
            // 
            showTreasureSpotsToolStripMenuItem.Checked = true;
            showTreasureSpotsToolStripMenuItem.CheckState = CheckState.Checked;
            showTreasureSpotsToolStripMenuItem.Name = "showTreasureSpotsToolStripMenuItem";
            showTreasureSpotsToolStripMenuItem.Size = new Size(178, 22);
            showTreasureSpotsToolStripMenuItem.Text = "Show TreasureSpots";
            showTreasureSpotsToolStripMenuItem.Click += showTreasureSpotsToolStripMenuItem_Click_1;
            // 
            // showRiddleSpotsToolStripMenuItem
            // 
            showRiddleSpotsToolStripMenuItem.Checked = true;
            showRiddleSpotsToolStripMenuItem.CheckState = CheckState.Checked;
            showRiddleSpotsToolStripMenuItem.Name = "showRiddleSpotsToolStripMenuItem";
            showRiddleSpotsToolStripMenuItem.Size = new Size(178, 22);
            showRiddleSpotsToolStripMenuItem.Text = "Show RiddleSpots";
            showRiddleSpotsToolStripMenuItem.Click += showRiddleSpotsToolStripMenuItem_Click;
            // 
            // mapToolStripMenuItem
            // 
            mapToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showMapPinsToolStripMenuItem, showMapShipsToolStripMenuItem, showMapTreasureToolStripMenuItem, showAllIslandsToolStripMenuItem, showAllOutpostsToolStripMenuItem, showAllSeaPostsToolStripMenuItem, showAllSeaFortsToolStripMenuItem, showOnlyIslandsWith3kmToolStripMenuItem, showAllSkeletonFortsToolStripMenuItem, removeMarkersToolStripMenuItem });
            mapToolStripMenuItem.Name = "mapToolStripMenuItem";
            mapToolStripMenuItem.Size = new Size(43, 20);
            mapToolStripMenuItem.Text = "Map";
            // 
            // showMapPinsToolStripMenuItem
            // 
            showMapPinsToolStripMenuItem.Name = "showMapPinsToolStripMenuItem";
            showMapPinsToolStripMenuItem.Size = new Size(220, 22);
            showMapPinsToolStripMenuItem.Text = "Show Map Pins";
            showMapPinsToolStripMenuItem.Click += showMapPinsToolStripMenuItem_Click;
            // 
            // showMapShipsToolStripMenuItem
            // 
            showMapShipsToolStripMenuItem.Name = "showMapShipsToolStripMenuItem";
            showMapShipsToolStripMenuItem.Size = new Size(220, 22);
            showMapShipsToolStripMenuItem.Text = "Show Map Ships";
            showMapShipsToolStripMenuItem.Click += showMapShipsToolStripMenuItem_Click;
            // 
            // showMapTreasureToolStripMenuItem
            // 
            showMapTreasureToolStripMenuItem.Name = "showMapTreasureToolStripMenuItem";
            showMapTreasureToolStripMenuItem.Size = new Size(220, 22);
            showMapTreasureToolStripMenuItem.Text = "Show Map Treasure";
            showMapTreasureToolStripMenuItem.Click += showMapTreasureToolStripMenuItem_Click;
            // 
            // showAllIslandsToolStripMenuItem
            // 
            showAllIslandsToolStripMenuItem.Name = "showAllIslandsToolStripMenuItem";
            showAllIslandsToolStripMenuItem.Size = new Size(220, 22);
            showAllIslandsToolStripMenuItem.Text = "Show all islands";
            showAllIslandsToolStripMenuItem.Click += showAllIslandsToolStripMenuItem_Click;
            // 
            // showAllOutpostsToolStripMenuItem
            // 
            showAllOutpostsToolStripMenuItem.Name = "showAllOutpostsToolStripMenuItem";
            showAllOutpostsToolStripMenuItem.Size = new Size(220, 22);
            showAllOutpostsToolStripMenuItem.Text = "Show all outposts";
            showAllOutpostsToolStripMenuItem.Click += showAllOutpostsToolStripMenuItem_Click;
            // 
            // showAllSeaPostsToolStripMenuItem
            // 
            showAllSeaPostsToolStripMenuItem.Name = "showAllSeaPostsToolStripMenuItem";
            showAllSeaPostsToolStripMenuItem.Size = new Size(220, 22);
            showAllSeaPostsToolStripMenuItem.Text = "Show all sea posts";
            showAllSeaPostsToolStripMenuItem.Click += showAllSeaPostsToolStripMenuItem_Click;
            // 
            // showAllSeaFortsToolStripMenuItem
            // 
            showAllSeaFortsToolStripMenuItem.Name = "showAllSeaFortsToolStripMenuItem";
            showAllSeaFortsToolStripMenuItem.Size = new Size(220, 22);
            showAllSeaFortsToolStripMenuItem.Text = "Show all Sea Forts";
            showAllSeaFortsToolStripMenuItem.Click += showAllSeaFortsToolStripMenuItem_Click;
            // 
            // showOnlyIslandsWith3kmToolStripMenuItem
            // 
            showOnlyIslandsWith3kmToolStripMenuItem.Name = "showOnlyIslandsWith3kmToolStripMenuItem";
            showOnlyIslandsWith3kmToolStripMenuItem.Size = new Size(220, 22);
            showOnlyIslandsWith3kmToolStripMenuItem.Text = "Show only islands with 3km";
            showOnlyIslandsWith3kmToolStripMenuItem.Click += showOnlyIslandsWith3kmToolStripMenuItem_Click;
            // 
            // showAllSkeletonFortsToolStripMenuItem
            // 
            showAllSkeletonFortsToolStripMenuItem.Name = "showAllSkeletonFortsToolStripMenuItem";
            showAllSkeletonFortsToolStripMenuItem.Size = new Size(220, 22);
            showAllSkeletonFortsToolStripMenuItem.Text = "Show all Skeleton Forts";
            showAllSkeletonFortsToolStripMenuItem.Click += showAllSkeletonFortsToolStripMenuItem_Click;
            // 
            // removeMarkersToolStripMenuItem
            // 
            removeMarkersToolStripMenuItem.Name = "removeMarkersToolStripMenuItem";
            removeMarkersToolStripMenuItem.Size = new Size(220, 22);
            removeMarkersToolStripMenuItem.Text = "Remove markers";
            removeMarkersToolStripMenuItem.Click += removeMarkersToolStripMenuItem_Click;
            // 
            // botsToolStripMenuItem
            // 
            botsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cookBotToolStripMenuItem, fishingBotToolStripMenuItem, fishToFishToolStripMenuItem });
            botsToolStripMenuItem.Name = "botsToolStripMenuItem";
            botsToolStripMenuItem.Size = new Size(42, 20);
            botsToolStripMenuItem.Text = "Bots";
            // 
            // cookBotToolStripMenuItem
            // 
            cookBotToolStripMenuItem.Name = "cookBotToolStripMenuItem";
            cookBotToolStripMenuItem.Size = new Size(133, 22);
            cookBotToolStripMenuItem.Text = "Cook bot";
            cookBotToolStripMenuItem.Click += cookBotToolStripMenuItem_Click;
            // 
            // fishingBotToolStripMenuItem
            // 
            fishingBotToolStripMenuItem.Name = "fishingBotToolStripMenuItem";
            fishingBotToolStripMenuItem.Size = new Size(133, 22);
            fishingBotToolStripMenuItem.Text = "Fishing bot";
            fishingBotToolStripMenuItem.Click += fishingBotToolStripMenuItem_Click;
            // 
            // fishToFishToolStripMenuItem
            // 
            fishToFishToolStripMenuItem.Name = "fishToFishToolStripMenuItem";
            fishToFishToolStripMenuItem.Size = new Size(133, 22);
            fishToFishToolStripMenuItem.Text = "Fish to fish";
            fishToFishToolStripMenuItem.Click += fishToFishToolStripMenuItem_Click;
            // 
            // IslandComboBox
            // 
            IslandComboBox.FormattingEnabled = true;
            IslandComboBox.Location = new Point(555, 125);
            IslandComboBox.Name = "IslandComboBox";
            IslandComboBox.Size = new Size(391, 23);
            IslandComboBox.TabIndex = 15;
            // 
            // ShowIslandToggleButton
            // 
            ShowIslandToggleButton.Location = new Point(953, 125);
            ShowIslandToggleButton.Name = "ShowIslandToggleButton";
            ShowIslandToggleButton.Size = new Size(84, 23);
            ShowIslandToggleButton.TabIndex = 16;
            ShowIslandToggleButton.Text = "Toggle";
            ShowIslandToggleButton.UseVisualStyleBackColor = true;
            ShowIslandToggleButton.Click += ShowIslandToggleButton_Click;
            // 
            // ClearLogButton
            // 
            ClearLogButton.Location = new Point(544, 530);
            ClearLogButton.Name = "ClearLogButton";
            ClearLogButton.Size = new Size(75, 23);
            ClearLogButton.TabIndex = 17;
            ClearLogButton.Text = "Clear Log";
            ClearLogButton.UseVisualStyleBackColor = true;
            ClearLogButton.Click += ClearLogButton_Click;
            // 
            // UpdateFilterButton
            // 
            UpdateFilterButton.Location = new Point(462, 57);
            UpdateFilterButton.Name = "UpdateFilterButton";
            UpdateFilterButton.Size = new Size(87, 23);
            UpdateFilterButton.TabIndex = 18;
            UpdateFilterButton.Text = "Update Filter";
            UpdateFilterButton.UseVisualStyleBackColor = true;
            UpdateFilterButton.Click += UpdateFilterButton_Click;
            // 
            // DebugTextBox
            // 
            DebugTextBox.Location = new Point(947, 446);
            DebugTextBox.Name = "DebugTextBox";
            DebugTextBox.Size = new Size(100, 23);
            DebugTextBox.TabIndex = 19;
            DebugTextBox.TextChanged += DebugTextBox_TextChanged;
            // 
            // MigrationButton
            // 
            MigrationButton.Location = new Point(648, 503);
            MigrationButton.Name = "MigrationButton";
            MigrationButton.Size = new Size(204, 23);
            MigrationButton.TabIndex = 20;
            MigrationButton.Text = "Disable ShipVelocityMigration";
            MigrationButton.UseVisualStyleBackColor = true;
            MigrationButton.Click += MigrationButton_Click;
            // 
            // RescanButton
            // 
            RescanButton.Location = new Point(953, 90);
            RescanButton.Name = "RescanButton";
            RescanButton.Size = new Size(94, 23);
            RescanButton.TabIndex = 21;
            RescanButton.Text = "Rescan";
            RescanButton.UseVisualStyleBackColor = true;
            RescanButton.Click += RescanButton_Click;
            // 
            // SoTHelper
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1059, 567);
            Controls.Add(RescanButton);
            Controls.Add(MigrationButton);
            Controls.Add(DebugTextBox);
            Controls.Add(UpdateFilterButton);
            Controls.Add(ClearLogButton);
            Controls.Add(ShowIslandToggleButton);
            Controls.Add(IslandComboBox);
            Controls.Add(RemoveAllButton);
            Controls.Add(RemoveButton);
            Controls.Add(AddAllbutton);
            Controls.Add(numericUpDown1);
            Controls.Add(RangeFilterButton);
            Controls.Add(FilterButton);
            Controls.Add(FilterTextBox);
            Controls.Add(AddActorButton);
            Controls.Add(comboBoxActors);
            Controls.Add(TestButton);
            Controls.Add(AddOverlayButton);
            Controls.Add(IdleDC_Button);
            Controls.Add(richTextBox1);
            Controls.Add(DoStuff);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "SoTHelper";
            Text = "SoT Helper";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
        #endregion

        private Button DoStuff;
        private Button IdleDC_Button;
        private System.Windows.Forms.Timer timer1;
        private Button AddOverlayButton;
        private Button TestButton;
        private System.Diagnostics.Process process1;
        private ComboBox comboBoxActors;
        private Button AddActorButton;
        private TextBox FilterTextBox;
        private Button FilterButton;
        private Button RangeFilterButton;
        private NumericUpDown numericUpDown1;
        private Button AddAllbutton;
        private Button RemoveAllButton;
        private Button RemoveButton;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem dumpJsonToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem showStorageToolStripMenuItem;
        private ToolStripMenuItem showStorageItemsToolStripMenuItem;
        private ToolStripMenuItem showPlayersToolStripMenuItem;
        private ToolStripMenuItem showShipsToolStripMenuItem;
        private ToolStripMenuItem showItemsOnYourShipToolStripMenuItem;
        private ToolStripMenuItem showCrewsToolStripMenuItem;
        private ToolStripMenuItem showOtherStuffToolStripMenuItem;
        private ToolStripMenuItem loadSettingsToolStripMenuItem;
        private ToolStripMenuItem saveSettingsToolStripMenuItem;
        private ComboBox IslandComboBox;
        private Button ShowIslandToggleButton;
        private Button ClearLogButton;
        private ToolStripMenuItem showTomesToolStripMenuItem;
        private ToolStripMenuItem updateOffsetsToolStripMenuItem;
        public RichTextBox richTextBox1;
        private Button UpdateFilterButton;
        private ToolStripMenuItem updateSDKDumpPathToolStripMenuItem;
        private ToolStripMenuItem showShipStatusToolStripMenuItem;
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem showSkeletonMeshNamesToolStripMenuItem;
        private ToolStripMenuItem onlyEnemyPlayersToolStripMenuItem;
        private ToolStripMenuItem lessShipInfoToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem resetTextSizeToolStripMenuItem;
        private ToolStripMenuItem startOverlayOnLaunchToolStripMenuItem;
        private ToolStripMenuItem crosshairToolStripMenuItem;
        private ToolStripMenuItem compassToolStripMenuItem;
        private ToolStripMenuItem displayOverlayFPSToolStripMenuItem;
        private ToolStripMenuItem sDKToolToolStripMenuItem;
        private ToolStripMenuItem questsToolStripMenuItem;
        private ToolStripMenuItem showTreasureSpotsToolStripMenuItem;
        private ToolStripMenuItem showRiddleSpotsToolStripMenuItem;
        private ToolStripMenuItem showSkeletonsToolStripMenuItem;
        private ToolStripMenuItem disableTreasureMapScalingToolStripMenuItem;
        private ToolStripMenuItem enemyPlayerTracelinesToolStripMenuItem;
        private ToolStripMenuItem showProjectilesToolStripMenuItem;
        private ToolStripMenuItem resetOverlayToolStripMenuItem;
        private ToolStripMenuItem botsToolStripMenuItem;
        private ToolStripMenuItem cookBotToolStripMenuItem;
        private ToolStripMenuItem fishingBotToolStripMenuItem;
        private ToolStripMenuItem keepIdleDisconnectDisabledToolStripMenuItem;
        private ToolStripMenuItem setKeybindingsToolStripMenuItem;
        private ToolStripMenuItem mapToolStripMenuItem;
        private ToolStripMenuItem showMapPinsToolStripMenuItem;
        private ToolStripMenuItem showMapShipsToolStripMenuItem;
        private ToolStripMenuItem showMapTreasureToolStripMenuItem;
        private ToolStripMenuItem showAllIslandsToolStripMenuItem;
        private ToolStripMenuItem showAllOutpostsToolStripMenuItem;
        private ToolStripMenuItem showAllSeaPostsToolStripMenuItem;
        private ToolStripMenuItem showOnlyIslandsWith3kmToolStripMenuItem;
        private ToolStripMenuItem showAllSeaFortsToolStripMenuItem;
        public TextBox DebugTextBox;
        private ToolStripMenuItem showAllSkeletonFortsToolStripMenuItem;
        private ToolStripMenuItem showResourcesOfShipsToolStripMenuItem;
        private ToolStripMenuItem removeMarkersToolStripMenuItem;
        private Button button1;
        private Button MigrationButton;
        private ToolStripMenuItem showCannonTrajectoryToolStripMenuItem;
        private ToolStripMenuItem readOffsetsFromMemoryToolStripMenuItem;
        private ToolStripMenuItem updateOffsetsFromMemoryToolStripMenuItem;
        private ToolStripMenuItem fishToFishToolStripMenuItem;
        private Button RescanButton;
        private ToolStripMenuItem inspectorToolToolStripMenuItem;
        private ToolStripMenuItem useFormsOverlayToolStripMenuItem;
    }
}