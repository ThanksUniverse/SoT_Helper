namespace SoT_Helper
{
    partial class SDK_ExploreerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            treeView1 = new TreeView();
            ExpandNodeButton = new Button();
            menuStrip1 = new MenuStrip();
            filesToolStripMenuItem1 = new ToolStripMenuItem();
            saveOffsetsjsonToolStripMenuItem = new ToolStripMenuItem();
            filesToolStripMenuItem = new ToolStripMenuItem();
            listOffsetsjsonItemsToolStripMenuItem = new ToolStripMenuItem();
            listAllSDKScriptFilesToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            showPropertiesToolStripMenuItem = new ToolStripMenuItem();
            showFunctionsToolStripMenuItem = new ToolStripMenuItem();
            showParentToolStripMenuItem = new ToolStripMenuItem();
            showChildrenToolStripMenuItem = new ToolStripMenuItem();
            inspectToolStripMenuItem = new ToolStripMenuItem();
            attachClassToMemToolStripMenuItem = new ToolStripMenuItem();
            focusNodeButton = new Button();
            onlyExpandedButton = new Button();
            SearchTextBox = new TextBox();
            SearchSDKButton = new Button();
            treeViewSearchResults = new TreeView();
            SearchComboBox = new ComboBox();
            FindParentButton = new Button();
            FindChilrenButton = new Button();
            ClearButton = new Button();
            AddOffsetButton = new Button();
            RemoveOffsetButton = new Button();
            SearchRangeComboBox = new ComboBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            treeView1.Location = new Point(23, 36);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(692, 742);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.DoubleClick += treeView1_DoubleClick;
            // 
            // ExpandNodeButton
            // 
            ExpandNodeButton.Location = new Point(721, 36);
            ExpandNodeButton.Name = "ExpandNodeButton";
            ExpandNodeButton.Size = new Size(97, 23);
            ExpandNodeButton.TabIndex = 1;
            ExpandNodeButton.Text = "Expand Node";
            ExpandNodeButton.UseVisualStyleBackColor = true;
            ExpandNodeButton.Click += ExpandNodeButton_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { filesToolStripMenuItem1, filesToolStripMenuItem, viewToolStripMenuItem, inspectToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1599, 24);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // filesToolStripMenuItem1
            // 
            filesToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { saveOffsetsjsonToolStripMenuItem });
            filesToolStripMenuItem1.Name = "filesToolStripMenuItem1";
            filesToolStripMenuItem1.Size = new Size(42, 20);
            filesToolStripMenuItem1.Text = "Files";
            // 
            // saveOffsetsjsonToolStripMenuItem
            // 
            saveOffsetsjsonToolStripMenuItem.Name = "saveOffsetsjsonToolStripMenuItem";
            saveOffsetsjsonToolStripMenuItem.Size = new Size(161, 22);
            saveOffsetsjsonToolStripMenuItem.Text = "Save offsets.json";
            saveOffsetsjsonToolStripMenuItem.Click += saveOffsetsjsonToolStripMenuItem_Click;
            // 
            // filesToolStripMenuItem
            // 
            filesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { listOffsetsjsonItemsToolStripMenuItem, listAllSDKScriptFilesToolStripMenuItem });
            filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            filesToolStripMenuItem.Size = new Size(63, 20);
            filesToolStripMenuItem.Text = "SDK lists";
            // 
            // listOffsetsjsonItemsToolStripMenuItem
            // 
            listOffsetsjsonItemsToolStripMenuItem.Name = "listOffsetsjsonItemsToolStripMenuItem";
            listOffsetsjsonItemsToolStripMenuItem.Size = new Size(189, 22);
            listOffsetsjsonItemsToolStripMenuItem.Text = "List Offsets.json items";
            listOffsetsjsonItemsToolStripMenuItem.Click += listOffsetsjsonItemsToolStripMenuItem_Click;
            // 
            // listAllSDKScriptFilesToolStripMenuItem
            // 
            listAllSDKScriptFilesToolStripMenuItem.Name = "listAllSDKScriptFilesToolStripMenuItem";
            listAllSDKScriptFilesToolStripMenuItem.Size = new Size(189, 22);
            listAllSDKScriptFilesToolStripMenuItem.Text = "List all SDK script files";
            listAllSDKScriptFilesToolStripMenuItem.Click += listAllSDKScriptFilesToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showPropertiesToolStripMenuItem, showFunctionsToolStripMenuItem, showParentToolStripMenuItem, showChildrenToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // showPropertiesToolStripMenuItem
            // 
            showPropertiesToolStripMenuItem.Checked = true;
            showPropertiesToolStripMenuItem.CheckOnClick = true;
            showPropertiesToolStripMenuItem.CheckState = CheckState.Checked;
            showPropertiesToolStripMenuItem.Name = "showPropertiesToolStripMenuItem";
            showPropertiesToolStripMenuItem.Size = new Size(159, 22);
            showPropertiesToolStripMenuItem.Text = "Show Properties";
            showPropertiesToolStripMenuItem.Click += showPropertiesToolStripMenuItem_Click;
            // 
            // showFunctionsToolStripMenuItem
            // 
            showFunctionsToolStripMenuItem.Checked = true;
            showFunctionsToolStripMenuItem.CheckOnClick = true;
            showFunctionsToolStripMenuItem.CheckState = CheckState.Checked;
            showFunctionsToolStripMenuItem.Name = "showFunctionsToolStripMenuItem";
            showFunctionsToolStripMenuItem.Size = new Size(159, 22);
            showFunctionsToolStripMenuItem.Text = "Show Functions";
            showFunctionsToolStripMenuItem.Click += showFunctionsToolStripMenuItem_Click;
            // 
            // showParentToolStripMenuItem
            // 
            showParentToolStripMenuItem.Checked = true;
            showParentToolStripMenuItem.CheckOnClick = true;
            showParentToolStripMenuItem.CheckState = CheckState.Checked;
            showParentToolStripMenuItem.Name = "showParentToolStripMenuItem";
            showParentToolStripMenuItem.Size = new Size(159, 22);
            showParentToolStripMenuItem.Text = "Show Parent";
            showParentToolStripMenuItem.Click += showParentToolStripMenuItem_Click;
            // 
            // showChildrenToolStripMenuItem
            // 
            showChildrenToolStripMenuItem.CheckOnClick = true;
            showChildrenToolStripMenuItem.Enabled = false;
            showChildrenToolStripMenuItem.Name = "showChildrenToolStripMenuItem";
            showChildrenToolStripMenuItem.Size = new Size(159, 22);
            showChildrenToolStripMenuItem.Text = "Show Children";
            // 
            // inspectToolStripMenuItem
            // 
            inspectToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { attachClassToMemToolStripMenuItem });
            inspectToolStripMenuItem.Name = "inspectToolStripMenuItem";
            inspectToolStripMenuItem.Size = new Size(57, 20);
            inspectToolStripMenuItem.Text = "Inspect";
            // 
            // attachClassToMemToolStripMenuItem
            // 
            attachClassToMemToolStripMenuItem.Name = "attachClassToMemToolStripMenuItem";
            attachClassToMemToolStripMenuItem.Size = new Size(180, 22);
            attachClassToMemToolStripMenuItem.Text = "Inspector Tool";
            attachClassToMemToolStripMenuItem.Click += attachClassToMemToolStripMenuItem_Click;
            // 
            // focusNodeButton
            // 
            focusNodeButton.Location = new Point(721, 184);
            focusNodeButton.Name = "focusNodeButton";
            focusNodeButton.Size = new Size(97, 23);
            focusNodeButton.TabIndex = 3;
            focusNodeButton.Text = "Focus on Node";
            focusNodeButton.UseVisualStyleBackColor = true;
            focusNodeButton.Click += focusNodeButton_Click;
            // 
            // onlyExpandedButton
            // 
            onlyExpandedButton.Location = new Point(721, 213);
            onlyExpandedButton.Name = "onlyExpandedButton";
            onlyExpandedButton.Size = new Size(97, 23);
            onlyExpandedButton.TabIndex = 4;
            onlyExpandedButton.Text = "Only expanded";
            onlyExpandedButton.UseVisualStyleBackColor = true;
            onlyExpandedButton.Click += onlyExpandedButton_Click;
            // 
            // SearchTextBox
            // 
            SearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchTextBox.Location = new Point(1430, 36);
            SearchTextBox.Name = "SearchTextBox";
            SearchTextBox.Size = new Size(157, 23);
            SearchTextBox.TabIndex = 5;
            // 
            // SearchSDKButton
            // 
            SearchSDKButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchSDKButton.Location = new Point(1430, 64);
            SearchSDKButton.Name = "SearchSDKButton";
            SearchSDKButton.Size = new Size(157, 23);
            SearchSDKButton.TabIndex = 6;
            SearchSDKButton.Text = "Search SDK";
            SearchSDKButton.UseVisualStyleBackColor = true;
            SearchSDKButton.Click += SearchSDKButton_Click;
            // 
            // treeViewSearchResults
            // 
            treeViewSearchResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewSearchResults.Location = new Point(824, 35);
            treeViewSearchResults.Name = "treeViewSearchResults";
            treeViewSearchResults.Size = new Size(600, 743);
            treeViewSearchResults.TabIndex = 11;
            treeViewSearchResults.AfterSelect += treeViewSearchResults_AfterSelect;
            treeViewSearchResults.NodeMouseDoubleClick += treeViewSearchResults_NodeMouseDoubleClick;
            // 
            // SearchComboBox
            // 
            SearchComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchComboBox.FormattingEnabled = true;
            SearchComboBox.Location = new Point(1430, 93);
            SearchComboBox.Name = "SearchComboBox";
            SearchComboBox.Size = new Size(157, 23);
            SearchComboBox.TabIndex = 12;
            SearchComboBox.SelectedIndexChanged += SearchComboBox_SelectedIndexChanged;
            // 
            // FindParentButton
            // 
            FindParentButton.Location = new Point(721, 64);
            FindParentButton.Name = "FindParentButton";
            FindParentButton.Size = new Size(97, 23);
            FindParentButton.TabIndex = 13;
            FindParentButton.Text = "Find Parent";
            FindParentButton.UseVisualStyleBackColor = true;
            // 
            // FindChilrenButton
            // 
            FindChilrenButton.Location = new Point(721, 93);
            FindChilrenButton.Name = "FindChilrenButton";
            FindChilrenButton.Size = new Size(97, 23);
            FindChilrenButton.TabIndex = 14;
            FindChilrenButton.Text = "Find Children";
            FindChilrenButton.UseVisualStyleBackColor = true;
            // 
            // ClearButton
            // 
            ClearButton.Location = new Point(721, 242);
            ClearButton.Name = "ClearButton";
            ClearButton.Size = new Size(97, 23);
            ClearButton.TabIndex = 15;
            ClearButton.Text = "Clear";
            ClearButton.UseVisualStyleBackColor = true;
            ClearButton.Click += ClearButton_Click;
            // 
            // AddOffsetButton
            // 
            AddOffsetButton.Location = new Point(721, 336);
            AddOffsetButton.Name = "AddOffsetButton";
            AddOffsetButton.Size = new Size(97, 23);
            AddOffsetButton.TabIndex = 16;
            AddOffsetButton.Text = "Add Offset";
            AddOffsetButton.UseVisualStyleBackColor = true;
            AddOffsetButton.Click += AddOffsetButton_Click;
            // 
            // RemoveOffsetButton
            // 
            RemoveOffsetButton.Location = new Point(721, 365);
            RemoveOffsetButton.Name = "RemoveOffsetButton";
            RemoveOffsetButton.Size = new Size(97, 23);
            RemoveOffsetButton.TabIndex = 17;
            RemoveOffsetButton.Text = "Remove Offset";
            RemoveOffsetButton.UseVisualStyleBackColor = true;
            // 
            // SearchRangeComboBox
            // 
            SearchRangeComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SearchRangeComboBox.FormattingEnabled = true;
            SearchRangeComboBox.Location = new Point(1430, 122);
            SearchRangeComboBox.Name = "SearchRangeComboBox";
            SearchRangeComboBox.Size = new Size(157, 23);
            SearchRangeComboBox.TabIndex = 18;
            // 
            // SDK_ExploreerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1599, 790);
            Controls.Add(SearchRangeComboBox);
            Controls.Add(RemoveOffsetButton);
            Controls.Add(AddOffsetButton);
            Controls.Add(ClearButton);
            Controls.Add(FindChilrenButton);
            Controls.Add(FindParentButton);
            Controls.Add(SearchComboBox);
            Controls.Add(treeViewSearchResults);
            Controls.Add(SearchSDKButton);
            Controls.Add(SearchTextBox);
            Controls.Add(onlyExpandedButton);
            Controls.Add(focusNodeButton);
            Controls.Add(ExpandNodeButton);
            Controls.Add(treeView1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "SDK_ExploreerForm";
            Text = "SoT SDK Explorer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private void treeViewSearchResults_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ExpandSelectedNode(e.Node);
        }

        #endregion

        private TreeView treeView1;
        private Button ExpandNodeButton;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem filesToolStripMenuItem;
        private ToolStripMenuItem listOffsetsjsonItemsToolStripMenuItem;
        private ToolStripMenuItem listAllSDKScriptFilesToolStripMenuItem;
        private Button focusNodeButton;
        private Button onlyExpandedButton;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem showPropertiesToolStripMenuItem;
        private ToolStripMenuItem showFunctionsToolStripMenuItem;
        private ToolStripMenuItem showParentToolStripMenuItem;
        private ToolStripMenuItem showChildrenToolStripMenuItem;
        private TextBox SearchTextBox;
        private Button SearchSDKButton;
        private TreeView treeViewSearchResults;
        private ComboBox SearchComboBox;
        private Button FindParentButton;
        private Button FindChilrenButton;
        private Button ClearButton;
        private Button AddOffsetButton;
        private Button RemoveOffsetButton;
        private ToolStripMenuItem filesToolStripMenuItem1;
        private ToolStripMenuItem saveOffsetsjsonToolStripMenuItem;
        private ComboBox SearchRangeComboBox;
        private ToolStripMenuItem inspectToolStripMenuItem;
        private ToolStripMenuItem attachClassToMemToolStripMenuItem;
    }
}