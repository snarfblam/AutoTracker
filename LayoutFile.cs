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

        public static TrackerLayoutFile Load(string path) {
            return Load(path, false);
        }
        public static TrackerLayoutFile Load(string path, bool applyEffectiveValues) {
            var relativePath = Path.GetDirectoryName(path);

            var result = Parse(File.ReadAllText(path), applyEffectiveValues);
            result.Meta.RootPath = relativePath;

            return result;
        }

        public static TrackerLayoutFile Parse(string json) {
            return Parse(json, false);
        }
        /// <summary>Deserializes layout data from a string.</summary>
        /// <returns></returns>
        /// <param name="json"></param>
        /// <param name="applyEffectiveValues">If true, any omitted optional field 
        /// where a value can be inherited or calculated will be filled in if possible.
        /// This may be undesirable if the data is being loaded to be modified and
        /// re-serialized.</param>
        public static TrackerLayoutFile Parse(string json, bool applyEffectiveValues) {
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TrackerLayoutFile>(json);

            // Todo: validate map:
            // - There are almost certainly other things not listed here that should be validated
            // - Required strings are not null or empty
            //     - For each map definition, it must specify a state name, or every placement of it must specify a state name
            // - Require that each identifier references an actually declared object (e.g. each map placement's .name must match a defined map)
            // - Things should not be defined more than once (no more than one map with a given name)
            // - Values must be in-range (widths > 0, levels >= 0)
            // - references to embedded files can be statically validated



            if (applyEffectiveValues) {
                result.ApplyEffectiveValues();
            }
            return result;
        }
    }

    class TrackerPickerPlacement
    {
        public int x { get; set; }
        public int y { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public int? scale { get; set; }

        public void ApplyEffectiveValues(TrackerLayoutFile file, TrackerMarkerSetReference markerSet) {
            if (scale == null || scale < 1) scale = 1;

            if (width == null || height == null) {
                var image = file.Meta.GetImage(markerSet.source);
                width = width ?? (image.Width * scale);
                height = height ?? (image.Height * scale);
            }

        }
    }

    class TrackerLayoutFile
    {
        public TrackerLayoutFile() {
            this.Meta = new TrackerMeta(this);
            this.files = new Dictionary<string, string>();
            this.layouts = new Dictionary<string, TrackerLayout>();
            this.maps = new Dictionary<string, TrackerMap>();
            this.markerSets = new Dictionary<string, TrackerMapMarker[]>();
        }

        public string version { get; set; }
        public Dictionary<string, string> files { get; set; }
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

        static int autoID = 0;
        internal void ApplyEffectiveValues() {
            // Automatically assign a unique state name to maps that don't explicity define one
            foreach (var map in maps.Values) {
                if (string.IsNullOrEmpty(map.stateName)) {
                    string stateName = "_auto_" + autoID.ToString();
                    autoID++;

                    map.stateName = stateName;
                }
            }

            foreach (var layout in layouts.Values) {
                layout.ApplyEffectiveValues(this);
            }
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
        public LayoutMargin? margin { get; set; }
        public string backcolor { get; set; }

        string _backcolorCached = null;
        Color? _backcolorColor = null;
        public Color? GetBackcolor() {
            // Recalculate if the backcolor property has been changed
            if (backcolor != _backcolorCached) {
                _backcolorCached = backcolor;
                // If parsing the color as RRGGBB fails, we'll return null
                try {
                    _backcolorColor = ColorTranslator.FromHtml("#" + _backcolorCached);
                } catch (Exception) {
                    _backcolorColor = null;
                }
            }

            return _backcolorColor;
        }

        internal void ApplyEffectiveValues(TrackerLayoutFile file) {
            if (this.margin == null) {
                this.margin = new LayoutMargin();
            }
            foreach (var map in this.maps) {
                map.ApplyEffectiveValues(file);
            }
        }
    }

    struct LayoutMargin
    {
        public int top { get; set; }
        public int bottom { get; set; }
        public int left { get; set; }
        public int right { get; set; }
    }

    class TrackerIndicator
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public int? max { get; set; }

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
        public TrackerPickerPlacement picker { get; set; }


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

        internal void ApplyEffectiveValues(TrackerLayoutFile file) {
            var mapBase = file.maps[this.name];
            if (this.stateName == null) this.stateName = mapBase.stateName;
            if (this.x == null) this.x = mapBase.x;
            if (this.y == null) this.y = mapBase.y;
            if (this.cellWidth == null) this.cellWidth = mapBase.cellWidth;
            if (this.cellHeight == null) this.cellHeight = mapBase.cellHeight;
            if (this.backgrounds == null || this.backgrounds.Length == 0) this.backgrounds = mapBase.backgrounds;

            List<TrackerMarkerSetReference> markerSetList = new List<TrackerMarkerSetReference>();
            markerSetList.AddRange(mapBase.markerSets);
            markerSetList.AddRange(this.markerSets);
            this.markerSets = markerSetList.ToArray();

            foreach (var m in this.markerSets) {
                if (m.picker != null) {
                    m.picker.ApplyEffectiveValues(file, m);
                }
            }
        }

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