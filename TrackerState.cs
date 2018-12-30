using System;
using System.Collections.Generic;
using System.Text;

namespace AutoTracker
{
    class TrackerState
    {
        List<IStateListener> listeners = new List<IStateListener>();

        Dictionary<string, int> indicatorLevels = new Dictionary<string, int>();
        Dictionary<string, MapGridState> grids = new Dictionary<string, MapGridState>();
        Dictionary<string, MapMarkerList> markerSets = new Dictionary<string, MapMarkerList>();

        public TrackerState() {
        }

        public TrackerState(TrackerLayoutFile trackerInfo) {
            List<string> mapStateNames = new List<string>();

            // Initialize any placed maps and marker sets
            foreach (var layout in trackerInfo.layouts.Values) {
                foreach (var mapPlacement in layout.maps) {
                    var mapDefinition = trackerInfo.maps[mapPlacement.name];
                    var gridStateName = mapPlacement.stateName;
                    if (gridStateName == null) {
                        gridStateName = mapDefinition.stateName; 
                    }

                    InitializeGrid(gridStateName, mapDefinition.gridWidth, mapDefinition.gridHeight);

                    if (mapPlacement.markerSets != null) {
                        foreach (var markerPlacement in mapPlacement.markerSets) {
                            InitializeMarkers(markerPlacement.name, null);
                        }
                    }
                }
            }

            // Initialize all defined maps and marker sets
            // Even though they may not be part of a layout, they might still be referenced in code
            foreach (var map in trackerInfo.maps.Values) {
                InitializeGrid(map.stateName, 0, 0);
            }
        }

        private void InitializeMarkers(string stateName, MapMarker[] markers) {
            MapMarkerList markerState;
            if (!markerSets.TryGetValue(stateName, out markerState)) {
                markerState = new MapMarkerList();
                markerSets.Add(stateName, markerState);
            }

            markerState.AddRange(markers);
        }

        /// <summary>
        /// Ensures that a state object with the specified name exists and can accomodate the given dimensions.
        /// </summary>
        private void InitializeGrid(string stateName, int width, int height) {
            MapGridState state;
            if (!grids.TryGetValue(stateName, out state)) {
                state = new MapGridState();
                grids.Add(stateName, state);
            }

            state.allocate(width, height);
        }

        public void AddListener(IStateListener listener) {
            this.listeners.Add(listener);
        }
        public void RemoveListener(IStateListener listener) {
            this.listeners.Remove(listener);
        }

        public int GetIndicatorLevel(string name) {
            int result;
            if (!indicatorLevels.TryGetValue(name, out result)) result = 0;
            return result;
        }

        public void SetIndicatorLevel(string name, int value) {
            if (value < 0) throw new ArgumentException("negative value not valid");

            indicatorLevels[name] = value;
            foreach (var l in listeners) l.NotifyIndicatorChanged(name);
        }

        public int GetMapLevel(string stateName, int x, int y) {
            MapGridState gridState;
            if (!grids.TryGetValue(stateName, out gridState)) return 0;
            if (x < 0 || y < 0 || x >= gridState.Width || y >= gridState.Height) return 0;
            return gridState[x, y];
        }
        public void SetMapLevel(string stateName, int x, int y, int level) {
            MapGridState gridState;
            if (grids.TryGetValue(stateName, out gridState)) {
                if (x < 0 || y < 0 || x >= gridState.Width || y >= gridState.Height) return; // ignored
                gridState[x, y] = level;

                foreach (var l in listeners) l.NotifyMapChanged(stateName, x, y);
            }
        }
    }

    class MapGridState
    {
        int bufferWidth;
        int bufferHeight;
        int [,] buffer = new int[0,0];

        public int Width { get { return bufferWidth; } }
        public int Height { get { return bufferHeight; } }

        public int this[int x, int y] {
            get { return buffer[x, y]; }
            set { buffer[x, y] = value; }
        }

        /// <summary>
        /// Allocates at least enough buffer space to allocate the specified dimensions.
        /// Mutiple calls may be made to ensure the buffer is large enough to accomodate
        /// multiple maps that are intended to share data but are not necessarily the same size.
        /// </summary>
        public void allocate(int width, int height) {
            if (width > bufferWidth || height > bufferHeight) {
                int oldWidth = bufferWidth;
                int oldHeight = bufferHeight;

                int newWidth = Math.Max(width, oldWidth);
                int newHeight = Math.Max(height, oldHeight);
                int[,] newBuffer = new int[newWidth, newHeight];

                for (int x = 0; x < oldWidth; x++) {
                    for (int y = 0; y < oldHeight; y++) {
                        newBuffer[x, y] = buffer[x, y];
                    }
                }

                buffer = newBuffer;
                bufferWidth = newWidth;
                bufferHeight = newHeight;
            }
        }
    }

    class MapMarkerList:List<MapMarker>
    {

    }

    struct MapMarker
    {
        public MapMarker(int x, int y, int value) 
        :this(){
            this.X = x;
            this.Y = y;
            this.Value = value;
        }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Value { get; private set; }
    }

    interface IStateListener
    {
        void NotifyIndicatorChanged(string name);
        void NotifyMapChanged(string name, int x, int y);
        void NotifyMarkerChanged(string name, int x, int y);
    }
}
