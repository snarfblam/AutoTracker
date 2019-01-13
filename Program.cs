using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AutoTracker.Properties;

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

                //var layoutFile = Resources.z1m1Layout;
                var layoutFile = Resources.z1m1PackedLayout;
                var layout = LayoutFileParser.Parse(layoutFile);
                //layout.Meta.RootPath = "D:\\gits\\AutoTracker\\z1m1Layout";

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mainFrm = new Form1();
                mainFrm.setLayout(layout);

                Application.Run(mainFrm);
            }
        }
    }
}
