using System;
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

        private JsonTracker trackinator; // = new JsonTracker();

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

            trackinator = new JsonTracker(AutoTracker.Properties.Resources.z1m1Associations);
            //trackinator.AddKeyNode("zelda", "zelda.player.inv");
            //trackinator.AddKeyNode("metroid", "metroid.player.inv.equipment");
            //trackinator.AddKeyNode("zflags", "zelda.flags");
            //trackinator.AddKeyNode("mstats", "metroid.player.inv");
            //trackinator.AddKeyNode("zstats", "zelda.player.stat");

            //trackinator.AddAssociation("wand", "zelda.rod");
            //trackinator.AddAssociation("sword", "zelda.sword");
            //trackinator.AddAssociation("boomerang", "zelda.boom");
            //trackinator.AddAssociation("recorder", "zelda.recorder");
            //trackinator.AddAssociation("bow", "zelda.bow");
            //trackinator.AddAssociation("raft", "zelda.raft");
            //trackinator.AddAssociation("candle", "zelda.candle");
            //trackinator.AddAssociation("ladder", "zelda.ladder");
            //trackinator.AddAssociation("book", "zelda.book");
            //trackinator.AddAssociation("arrow", "zelda.arrows");
            //trackinator.AddAssociation("mkey", "zelda.magickey");
            //trackinator.AddAssociation("bracelet", "zelda.bracelet");
            //trackinator.AddAssociation("ring", "zelda.ring");
            //trackinator.AddAssociation("bait", "zstats.bait");
            //trackinator.AddAssociation("letter", "zstats.letter");
            //trackinator.AddAssociation("sheild", "zstats.shield");
            trackinator.AddAssociation("bomb", "zelda.max_bombs", delegate(string name, int value) { return (value - 8) / 4; });

            trackinator.AddRule("tri1", json => Math.Max(json("zelda.triforces.0") * 3, json("zelda.compasses.0") * 2));
            trackinator.AddRule("tri1", json => Math.Max(json("zelda.triforces.0") * 3, json("zelda.compasses.0") * 2));
            trackinator.AddRule("tri2", json => Math.Max(json("zelda.triforces.1") * 3, json("zelda.compasses.1") * 2));
            trackinator.AddRule("tri3", json => Math.Max(json("zelda.triforces.2") * 3, json("zelda.compasses.2") * 2));
            trackinator.AddRule("tri4", json => Math.Max(json("zelda.triforces.3") * 3, json("zelda.compasses.3") * 2));
            trackinator.AddRule("tri5", json => Math.Max(json("zelda.triforces.4") * 3, json("zelda.compasses.4") * 2));
            trackinator.AddRule("tri6", json => Math.Max(json("zelda.triforces.5") * 3, json("zelda.compasses.5") * 2));
            trackinator.AddRule("tri7", json => Math.Max(json("zelda.triforces.6") * 3, json("zelda.compasses.6") * 2));
            trackinator.AddRule("tri8", json => Math.Max(json("zelda.triforces.7") * 3, json("zelda.compasses.7") * 2));
            trackinator.AddRule("tri9", json => Math.Max(json("zflags.triforce_of_power") * 3, json("zelda.compasses.8") * 2));

            //trackinator.AddAssociation("morphbomb", "metroid.bombs");
            //trackinator.AddAssociation("varia", "metroid.varia");
            //trackinator.AddAssociation("screw", "metroid.screw");
            //trackinator.AddAssociation("morph", "metroid.morph");
            //trackinator.AddAssociation("ice", "metroid.ice");
            //trackinator.AddAssociation("long", "metroid.long");
            //trackinator.AddAssociation("wave", "metroid.wave");
            //trackinator.AddAssociation("hijump", "metroid.jump");

            //trackinator.AddAssociation("kraid", "mstats.totems.K");
            //trackinator.AddAssociation("ridley", "mstats.totems.R");
            //trackinator.AddAssociation("mabrain", ".metroid.flags.finished_tourian");

        }

        internal void setLayout(TrackerLayoutFile layout) {
            this.layout = layout;
            trackerControl1.Tracker = layout;
            trackerControl1.LayoutName = "zelda";
            //this.renderer = new LayoutRenderer(layout, "zelda");
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            //e.Graphics.DrawImage(renderer.Image, 0, 0);
        }

        bool showMet = false;
        protected override void OnClick(EventArgs e) {
            base.OnClick(e);

            //layout.Meta.State.SetIndicatorLevel("tri1", 1);
            //renderer.Update();
            //this.Invalidate();
            showMet = !showMet;
            trackerControl1.LayoutName = showMet ? "metroid" : "zelda";

            HandleStateFileChanged(statusFilePath, 0);
        }

        private void trackerControl1_IndicatorClicked(object sender, IndicatorEventArgs e) {
            //this.Text = e.Name;
            var amt = e.Button == System.Windows.Forms.MouseButtons.Left ? 1 : -1;
            using (var update = trackerControl1.BeginUpdate()) {
                var lvl = update.State.GetIndicatorLevel(e.Name);
                update.State.SetIndicatorLevel(e.Name, Math.Max(0, lvl + amt));
            }
        }

        private void trackerControl1_MapCellClicked(object sender, GridEventArgs e) {
            //this.Text = e.Coords.ToString();
            var amt = e.Button == System.Windows.Forms.MouseButtons.Left ? 1 : -1;
            using (var update = trackerControl1.BeginUpdate()) {
                var lvl = update.State.GetMapLevel(e.Name, e.Coords.X, e.Coords.Y);
                update.State.SetMapLevel(e.Name, e.Coords.X, e.Coords.Y, Math.Max(0, lvl + amt));
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

                //layout.Meta.State.SetIndicatorLevel("sword", (int)zeldaInv["sword"]);
                using (var update = trackerControl1.BeginUpdate()) {
                    //foreach (var prop in zeldaProps) {
                    //    var value = (int)zeldaInv[prop.propName];
                    //    update.State.SetIndicatorLevel(prop.stateName, value);
                    //}
                    //foreach (var prop in metProps) {
                    //    var value = (int)metInv[prop.propName];
                    //    update.State.SetIndicatorLevel(prop.stateName, value);
                    //}
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
    }
}

