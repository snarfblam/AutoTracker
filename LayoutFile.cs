using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;

namespace AutoTracker
{
    static class LayoutFileParser
    {
        static int autoID = 0;

        public static TrackerLayoutFile Load(string path) {
            var relativePath = Path.GetDirectoryName(path);

            var result = Parse(File.ReadAllText(path));
            result.Meta.RootPath = relativePath;

            return result;
        }

        public static TrackerLayoutFile Parse(string json) {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TrackerLayoutFile>(json);

            // Todo: validate map:
            // - There are almost certainly other things not listed here that should be validated
            // - Required strings are not null or empty
            //     - For each map definition, it must specify a state name, or every placement of it must specify a state name
            // - Require that each identifier references an actually declared object (e.g. each map placement's .name must match a defined map)
            // - Things should not be defined more than once (no more than one map with a given name)
            // - Values must be in-range (widths > 0, levels >= 0)
            // - references to embedded files can be statically validated

            // Automatically assign a unique state name to maps that don't explicity define one
            foreach (var map in result.maps.Values) {
                if (string.IsNullOrEmpty(map.stateName)) {
                    string stateName = "_auto_" + autoID.ToString();
                    autoID++;

                    map.stateName = stateName;
                }
            }
            return result;
        }
    }

    class TrackerLayoutFile
    {
        public TrackerLayoutFile() {
            this.Meta = new TrackerMeta(this);
            this.Files = new Dictionary<string, string>();
            this.layouts = new Dictionary<string, TrackerLayout>();
            this.maps = new Dictionary<string, TrackerMap>();
            this.markerSets = new Dictionary<string, TrackerMapMarker[]>();
        }

        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, TrackerLayout> layouts { get; set; }
        public Dictionary<string, TrackerMap> maps { get; set; }
        public Dictionary<string, TrackerMapMarker[]> markerSets { get; set; }

        /// <summary>
        /// Holds runtime data that will not be serialized.
        /// </summary>
        [JsonIgnore]
        public TrackerMeta Meta { get; private set; }

        /// <summary>
        /// Returns a new copy of the specified map placement entry, with properties updated
        /// to reflect values that will be inherited from the underlying map.
        /// </summary>
        public TrackerMapMetrics GetEffectiveMetrics(TrackerMapPlacement actualPlacement) {
            TrackerMap map;
            if (!this.maps.TryGetValue(actualPlacement.name, out map)) {
                throw new ArgumentException("The specified placement refers to a map that is not defined.");
            }

            var result = new TrackerMapMetrics();
            result.name = actualPlacement.name;
            result.stateName = actualPlacement.stateName ?? map.stateName;
            result.x = actualPlacement.x ?? map.x;
            result.y = actualPlacement.y ?? map.y;
            result.cellWidth = actualPlacement.cellWidth ?? map.cellWidth;
            result.cellHeight = actualPlacement.cellHeight ?? map.cellHeight;
            result.gridWidth = map.gridWidth;
            result.gridHeight = map.gridHeight;

            List<TrackerMarkerSetReference> markers = new List<TrackerMarkerSetReference>();
            markers.AddRange(map.markerSets);
            markers.AddRange(actualPlacement.markerSets);
            result.markerSets = markers.ToArray();

            result.backgrounds = new string[Math.Max(actualPlacement.backgrounds.Length, map.backgrounds.Length)];
            for (var i = 0; i < result.backgrounds.Length; i++) {
                result.backgrounds[i] = (i < actualPlacement.backgrounds.Length) ? actualPlacement.backgrounds[i] : map.backgrounds[i];
            }

            return result;
        }
    }



    class TrackerLayout
    {
        public TrackerLayout() {
            this.backgrounds = Empty<string>.Array;
            this.maps = Empty<TrackerMapPlacement>.Array;
            this.indicators = new Dictionary<string, TrackerIndicator>();
        }

        public string[] backgrounds { get; set; }
        public Dictionary<string, TrackerIndicator> indicators { get; set; }
        public TrackerMapPlacement[] maps { get; set; }
    }

    class TrackerIndicator
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public int max { get; set; }

        public Rectangle ToRect() {
            return new Rectangle(x, y, w, h);
        }

    }

    class TrackerMap
    {
        public TrackerMap() {
            this.backgrounds = Empty<string>.Array;
            this.markerSets = Empty<TrackerMarkerSetReference>.Array;
        }

        public int x { get; set; }
        public int y { get; set; }
        public int cellWidth { get; set; }
        public int cellHeight { get; set; }
        public int gridWidth { get; set; }
        public int gridHeight { get; set; }
        public TrackerMarkerSetReference[] markerSets { get; set; }
        public string[] backgrounds { get; set; }
        public string stateName { get; set; }
    }

    class TrackerMarkerSetReference
    {
        public string name { get; set; }
        public string source { get; set; }
    }

    class TrackerMapPlacement
    {
        public TrackerMapPlacement() {
            this.backgrounds = Empty<string>.Array;
            this.markerSets = Empty<TrackerMarkerSetReference>.Array;
        }
        public string name { get; set; }
        public TrackerMarkerSetReference[] markerSets { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public int? cellWidth { get; set; }
        public int? cellHeight { get; set; }
        public string[] backgrounds { get; set; }
        public string stateName { get; set; }
    }

    class TrackerMapMarker
    {
        public int x { get; set; }
        public int y { get; set; }
        public int? level { get; set; }
    }

    /// <summary>
    /// Represents the effective metrics of a map placement (composition
    /// of a map placement entry and the properties it inherits from
    /// the underlying map)
    /// </summary>
    class TrackerMapMetrics
    {
        public TrackerMapMetrics() {
            this.backgrounds = Empty<string>.Array;
        }
        public string name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int cellWidth { get; set; }
        public int cellHeight { get; set; }
        public int gridWidth { get; set; }
        public int gridHeight { get; set; }
        public TrackerMarkerSetReference[] markerSets { get; set; }
        public string[] backgrounds { get; set; }
        public string stateName { get; set; }
    }

    static class Empty<T>
    {
        public static readonly T[] Array = new T[0];
    }

}