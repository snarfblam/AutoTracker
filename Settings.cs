using System;
using System.Collections.Generic;
using System.Text;

namespace AutoTracker
{
    /// <summary>
    /// Serializable class representing application settings.
    /// </summary>
    class Settings
    {
        public Settings() {
            hideBurger = false;
            autoTrack = false;
            layout = Layouts.Z1M1;
        }

        public bool hideBurger { get; set; }
        public bool autoTrack { get; set; }
        public string layout { get; set; }

        public static class Layouts
        {
            static string[] allLayouts = { Z1M1, Custom };

            public const string Z1M1 = "Z1M1";
            public const string Custom = "Custom";
            public static bool IsValid(string layout) {
                return Array.IndexOf(allLayouts, layout) >= 0;
            }
        }
    }
}
