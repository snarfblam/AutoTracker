namespace AutoTracker
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
            System.Windows.Forms.ToolStripMenuItem mnuLayouts;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.mnuLayoutFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.fswPlayerStatus = new System.IO.FileSystemWatcher();
            this.burger = new System.Windows.Forms.PictureBox();
            this.trackerMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuZelda = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMetroid = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNoMap = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAutoTrack = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReset = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHideDaBurger = new System.Windows.Forms.ToolStripMenuItem();
            this.mscMetMarkers = new AutoTracker.MarkerSelectorControl();
            this.mscMarkers = new AutoTracker.MarkerSelectorControl();
            this.trackerUI = new AutoTracker.TrackerControl();
            menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            mnuLayouts = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.fswPlayerStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.burger)).BeginInit();
            this.trackerMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.Enabled = false;
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.ShortcutKeyDisplayString = "M";
            menuToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            menuToolStripMenuItem.Text = "Menu";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(185, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(185, 6);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(185, 6);
            // 
            // mnuLayouts
            // 
            mnuLayouts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLayoutFolder});
            mnuLayouts.Name = "mnuLayouts";
            mnuLayouts.Size = new System.Drawing.Size(188, 22);
            mnuLayouts.Text = "Layouts";
            // 
            // mnuLayoutFolder
            // 
            this.mnuLayoutFolder.Image = ((System.Drawing.Image)(resources.GetObject("mnuLayoutFolder.Image")));
            this.mnuLayoutFolder.Name = "mnuLayoutFolder";
            this.mnuLayoutFolder.Size = new System.Drawing.Size(178, 22);
            this.mnuLayoutFolder.Text = "Open Layout Folder";
            this.mnuLayoutFolder.Click += new System.EventHandler(this.mnuLayoutFolder_Click);
            // 
            // fswPlayerStatus
            // 
            this.fswPlayerStatus.EnableRaisingEvents = true;
            this.fswPlayerStatus.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            this.fswPlayerStatus.SynchronizingObject = this;
            this.fswPlayerStatus.Changed += new System.IO.FileSystemEventHandler(this.fswPlayerStatus_Changed);
            // 
            // burger
            // 
            this.burger.Image = ((System.Drawing.Image)(resources.GetObject("burger.Image")));
            this.burger.Location = new System.Drawing.Point(1, 0);
            this.burger.Name = "burger";
            this.burger.Size = new System.Drawing.Size(20, 20);
            this.burger.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.burger.TabIndex = 2;
            this.burger.TabStop = false;
            this.burger.MouseDown += new System.Windows.Forms.MouseEventHandler(this.burger_MouseDown);
            this.burger.MouseUp += new System.Windows.Forms.MouseEventHandler(this.burger_MouseUp);
            // 
            // trackerMenu
            // 
            this.trackerMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            menuToolStripMenuItem,
            toolStripSeparator1,
            this.mnuZelda,
            this.mnuMetroid,
            this.mnuNoMap,
            toolStripSeparator2,
            this.mnuAutoTrack,
            this.mnuReset,
            toolStripSeparator3,
            this.mnuHideDaBurger,
            mnuLayouts});
            this.trackerMenu.Name = "contextMenuStrip1";
            this.trackerMenu.Size = new System.Drawing.Size(189, 198);
            // 
            // mnuZelda
            // 
            this.mnuZelda.Checked = true;
            this.mnuZelda.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuZelda.Name = "mnuZelda";
            this.mnuZelda.ShortcutKeyDisplayString = "Space";
            this.mnuZelda.Size = new System.Drawing.Size(188, 22);
            this.mnuZelda.Text = "&Zelda Map";
            this.mnuZelda.Click += new System.EventHandler(this.mnuZelda_Click);
            // 
            // mnuMetroid
            // 
            this.mnuMetroid.Name = "mnuMetroid";
            this.mnuMetroid.ShortcutKeyDisplayString = "Space";
            this.mnuMetroid.Size = new System.Drawing.Size(188, 22);
            this.mnuMetroid.Text = "&Metroid Map";
            this.mnuMetroid.Click += new System.EventHandler(this.mnuMetroid_Click);
            // 
            // mnuNoMap
            // 
            this.mnuNoMap.Name = "mnuNoMap";
            this.mnuNoMap.ShortcutKeyDisplayString = "Ctrl + Space";
            this.mnuNoMap.Size = new System.Drawing.Size(188, 22);
            this.mnuNoMap.Text = "&No Map";
            this.mnuNoMap.Click += new System.EventHandler(this.mnuNoMap_Click);
            // 
            // mnuAutoTrack
            // 
            this.mnuAutoTrack.Name = "mnuAutoTrack";
            this.mnuAutoTrack.Size = new System.Drawing.Size(188, 22);
            this.mnuAutoTrack.Text = "&Auto-track Inventory";
            this.mnuAutoTrack.Click += new System.EventHandler(this.mnuAutoTrack_Click);
            // 
            // mnuReset
            // 
            this.mnuReset.Name = "mnuReset";
            this.mnuReset.Size = new System.Drawing.Size(188, 22);
            this.mnuReset.Text = "&Reset";
            this.mnuReset.Click += new System.EventHandler(this.mnuReset_Click);
            // 
            // mnuHideDaBurger
            // 
            this.mnuHideDaBurger.Name = "mnuHideDaBurger";
            this.mnuHideDaBurger.ShortcutKeyDisplayString = "Esc";
            this.mnuHideDaBurger.Size = new System.Drawing.Size(188, 22);
            this.mnuHideDaBurger.Text = "&Hide Hamburger";
            this.mnuHideDaBurger.Click += new System.EventHandler(this.mnuHideDaBurger_Click);
            // 
            // mscMetMarkers
            // 
            this.mscMetMarkers.BackColor = System.Drawing.Color.Black;
            this.mscMetMarkers.LayoutFile = null;
            this.mscMetMarkers.LayoutName = null;
            this.mscMetMarkers.Location = new System.Drawing.Point(154, 494);
            this.mscMetMarkers.MarkerSetName = null;
            this.mscMetMarkers.MarkerSetPlacement = null;
            this.mscMetMarkers.Name = "mscMetMarkers";
            this.mscMetMarkers.Scale = 3;
            this.mscMetMarkers.SelectedIndex = 0;
            this.mscMetMarkers.Size = new System.Drawing.Size(240, 30);
            this.mscMetMarkers.TabIndex = 3;
            this.mscMetMarkers.Text = "markerSelectorControl1";
            this.mscMetMarkers.Visible = false;
            // 
            // mscMarkers
            // 
            this.mscMarkers.BackColor = System.Drawing.Color.Black;
            this.mscMarkers.LayoutFile = null;
            this.mscMarkers.LayoutName = null;
            this.mscMarkers.Location = new System.Drawing.Point(18, 494);
            this.mscMarkers.MarkerSetName = null;
            this.mscMarkers.MarkerSetPlacement = null;
            this.mscMarkers.Name = "mscMarkers";
            this.mscMarkers.Scale = 1;
            this.mscMarkers.SelectedIndex = 0;
            this.mscMarkers.Size = new System.Drawing.Size(512, 32);
            this.mscMarkers.TabIndex = 1;
            this.mscMarkers.Text = "markerSelectorControl1";
            // 
            // trackerUI
            // 
            this.trackerUI.BackColor = System.Drawing.Color.Black;
            this.trackerUI.LayoutName = null;
            this.trackerUI.Location = new System.Drawing.Point(1, 0);
            this.trackerUI.Name = "trackerUI";
            this.trackerUI.Size = new System.Drawing.Size(529, 491);
            this.trackerUI.TabIndex = 0;
            this.trackerUI.Text = "trackerControl1";
            this.trackerUI.Tracker = null;
            this.trackerUI.IndicatorClicked += new System.EventHandler<AutoTracker.IndicatorEventArgs>(this.trackerControl1_IndicatorClicked);
            this.trackerUI.MapCellClicked += new System.EventHandler<AutoTracker.GridEventArgs>(this.trackerControl1_MapCellClicked);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(549, 529);
            this.Controls.Add(this.mscMetMarkers);
            this.Controls.Add(this.burger);
            this.Controls.Add(this.mscMarkers);
            this.Controls.Add(this.trackerUI);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Z1M1 Auto Tracker 0.@";
            ((System.ComponentModel.ISupportInitialize)(this.fswPlayerStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.burger)).EndInit();
            this.trackerMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TrackerControl trackerUI;
        private System.IO.FileSystemWatcher fswPlayerStatus;
        private MarkerSelectorControl mscMarkers;
        private System.Windows.Forms.PictureBox burger;
        private System.Windows.Forms.ContextMenuStrip trackerMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuZelda;
        private System.Windows.Forms.ToolStripMenuItem mnuMetroid;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoTrack;
        private System.Windows.Forms.ToolStripMenuItem mnuReset;
        private System.Windows.Forms.ToolStripMenuItem mnuHideDaBurger;
        private MarkerSelectorControl mscMetMarkers;
        private System.Windows.Forms.ToolStripMenuItem mnuNoMap;
        private System.Windows.Forms.ToolStripMenuItem mnuLayoutFolder;
    }
}

