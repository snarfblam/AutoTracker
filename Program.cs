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
        static void Main() {
            var layoutFile = Resources.z1m1Layout;
            var layout = LayoutFileParser.Parse(layoutFile);
            layout.Meta.RootPath = "D:\\gits\\AutoTracker\\z1m1Layout";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainFrm = new Form1();
            mainFrm.setLayout(layout);
            
            Application.Run(mainFrm);

        }
    }
}
