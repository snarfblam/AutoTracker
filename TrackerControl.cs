using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AutoTracker
{
    class TrackerControl: Control
    {
        TrackerLayoutFile _layoutFile;
        string _layoutName;
        string _unloadingLayoutName;

        TrackerLayout _layout;
        LayoutRenderer _renderer;
        LayoutMargin LayoutMargin { get { return _layout.margin ?? new LayoutMargin(); } }

        Dictionary<string, int> LayoutMarkerSelections = new Dictionary<string, int>();
        /// <summary>To be called while loading a new layout to retrieve the previous marker selection for that layout</summary>
        int GetLayoutMarkerSelection(string layout) {
            int result;
            if (LayoutMarkerSelections.TryGetValue(layout, out result)) return result;
            return 0;
        }
        /// <summary>To be called before loading a new layout to cache the marker selection</summary>
        void SetLayoutMarkerSelection(string layout, int value) {
            if (string.IsNullOrEmpty(layout)) return;
            LayoutMarkerSelections[layout] = value;
        }

        MarkerSelectorControl pickerControl;

        int updateLevel = 0;

        [System.ComponentModel.DefaultValue(true)]
        public bool CacheViews { get; set; }
        Dictionary<string, LayoutRenderer> cachedViews = new Dictionary<string, LayoutRenderer>();


        List<Tuple<TrackerMapMetrics, Rectangle>> mapBounds = new List<Tuple<TrackerMapMetrics, Rectangle>>();

        public TrackerControl() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            CacheViews = true;

            pickerControl = new MarkerSelectorControl();
            pickerControl.Visible = false;
            this.Controls.Add(pickerControl);
        }

        public TrackerLayoutFile Tracker {
            get { return _layoutFile; }
            set {
                if (value == _layoutFile) return;
                _layoutFile = value;
                foreach (var view in cachedViews.Values) view.Dispose();
                cachedViews.Clear();
                InitializeTracker();
            }
        }

        public string LayoutName {
            get { return _layoutName; }
            set {
                if (value == _layoutName) return;

                SetLayoutMarkerSelection(_layoutName, pickerControl.SelectedIndex);
                _unloadingLayoutName = _layoutName;
                _layoutName = value;
                InitializeTracker();
                _unloadingLayoutName = null;
            }
        }

        [System.ComponentModel.Browsable(false)]
        public TrackerLayout Layout { get { return _layout; } }

        private void InitializeTracker() {
            bool isEmpty = _layout == null;
            bool willBeEmpty = false;

            if (_layoutFile == null || _layoutName == null) {
                willBeEmpty = true;
            } else {
                TrackerLayout l;
                if (!_layoutFile.layouts.TryGetValue(_layoutName, out l)) willBeEmpty = true;
            }

            if (!isEmpty && willBeEmpty) {
                UninitializeTracker();
            } else if(!willBeEmpty) {
                // ACTUALLY INITIALIZE
                FreeLayoutResources();

                _layout = _layoutFile.layouts[_layoutName];
                if (cachedViews.ContainsKey(_layoutName)) {
                    _renderer = cachedViews[_layoutName];
                    _renderer.Update(); // Todo: determine if cached views should update in realtime, or if not, they should at least to allow redundant invalidations
                } else {
                    _renderer = new LayoutRenderer(_layoutFile, _layoutName);
                    if (CacheViews && !cachedViews.ContainsKey(_layoutName)) {
                        cachedViews.Add(_layoutName, _renderer);
                    }
                }
                foreach (var placement in _layout.maps) {
                    var mmap = _layoutFile.GetEffectiveMetrics(placement);
                    Rectangle bounds = new Rectangle(
                        mmap.x,
                        mmap.y,
                        mmap.cellWidth * mmap.gridWidth,
                        mmap.cellHeight * mmap.gridHeight
                    );
                    mapBounds.Add(Tuple.Create(mmap, bounds));
                }

                SetupPicker();
                Invalidate(new Rectangle(0,0, _renderer.Width, _renderer.Height));
            }
        }

        public int SelectedMarker { get { return pickerControl.SelectedIndex; } set { pickerControl.SelectedIndex = value; } }

        private void SetupPicker() {
            TrackerMarkerSetReference markerSet;
            TrackerMapPlacement map;

            var picker = FindPicker(out map, out markerSet);

            if (picker == null) {
                pickerControl.Visible = false;
            } else {
                var margin = this.LayoutMargin;

                pickerControl.Bounds = new Rectangle(
                    picker.x + margin.left,
                    picker.y + margin.top,
                    picker.width.Value, picker.height.Value);

                pickerControl.ClearLayout();
                pickerControl.LayoutFile = _layoutFile;
                pickerControl.LayoutName = _layoutName;
                pickerControl.MarkerSetPlacement = markerSet;
                pickerControl.MapPlacement = map;
                pickerControl.Scale = picker.scale ?? 1;

                pickerControl.SelectedIndex = GetLayoutMarkerSelection(_layoutName);
                pickerControl.Visible = true;
            }
        }
        private TrackerPickerPlacement FindPicker(out TrackerMapPlacement mapPlacement, out TrackerMarkerSetReference markerSet) {
            markerSet = null;
            mapPlacement = null;

            foreach (var map in _layout.maps) {
                foreach (var marker in map.markerSets) {
                    markerSet = marker;
                    mapPlacement = map;
                    if (marker.picker != null) return marker.picker;
                }
            }

            return null;
        }

        private void FreeLayoutResources() {
            // Todo: clear any layout-specific values and free any resources
            if(_renderer != null && (_unloadingLayoutName != null && !cachedViews.ContainsKey(_unloadingLayoutName))) {
                _renderer.Dispose();
            }
            _renderer = null;
            _layout = null;
            mapBounds.Clear();
        }
        private void UninitializeTracker() {
            FreeLayoutResources();
            // Todo: Prepare to draw empty control
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            LayoutMargin margin = new LayoutMargin();
            if (_layout != null) margin = _layout.margin ?? new LayoutMargin();

            Rectangle dest = e.ClipRectangle;
            Rectangle source = dest;
            source.X -= margin.left;
            source.Y -= margin.top;

            Rectangle renderedBounds = new Rectangle();
            if (_renderer != null) {
                renderedBounds = new Rectangle(margin.left, margin.top, _renderer.Width, _renderer.Height);
            }

            // We only need to draw the backcolor of the invalid rect includes margins
            if (!renderedBounds.Contains(e.ClipRectangle)) {
                using (var b = new SolidBrush(this.BackColor)) {
                    e.Graphics.FillRectangle(b, e.ClipRectangle);
                }
            }

            // Draw rendered tracker image
            if (_renderer != null) {
                e.Graphics.DrawImage(_renderer.Image, dest, source, GraphicsUnit.Pixel);
            }
        }

        private static readonly CellQuadrant[] quads = { CellQuadrant.TopLeft, CellQuadrant.TopRight, CellQuadrant.BottomLeft, CellQuadrant.BottomRight };
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            Point p = new Point(e.X - _layout.margin.Value.left, e.Y - _layout.margin.Value.top);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle) {
                foreach (var indicator in _layout.indicators) {
                    if (indicator.Value.ToRect().Contains(p)) {
                        var evt = IndicatorClicked;
                        if (evt != null) {
                            evt(this, new IndicatorEventArgs(indicator.Key, e.Button));
                            return;
                        }
                    }
                }
                foreach (var map in mapBounds) {
                    var bounds = map.Item1;
                    if (bounds.Contains(p)) {
                        var cellWidth = map.Item0.cellWidth;
                        var cellHeight = map.Item0.cellHeight;
                        var relX = (p.X - bounds.X);
                        var relY = (p.Y - bounds.Y);

                        int x = relX / cellWidth;
                        int y = relY / cellHeight;

                        var qX = (relX % cellWidth) >= (cellWidth / 2) ? 1 : 0;
                        var qY = (relY % cellHeight) >= (cellHeight / 2) ? 2 : 0;
                        var quad = quads[qX + qY];
    
                        var evt = MapCellClicked;
                        if (evt != null) {
                            evt(this, new GridEventArgs(map.Item0.name, e.Button, new Point(x, y), quad));
                        }
                    }
                }
            }
        }

        ///// <summary>
        ///// Returns the size this control should assume to fit its contents, or (0, 0) if there are no contents to display.
        ///// </summary>
        //public override Size PreferredSize { get { return new Size(_renderer.Width, _renderer.Height); } }
        public override Size GetPreferredSize(Size proposedSize) {
            var width = _renderer.Width + LayoutMargin.left + LayoutMargin.right;
            var height = _renderer.Height + LayoutMargin.top + LayoutMargin.bottom;
            return new Size(width, height); 
        }

        public event EventHandler<IndicatorEventArgs> IndicatorClicked;
        public event EventHandler<GridEventArgs> MapCellClicked;

        /// <summary>
        /// Forces the tracker to immedately update its graphics
        /// </summary>
        public new void Update() {
            _renderer.Update();
            Invalidate(new Rectangle(0, 0, _renderer.Width, _renderer.Height));
        }

        /// <summary>
        /// Begins a batch update. Should be called before changing tracker state. Tracker will
        /// redraw when an EndUpdate call has been made for each BeginUpdate call (or each update
        /// object has been disposed).
        /// </summary>
        public TrackerUpdate BeginUpdate() {
            updateLevel++;
            return new TrackerUpdate(this, this._layoutFile.Meta.State);
        }

        public void EndUpdate(TrackerUpdate scope) {
            scope.EndUpdate();
        }

        private void EndUpdate() {
            updateLevel--;
            if (updateLevel == 0) Update();
        }

        public class TrackerUpdate : IDisposable
        {
            public TrackerState State { get; private set; }
            TrackerControl control;

            public TrackerUpdate(TrackerControl c, TrackerState s) {
                this.control = c;
                this.State = s;
            }

            void IDisposable.Dispose() {
                // Multiple calls to dispose must be tolerated
                if (control != null) {
                    control.EndUpdate();
                    control = null;
                    State = null;
                }
            }

            public void EndUpdate() {
                // When explicitly calling BeginUpdate/EndUpdate, only one call to EndUpdate is allowed
                if (control == null) throw new InvalidOperationException("The update has already been ended.");
                ((IDisposable)this).Dispose();
            }
        }

        internal void ResetTracker() {
            _layoutFile.Meta.State.Reset();
            _renderer.UpdateAll();
            Invalidate();
        }
    }



    class IndicatorEventArgs:EventArgs
    {
        public IndicatorEventArgs(string name, MouseButtons button) {
            this.Name = name;
            this.Button = button;
        }

        public string Name { get; private set; }
        public MouseButtons Button{get; private set;}
    }
    class GridEventArgs : EventArgs
    {
        public GridEventArgs(string name, MouseButtons button, Point coords, CellQuadrant quad) {
            this.Name = name;
            this.Button = button;
            this.Coords = coords;
            this.Quadrant = quad;
        }

        public string Name { get; private set; }
        public MouseButtons Button { get; private set; }
        public Point Coords { get; private set; }
        public CellQuadrant Quadrant { get; private set; }
    }
}
