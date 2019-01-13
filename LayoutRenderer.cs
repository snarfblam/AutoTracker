using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace AutoTracker
{
    /// <summary>
    /// Renders a bitmap based on tracker state, allowing for incremental changes.
    /// </summary>
    class LayoutRenderer: IStateListener, IDisposable
    {
        private readonly TrackerLayoutFile trackerDefinition;
        private readonly TrackerLayout layout;
        private readonly string layoutName;

        Bitmap[] backgrounds;
        readonly int width;
        readonly int height;
        Renderer renderer;
        HashCollection<string> invalidIndicators = new HashCollection<string>();

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public Bitmap Image { get { return renderer.Image; } }

        List<MapRenderer> maps = new List<MapRenderer>();


        public LayoutRenderer(TrackerLayoutFile trackerDefinition, string layoutName) {

            this.trackerDefinition = trackerDefinition;
            this.layoutName = layoutName;
            this.layout = trackerDefinition.layouts[layoutName];

            backgrounds = new Bitmap[layout.backgrounds.Length];
            for(var i = 0; i < layout.backgrounds.Length; i++){
                var bgPath = layout.backgrounds[i];
                backgrounds[i] = trackerDefinition.Meta.GetImage(bgPath);
            }

            if (backgrounds.Length == 0) throw new TrackerFileException("Layout " + layoutName + " has no background.");
            width = backgrounds[0].Width;
            height = backgrounds[0].Height;

            foreach (var placement in layout.maps) {
                maps.Add(new MapRenderer(this, placement));
            }

            renderer = new Renderer(width, height);
            RenderInitial();

            trackerDefinition.Meta.State.AddListener(this);
        }

        private void RenderInitial() {
            renderer.Draw(backgrounds[0], 0, 0, width, height, 0, 0);
            foreach (var map in maps) {
                map.RenderInitial();
                map.InvalidateAll();
                this.InvalidateAll();
            }
            Update();
        }

        private void InvalidateAll() {
            foreach (var indicator in layout.indicators) {
                invalidIndicators.Add(indicator.Key);
            }
        }

        public void Update() {
            foreach (var indicatorName in invalidIndicators) {
                TrackerIndicator indicator = null;
                if (layout.indicators.TryGetValue(indicatorName, out indicator)) {
                    var bounds = indicator.ToRect();
                    var level = trackerDefinition.Meta.State.GetIndicatorLevel(indicatorName);
                    level = Math.Min(level, backgrounds.Length - 1);

                    renderer.Draw(backgrounds[level], bounds, bounds.Location);
                }
            }

            invalidIndicators.Clear();

            foreach (var map in maps) {
                map.Update();
            }
        }


        void IStateListener.NotifyIndicatorChanged(string name) {
            invalidIndicators.Add(name);
        }

        void IStateListener.NotifyMapChanged(string name, int x, int y) {
            foreach (var map in maps) {
                if (map.StateName == name) map.InvalidateCell(new Point(x, y));
            }
        }

        void IStateListener.NotifyMarkerChanged(string name, int x, int y) {
            foreach (var map in maps) {
                map.InvalidateMarker(name, new Point(x, y));
            }
        }

        public void Dispose() {
            trackerDefinition.Meta.State.RemoveListener(this);
            renderer.Dispose();
            renderer = null;
        }

        class MapRenderer
        {
            LayoutRenderer owner;
            TrackerMapPlacement placement;
            TrackerMap map;
            public string StateName { get; private set; }
            Renderer Renderer { get { return owner.renderer; } }

            int x = 0;
            int y = 0;
            int cellWidth = 0;
            int cellHeight = 0;
            int gridWidth = 0;
            int gridHeight = 0;

            Bitmap[] backgrounds;

            HashCollection<Point> invalidCells = new HashCollection<Point>();
            List<TrackerMarkerSetReference> markerSets = new List<TrackerMarkerSetReference>();

            public MapRenderer(LayoutRenderer owner, TrackerMapPlacement mapPlacement) {
                this.owner = owner;
                this.placement = mapPlacement;
                this.map = owner.trackerDefinition.maps[mapPlacement.name];

                markerSets.AddRange(map.markerSets);
                markerSets.AddRange(mapPlacement.markerSets);

                x = placement.x ?? map.x;
                y = placement.y ?? map.y;
                cellWidth = placement.cellWidth ?? map.cellWidth;
                cellHeight = placement.cellHeight ?? map.cellHeight;
                gridWidth = map.gridWidth;
                gridHeight = map.gridHeight;
                StateName = placement.stateName ?? map.stateName;

                var placementBackgrounds = placement.backgrounds ?? new string[0];
                backgrounds = new Bitmap[Math.Max(placementBackgrounds.Length, map.backgrounds.Length)];
                for (int i = 0; i < backgrounds.Length; i++) {
                    // Prefer backgrounds from placement, but fall back to map definition if the placement doesn't provide enough to supercede all of them
                    if (i < placementBackgrounds.Length) {
                        backgrounds[i] = owner.trackerDefinition.Meta.GetImage(placement.backgrounds[i]);
                    } else {
                        backgrounds[i] = owner.trackerDefinition.Meta.GetImage(map.backgrounds[i]);
                    }
                }

            }

            public void RenderInitial() {
                Rectangle src = new Rectangle(0, 0, cellWidth * gridWidth, cellHeight * gridHeight);
                Renderer.Draw(backgrounds[0], src, x, y);
            }

            public void InvalidateCell(Point coords) {
                if (!invalidCells.Contains(coords)) {
                    invalidCells.Add(coords);
                }
            }

            public void InvalidateMarker(string setName, Point coords) {
                foreach (var markerSet in this.markerSets) {
                    if (markerSet.name == setName) {
                        InvalidateCell(coords);
                        return;
                    }
                }
            }

            public void Update() {
                foreach (var cell in invalidCells) {
                    Rectangle rect = new Rectangle(cell.X * cellWidth, cell.Y * cellHeight, cellWidth, cellHeight);
                    int state = this.owner.trackerDefinition.Meta.State.GetMapLevel(StateName, cell.X, cell.Y);
                    state = Math.Min(backgrounds.Length - 1, state);

                    Renderer.Draw(backgrounds[state], rect, rect.X + x, rect.Y + y);

                    // Todo: draw any markers
                    for (var i = 0; i < this.markerSets.Count; i++) {
                        var mSet = this.markerSets[i];
                        var markerState = this.owner.trackerDefinition.Meta.State.GetMarkers(this.markerSets[i].name, cell.X, cell.Y);
                        var source = this.owner.trackerDefinition.Meta.GetImage(mSet.source);
                        var srcRect = new Rectangle(0, 0, cellWidth, cellHeight);
                        for (var j = 0; j < markerState.Count; j++) {
                            srcRect.X = markerState[j] * cellWidth;
                            Renderer.Draw(source, srcRect, rect.X + x, rect.Y + y);
                        }
                    }
                }

                invalidCells.Clear();
            }

            public void InvalidateAll() {
                for (var x = 0; x < map.gridWidth; x++) {
                    for (var y = 0; y < map.gridHeight; y++) {
                        InvalidateCell(new Point(x, y));
                    }
                }
            }
        }


        public void UpdateAll() {
            this.RenderInitial();
        }
    }



    class Renderer : IDisposable
    {
        public Bitmap Image { get; private set; }
        Graphics gfx;
        public Renderer(int width, int height) {
            this.Image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfx = Graphics.FromImage(Image);
        }

        public void Draw(Image img, int sx, int sy, int w, int h, int dx, int dy) {
            Draw(img, new Rectangle(sx, sy, w, h), dx, dy);
        }

        public void Draw(Image img, Rectangle src, int x, int y) {
            gfx.DrawImage(img, new Rectangle(x, y, src.Width, src.Height), src, GraphicsUnit.Pixel);
        }

        public void Draw(Image img, Rectangle src, Point dest) {
            Draw(img, src, dest.X, dest.Y);
        }

        public void Dispose() {
            gfx.Dispose();
            Image.Dispose();

            gfx = null;
            Image = null;
        }
    }
}