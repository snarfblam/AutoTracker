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
        }

        public bool hideBurger { get; set; }
        public bool autoTrack { get; set; }
    }
}
