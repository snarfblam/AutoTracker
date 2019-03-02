using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AutoTracker.Properties;
using System.IO;
using Newtonsoft.Json;

namespace AutoTracker
{
    static class Program
    {
        public static Random rand = new Random();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            if (args.Length > 0) {
                if (args[0].ToLower() == "pack") {
                    if (args.Length == 1 || args.Length > 3) {
                        if(args.Length == 1) Console.Write("Missing required parameter.");
                        if(args.Length > 3) Console.Write("Too many parameters.");
                        Console.Write("Syntax:");
                        Console.Write("    autotracker pack inputFile [outputFile]");
                    }

                    string input = args[1];
                    string output = "packedLayout.json";
                    if (args.Length > 2) output = args[2];
                    Packer.PackFile(input, output);
                    
                }
            } else {
                RunApplication();
            }
        }

        private static void RunApplication() {
            LoadSettings();

            //var layoutFile = Resources.z1m1Layout;
            string layoutFileDirectory;
            string layoutFileContents;

            LoadExternalLayoutFile(out layoutFileContents, out layoutFileDirectory);

            var layout = LayoutFileParser.Parse(layoutFileContents, true);
            if (layoutFileDirectory != null) {
                layout.Meta.RootPath = layoutFileDirectory;
            }


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainFrm = new Form1();
            mainFrm.setLayout(layout);

            Application.Run(mainFrm);

            SaveSettings();
        }

        /// <summary>
        /// Loads the external layout file, if present. Falls back to the internal layout file if no external layout is found.
        /// </summary>
        /// <param name="layoutFileContents"></param>
        /// <param name="layoutFileDirectory"></param>
        public static void LoadExternalLayoutFile(out string layoutFileContents, out string layoutFileDirectory) {
            string externalLayoutDirectory = appDataPath;
            string externalLayoutFile = Path.Combine(externalLayoutDirectory, "layout.json");

            if (File.Exists(externalLayoutFile)) {
                layoutFileDirectory = externalLayoutDirectory;
                layoutFileContents = File.ReadAllText(externalLayoutFile);
            } else {
                layoutFileDirectory = null;
                layoutFileContents = Resources.z1m1PackedLayout;
            }
        }

        public static readonly string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Z1M1AutoTracker");
        public static readonly string settingsFilePath = Path.Combine(appDataPath, "settings.json");
        public static Settings Settings { get; private set; }

        static void LoadSettings() {
            try {
                EnsureDirectoriesExist();
            } catch (Exception ex) {
                MessageBox.Show("Can not access application data." + Environment.NewLine +
                    ex.GetType().ToString() + ": " + ex.Message);
            }

            Settings = new Settings();

            if (File.Exists(settingsFilePath)) {
                try {
                    var fileContents = File.ReadAllText(settingsFilePath);
                    Settings = JsonConvert.DeserializeObject<Settings>(fileContents);
                } catch (Exception ex) {
                    MessageBox.Show("Failed to load settings." + Environment.NewLine +
                        ex.GetType().ToString() + ": " + ex.Message);
                }
            }
        }

        static void SaveSettings() {
            var settingsJson = JsonConvert.SerializeObject(Settings);
            try {
                EnsureDirectoriesExist();
                File.WriteAllText(settingsFilePath, settingsJson);
            } catch (Exception ex) {
                MessageBox.Show("Failed to save settings." + Environment.NewLine +
                    ex.GetType().ToString() + ": " + ex.Message);
            }
        }

        private static void EnsureDirectoriesExist() {
            if (!Directory.Exists(appDataPath)) {
                Directory.CreateDirectory(appDataPath);
            }
        }
    }
}
