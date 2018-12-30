﻿namespace AutoTracker
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
            this.trackerControl1 = new AutoTracker.TrackerControl();
            this.fswPlayerStatus = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.fswPlayerStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // trackerControl1
            // 
            this.trackerControl1.BackColor = System.Drawing.Color.Black;
            this.trackerControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.trackerControl1.LayoutName = null;
            this.trackerControl1.Location = new System.Drawing.Point(0, 0);
            this.trackerControl1.Name = "trackerControl1";
            this.trackerControl1.Size = new System.Drawing.Size(551, 503);
            this.trackerControl1.TabIndex = 0;
            this.trackerControl1.Text = "trackerControl1";
            this.trackerControl1.Tracker = null;
            this.trackerControl1.IndicatorClicked += new System.EventHandler<AutoTracker.IndicatorEventArgs>(this.trackerControl1_IndicatorClicked);
            this.trackerControl1.MapCellClicked += new System.EventHandler<AutoTracker.GridEventArgs>(this.trackerControl1_MapCellClicked);
            // 
            // fswPlayerStatus
            // 
            this.fswPlayerStatus.EnableRaisingEvents = true;
            this.fswPlayerStatus.NotifyFilter = System.IO.NotifyFilters.LastWrite;
            this.fswPlayerStatus.SynchronizingObject = this;
            this.fswPlayerStatus.Changed += new System.IO.FileSystemEventHandler(this.fswPlayerStatus_Changed);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(906, 503);
            this.Controls.Add(this.trackerControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.fswPlayerStatus)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TrackerControl trackerControl1;
        private System.IO.FileSystemWatcher fswPlayerStatus;
    }
}

