﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AutoTracker
{
    public partial class Form1 : Form
    {
        TrackerLayoutFile layout;
        Scheduler timer;

        private TrackerEngine trackinator; // = new JsonTracker();

        string statusFilePath;



        public Form1() {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var z1m1 = Path.Combine(localAppData, "z1m1");

            fswPlayerStatus.Path = z1m1;
            fswPlayerStatus.Filter = "z1m1.main.state";

            timer = new Scheduler(this);

            statusFilePath = Path.Combine(z1m1, "z1m1.main.state");

            trackinator = new TrackerEngine(AutoTracker.Properties.Resources.z1m1Associations);
            trackinator.AddAssociation("bomb", "zelda.max_bombs", delegate(string name, int value) { return (value - 8) / 4; });

            trackinator.AddRule("tri1", json => Math.Max(json("zelda.triforces.0") * 2, json("zelda.compasses.0") ));
            trackinator.AddRule("tri1", json => Math.Max(json("zelda.triforces.0") * 2, json("zelda.compasses.0") ));
            trackinator.AddRule("tri2", json => Math.Max(json("zelda.triforces.1") * 2, json("zelda.compasses.1") ));
            trackinator.AddRule("tri3", json => Math.Max(json("zelda.triforces.2") * 2, json("zelda.compasses.2") ));
            trackinator.AddRule("tri4", json => Math.Max(json("zelda.triforces.3") * 2, json("zelda.compasses.3") ));
            trackinator.AddRule("tri5", json => Math.Max(json("zelda.triforces.4") * 2, json("zelda.compasses.4") ));
            trackinator.AddRule("tri6", json => Math.Max(json("zelda.triforces.5") * 2, json("zelda.compasses.5") ));
            trackinator.AddRule("tri7", json => Math.Max(json("zelda.triforces.6") * 2, json("zelda.compasses.6") ));
            trackinator.AddRule("tri8", json => Math.Max(json("zelda.triforces.7") * 2, json("zelda.compasses.7") ));
            trackinator.AddRule("tri9", json => Math.Max(json("zflags.triforce_of_power") * 2, json("zelda.compasses.8")));


            this.Text = this.Text.Replace("@", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString());
            LoadSettings();
            SetLayoutSelection(Program.Settings.layout);

        }

        internal void setLayout(TrackerLayoutFile layout) {
            this.layout = layout;
            trackerUI.Tracker = layout;
            trackerUI.LayoutName = "zelda";
            ApplyLayoutExternalMetrics(layout.layouts["zelda"]);

            mscMarkers.ClearLayout();
            mscMarkers.LayoutFile = layout;
            mscMarkers.LayoutName = "zelda";
            mscMarkers.MarkerSetPlacement = layout.layouts["zelda"].maps[0].markerSets[0];
            //mscMarkers.MarkerSetName = mscMarkers.MarkerSetPlacement.name;
            mscMarkers.MapPlacement = layout.layouts["zelda"].maps[0];

            mscMetMarkers.ClearLayout();
            mscMetMarkers.LayoutFile = layout;
            mscMetMarkers.LayoutName = "metroid";
            mscMetMarkers.MarkerSetPlacement = layout.layouts["metroid"].maps[0].markerSets[0];
            //mscMetMarkers.MarkerSetName = mscMetMarkers.MarkerSetPlacement.name;
            mscMetMarkers.MapPlacement = layout.layouts["metroid"].maps[0];

            //LoadSettings();

        }

        private void ApplyLayoutExternalMetrics(TrackerLayout layout) {
            var backcolor = layout.GetBackcolor();
            if (backcolor != null) {
                //this.BackColor = 
            }

        }

        private void LoadSettings() {
            SetCheeseburgerVisible(!Program.Settings.hideBurger);
            SetTrackingMode(Program.Settings.autoTrack);
        }

        private void SetLayoutSelection(string layout) {
            Program.Settings.layout = layout;
            if (layout == Settings.Layouts.Custom) {
                mnuLayoutCustom.Checked = true;
                mnuLayoutZ1m1.Checked = false;

                string layoutData, layoutRoot;
                Program.LoadExternalLayoutFile(out layoutData, out layoutRoot);
                var layoutObj = LayoutFileParser.Parse(layoutData, true);

                if (layoutRoot != null) {
                    layoutObj.Meta.RootPath = layoutRoot;
                }

                setLayout(layoutObj);
            } else { //if (layout == Settings.Layouts.Z1M1) { 
                // Catch all
                Program.Settings.layout = Settings.Layouts.Z1M1;
                mnuLayoutCustom.Checked = false;
                mnuLayoutZ1m1.Checked = true;
                setLayout(LayoutFileParser.Parse(AutoTracker.Properties.Resources.z1m1PackedLayout, true));
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            //e.Graphics.DrawImage(renderer.Image, 0, 0);
        }

        bool showMet = false;
        protected override void OnClick(EventArgs e) {
            base.OnClick(e);
            //ToggleMaps();
        }

        private void ToggleMaps(bool hide) {
            if (hide) {
                mnuNoMap.Checked = true;
            } else {
                if (!mnuNoMap.Checked) {
                    showMet = !showMet;
                }
                mnuNoMap.Checked = false;
            }
            string layoutName = showMet ? "metroid" : "zelda";
            if (mnuNoMap.Checked) layoutName = "nomap";
            trackerUI.LayoutName = layoutName;
            mnuZelda.Checked = !showMet;
            mnuMetroid.Checked = showMet;
            //mscMarkers.Visible = !showMet;
            //mscMetMarkers.Visible = showMet;

            SetFormSize();

            if (mnuAutoTrack.Checked) {
                HandleStateFileChanged(statusFilePath, 0);
            }
        }

        private void HideMap() {
            mnuZelda.Checked = false;
            mnuMetroid.Checked = false;
            mnuNoMap.Checked = true;
            //SetFormSize();
            ToggleMaps(true);
        }

        private void SetFormSize() {
            if (mnuNoMap.Checked) {
                //if (this.ClientSize.Height != 184) {
                //    this.ClientSize = new Size(this.ClientSize.Width, 184);
                //}
                this.ClientSize = trackerUI.GetPreferredSize(this.ClientSize);

            } else {
                this.ClientSize = trackerUI.GetPreferredSize(this.ClientSize);
                //if (this.ClientSize.Height != 529) {
                //    this.ClientSize = new Size(this.ClientSize.Width, 529);
                //}
            }
        }
        private void trackerControl1_IndicatorClicked(object sender, IndicatorEventArgs e) {
            //this.Text = e.Name;
            var amt = e.Button == System.Windows.Forms.MouseButtons.Left ? 1 : -1;
            using (var update = trackerUI.BeginUpdate()) {
                var maximum = getMaxValue(e.Name) ?? 1;

                var lvl = update.State.GetIndicatorLevel(e.Name) + amt;
                lvl = Math.Max(0, Math.Min(lvl, maximum));

                update.State.SetIndicatorLevel(e.Name, lvl);
            }
        }

        int? getMaxValue(string indicatorName) {
            TrackerIndicator i;
            if (layout.layouts[trackerUI.LayoutName].indicators.TryGetValue(indicatorName, out i)) {
                return i.max;
            }
            return null;
        }

        private void trackerControl1_MapCellClicked(object sender, GridEventArgs e) {
            var zelda = mnuZelda.Checked;
            var markerSet = zelda ? "zmarkers" : "mmarkers";

            var miniMarkerClick = zelda && (e.Button == MouseButtons.Middle | (ModifierKeys == Keys.Control && e.Button == MouseButtons.Left));
            var markerClick = e.Button == System.Windows.Forms.MouseButtons.Right;
            var stateClick = !miniMarkerClick && e.Button == System.Windows.Forms.MouseButtons.Left;

            if (miniMarkerClick) {
                using (var update = trackerUI.BeginUpdate()) {
                    var currentValue = -1;
                    var currentMarkers = update.State.GetMiniMarkers(markerSet, e.Coords.X, e.Coords.Y, e.Quadrant);
                    foreach (var m in currentMarkers) currentValue = m;

                    update.State.ClearMiniMarker(markerSet, e.Coords.X, e.Coords.Y, e.Quadrant);

                    var newValue = trackerUI.SelectedMarker;
                    if (currentValue == -1) {
                        if (zelda) {
                            update.State.AddMiniMarker(markerSet, e.Coords.X, e.Coords.Y, e.Quadrant, newValue);
                        } else {
                            update.State.AddMarker(markerSet, e.Coords.X, e.Coords.Y, newValue);
                        }
                    }
                }
            } else if (markerClick) {
                using (var update = trackerUI.BeginUpdate()) {
                    //if (mnuZelda.Checked) {
                        var currentValue = -1;
                        var currentMarkers = update.State.GetAllMarkers(markerSet, e.Coords.X, e.Coords.Y);
                        foreach (var m in currentMarkers) currentValue = m.Value;

                        update.State.ClearAnyMarker(markerSet, e.Coords.X, e.Coords.Y);

                        var newValue = trackerUI.SelectedMarker;
                        if (currentValue == -1) {
                            update.State.AddMarker(markerSet, e.Coords.X, e.Coords.Y, newValue);
                        }
                    //} else {
                    //    var currentValue = -1;
                    //    var currentMarkers = update.State.GetMarkers(markerSet, e.Coords.X, e.Coords.Y);
                    //    foreach (var m in currentMarkers) currentValue = m;

                    //    update.State.ClearAnyMarker(markerSet, e.Coords.X, e.Coords.Y);

                    //    var newValue = trackerUI.SelectedMarker;
                    //    if (newValue != currentValue) {
                    //        update.State.AddMarker(markerSet, e.Coords.X, e.Coords.Y, newValue);
                    //    }
                    //}
                }
            } else if (stateClick) {
                var amt = e.Button == System.Windows.Forms.MouseButtons.Left ? 1 : -1;
                using (var update = trackerUI.BeginUpdate()) {
                    var lvl = 1 - update.State.GetMapLevel(e.Name, e.Coords.X, e.Coords.Y);
                    update.State.SetMapLevel(e.Name, e.Coords.X, e.Coords.Y, Math.Max(0, lvl));
                }
            }

        }

        private void fswPlayerStatus_Changed(object sender, FileSystemEventArgs e) {
            HandleStateFileChanged(e.FullPath, 0);
        }

        static double[] fileAccessDelays = { 250, 500, 1000, 5000 }; // Subsequent delay, in milliseconds, before attempting again to access the user's status

        private void HandleStateFileChanged(string path, int attemptNum) {
            bool retry = false;
            byte[] file = null;
            
            try {
                file = File.ReadAllBytes(path);
            } catch (FileNotFoundException x) {
                LogStateFileError(x);
            } catch (DirectoryNotFoundException x) {
                LogStateFileError(x);
            } catch (PathTooLongException x) {
                LogStateFileError(x);
            } catch (System.Security.SecurityException x) {
                LogStateFileError(x);
            } catch (NotSupportedException x) {
                LogStateFileError(x);
            } catch (UnauthorizedAccessException x) {
                retry = true;
            } catch (IOException x) {
                retry = true;
            }

            if (retry) {// Have we hit our threshold for number of attempts?
                if (attemptNum < fileAccessDelays.Length) {
                    // If not, queue the next attempt
                    var handler = (TickAction)delegate() {
                        HandleStateFileChanged(path, attemptNum + 1);
                    };
                    var delay = fileAccessDelays[attemptNum];
                    timer.Queue(handler, delay);
                }
            } else if (file != null) {
                deobfuscate(file);
                var fileText = System.Text.Encoding.UTF8.GetString(file);
                //Clipboard.SetText(fileText);
                var state = JObject.Parse(fileText);

                var zeldaInv = state["zelda"]["player"]["inv"];
                var metInv = state["metroid"]["player"]["inv"]["equipment"];

                using (var update = trackerUI.BeginUpdate()) {
                    trackinator.Process(state, update.State);
                }
            }

        }
        private void LogStateFileError(Exception x) {
            this.Text = x.GetType().Name + " occurred: " + x.Message;
        }

        void deobfuscate(byte[] file) {
            int obfuscosity = 7;
            for (int i = 0; i < file.Length; i++) {
                file[i] = (byte)(file[i] ^ obfuscosity);
                obfuscosity = 0xFF & (7 + obfuscosity);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e) {
            if (e.KeyChar == ' ') {

            } else if (e.KeyChar == 'M' || e.KeyChar == 'm') {
                e.Handled = true;
                trackerMenu.Show(this, 0, 0);
            }

            base.OnKeyPress(e);
        }
        protected override void OnKeyDown(KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape && !trackerMenu.Visible) {
                e.Handled = true;
                SetCheeseburgerVisible(!burger.Visible);
            } else if (e.KeyCode == Keys.Space) {
                if (ModifierKeys == Keys.Control) {
                    HideMap();
                } else {
                    ToggleMaps(false);
                }
                e.Handled = true;
            }
            base.OnKeyUp(e);
        }

        private void SetCheeseburgerVisible(bool visible) {
            burger.Visible = visible;
            mnuHideDaBurger.Checked = !visible;
            Program.Settings.hideBurger = !visible;
        }

        private void burger_MouseUp(object sender, MouseEventArgs e) {
            trackerMenu.Show(burger, e.Location);
        }

        private void burger_MouseDown(object sender, MouseEventArgs e) {

        }

        private void mnuZelda_Click(object sender, EventArgs e) {
            if (!mnuZelda.Checked) ToggleMaps(false);
        }

        private void mnuMetroid_Click(object sender, EventArgs e) {
            if (!mnuMetroid.Checked) ToggleMaps(false);
        }

        private void mnuHideDaBurger_Click(object sender, EventArgs e) {
            SetCheeseburgerVisible(!burger.Visible);
        }

        private void mnuAutoTrack_Click(object sender, EventArgs e) {
            bool willTrack = !mnuAutoTrack.Checked;

            SetTrackingMode(willTrack);
        }

        private void SetTrackingMode(bool willTrack) {
            mnuAutoTrack.Checked = willTrack;
            fswPlayerStatus.EnableRaisingEvents = willTrack;

            if (willTrack) {
                HandleStateFileChanged(statusFilePath, 0);
            }

            Program.Settings.autoTrack = willTrack;
        }

        private void mnuNoMap_Click(object sender, EventArgs e) {
            HideMap();
        }


        private void mnuReset_Click(object sender, EventArgs e) {
            trackerUI.ResetTracker();
        }

        private void mnuLayoutFolder_Click(object sender, EventArgs e) {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(Program.appDataPath);
            start.UseShellExecute = true;
            System.Diagnostics.Process.Start(start);
        }

        private void mnuLayoutZ1m1_Click(object sender, EventArgs e) {
            SetLayoutSelection(Settings.Layouts.Z1M1);
        }

        private void mnuLayoutCustom_Click(object sender, EventArgs e) {
            SetLayoutSelection(Settings.Layouts.Custom);
        }


    }
}

