using System.Windows.Forms;

namespace SoT_Helper
{
    partial class InspectorForm
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
            components = new System.ComponentModel.Container();
            treeView1 = new TreeView();
            SearchText = new TextBox();
            SearchButton = new Button();
            AttachButton = new Button();
            MemoryLabel = new Label();
            SDKClassComboBox = new ComboBox();
            AttachedTreeView = new TreeView();
            RangeFilterButton = new Button();
            DetectTypeButton = new Button();
            TrackButton = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            AttachPropButton = new Button();
            SuspendLayout();
            // 
            // treeView1
            // 
            treeView1.Location = new Point(22, 90);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(377, 505);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // 
            // SearchText
            // 
            SearchText.Location = new Point(22, 33);
            SearchText.Name = "SearchText";
            SearchText.Size = new Size(171, 23);
            SearchText.TabIndex = 1;
            // 
            // SearchButton
            // 
            SearchButton.Location = new Point(136, 61);
            SearchButton.Name = "SearchButton";
            SearchButton.Size = new Size(57, 23);
            SearchButton.TabIndex = 2;
            SearchButton.Text = "Search";
            SearchButton.UseVisualStyleBackColor = true;
            SearchButton.Click += SearchButton_Click;
            // 
            // AttachButton
            // 
            AttachButton.Location = new Point(324, 607);
            AttachButton.Name = "AttachButton";
            AttachButton.Size = new Size(75, 23);
            AttachButton.TabIndex = 3;
            AttachButton.Text = "Attach";
            AttachButton.UseVisualStyleBackColor = true;
            AttachButton.Click += AttachButton_Click;
            AttachButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // 
            // MemoryLabel
            // 
            MemoryLabel.AutoSize = true;
            MemoryLabel.Location = new Point(22, 72);
            MemoryLabel.Name = "MemoryLabel";
            MemoryLabel.Size = new Size(108, 15);
            MemoryLabel.TabIndex = 4;
            MemoryLabel.Text = "Objects in memory";
            // 
            // SDKClassComboBox
            // 
            SDKClassComboBox.FormattingEnabled = true;
            SDKClassComboBox.Location = new Point(216, 33);
            SDKClassComboBox.Name = "SDKClassComboBox";
            SDKClassComboBox.Size = new Size(272, 23);
            SDKClassComboBox.TabIndex = 5;
            // 
            // AttachedTreeView
            // 
            AttachedTreeView.Location = new Point(494, 33);
            AttachedTreeView.Name = "AttachedTreeView";
            AttachedTreeView.Size = new Size(593, 562);
            AttachedTreeView.TabIndex = 6;
            AttachedTreeView.AfterSelect += AttachedTreeView_AfterSelect;
            AttachedTreeView.DoubleClick += AttachedTreeView_DoubleClick;
            AttachedTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // 
            // RangeFilterButton
            // 
            RangeFilterButton.Location = new Point(199, 61);
            RangeFilterButton.Name = "RangeFilterButton";
            RangeFilterButton.Size = new Size(108, 23);
            RangeFilterButton.TabIndex = 7;
            RangeFilterButton.Text = "Search Within 5m";
            RangeFilterButton.UseVisualStyleBackColor = true;
            RangeFilterButton.Click += RangeFilterButton_Click;
            // 
            // DetectTypeButton
            // 
            DetectTypeButton.Location = new Point(236, 607);
            DetectTypeButton.Name = "DetectTypeButton";
            DetectTypeButton.Size = new Size(82, 23);
            DetectTypeButton.TabIndex = 8;
            DetectTypeButton.Text = "Detect type";
            DetectTypeButton.UseVisualStyleBackColor = true;
            DetectTypeButton.Click += DetectTypeButton_Click;
            DetectTypeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            // 
            // TrackButton
            // 
            TrackButton.Location = new Point(1012, 601);
            TrackButton.Name = "TrackButton";
            TrackButton.Size = new Size(75, 23);
            TrackButton.TabIndex = 9;
            TrackButton.Text = "Track";
            TrackButton.UseVisualStyleBackColor = true;
            TrackButton.Click += TrackButton_Click;
            TrackButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            // 
            // AttachPropButton
            // 
            AttachPropButton.Location = new Point(916, 601);
            AttachPropButton.Name = "AttachPropButton";
            AttachPropButton.Size = new Size(75, 23);
            AttachPropButton.TabIndex = 10;
            AttachPropButton.Text = "Attach";
            AttachPropButton.UseVisualStyleBackColor = true;
            AttachPropButton.Click += AttachPropButton_Click;
            AttachPropButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            // 
            // InspectorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1099, 642);
            Controls.Add(AttachPropButton);
            Controls.Add(TrackButton);
            Controls.Add(DetectTypeButton);
            Controls.Add(RangeFilterButton);
            Controls.Add(AttachedTreeView);
            Controls.Add(SDKClassComboBox);
            Controls.Add(MemoryLabel);
            Controls.Add(AttachButton);
            Controls.Add(SearchButton);
            Controls.Add(SearchText);
            Controls.Add(treeView1);
            Name = "InspectorForm";
            Text = "InspectorForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView treeView1;
        private TextBox SearchText;
        private Button SearchButton;
        private Button AttachButton;
        private Label MemoryLabel;
        private ComboBox SDKClassComboBox;
        private TreeView AttachedTreeView;
        private Button RangeFilterButton;
        private Button DetectTypeButton;
        private Button TrackButton;
        private System.Windows.Forms.Timer timer1;
        private Button AttachPropButton;
    }
}