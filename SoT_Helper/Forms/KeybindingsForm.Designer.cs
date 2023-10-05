namespace SoT_Helper.Forms
{
    partial class KeybindingsForm
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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadKeybindingsToolStripMenuItem = new ToolStripMenuItem();
            saveKeybindingsToolStripMenuItem = new ToolStripMenuItem();
            enableKeybindingsToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(491, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadKeybindingsToolStripMenuItem, saveKeybindingsToolStripMenuItem, enableKeybindingsToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(61, 20);
            fileToolStripMenuItem.Text = "Options";
            // 
            // loadKeybindingsToolStripMenuItem
            // 
            loadKeybindingsToolStripMenuItem.Name = "loadKeybindingsToolStripMenuItem";
            loadKeybindingsToolStripMenuItem.Size = new Size(180, 22);
            loadKeybindingsToolStripMenuItem.Text = "Load keybindings";
            loadKeybindingsToolStripMenuItem.Click += loadKeybindingsToolStripMenuItem_Click;
            // 
            // saveKeybindingsToolStripMenuItem
            // 
            saveKeybindingsToolStripMenuItem.Name = "saveKeybindingsToolStripMenuItem";
            saveKeybindingsToolStripMenuItem.Size = new Size(180, 22);
            saveKeybindingsToolStripMenuItem.Text = "Save keybindings";
            saveKeybindingsToolStripMenuItem.Click += saveKeybindingsToolStripMenuItem_Click;
            // 
            // enableKeybindingsToolStripMenuItem
            // 
            enableKeybindingsToolStripMenuItem.Checked = true;
            enableKeybindingsToolStripMenuItem.CheckState = CheckState.Checked;
            enableKeybindingsToolStripMenuItem.Name = "enableKeybindingsToolStripMenuItem";
            enableKeybindingsToolStripMenuItem.Size = new Size(180, 22);
            enableKeybindingsToolStripMenuItem.Text = "Enable keybindings";
            enableKeybindingsToolStripMenuItem.Click += enableKeybindingsToolStripMenuItem_Click;
            // 
            // KeybindingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(491, 459);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "KeybindingsForm";
            Text = "KeybindingsForm";
            Load += KeybindingsForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem loadKeybindingsToolStripMenuItem;
        private ToolStripMenuItem saveKeybindingsToolStripMenuItem;
        private ToolStripMenuItem enableKeybindingsToolStripMenuItem;
    }
}