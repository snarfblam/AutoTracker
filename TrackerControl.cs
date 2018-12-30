using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AutoTracker
{
    class TrackerControl: Control
    {
        TrackerLayoutFile _tracker;
        string _layoutName;
        string _unloadingLayoutName;

        TrackerLayout _layout;
        LayoutRenderer _renderer;

        int updateLevel = 0;

        [System.ComponentModel.DefaultValue(true)]
        public bool CacheViews { get; set; }
        Dictionary<string, LayoutRenderer> cachedViews = new Dictionary<string, LayoutRenderer>();


        List<Tuple<TrackerMapMetrics, Rectangle>> mapBounds = new List<Tuple<TrackerMapMetrics, Rectangle>>();

        public TrackerControl() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            CacheViews = true;
        }

        public TrackerLayoutFile Tracker {
            get { return _tracker; }
            set {
                if (value == _tracker) return;
                _tracker = value;
                InitializeTracker();
            }
        }

        public string LayoutName {
            get { return _layoutName; }
            set {
                if (value == _layoutName) return;
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

            if (_tracker == null || _layoutName == null) {
                willBeEmpty = true;
            } else {
                TrackerLayout l;
                if (!_tracker.layouts.TryGetValue(_layoutName, out l)) willBeEmpty = true;
            }

            if (!isEmpty && willBeEmpty) {
                UninitializeTracker();
            } else if(!willBeEmpty) {
                // ACTUALLY INITIALIZE
                FreeLayoutResources();

                _layout = _tracker.layouts[_layoutName];
                if (cachedViews.ContainsKey(_layoutName)) {
                    _renderer = cachedViews[_layoutName];
                    _renderer.Update(); // Todo: determine if cached views should update in realtime, or if not, they should at least to allow redundant invalidations
                } else {
                    _renderer = new LayoutRenderer(_tracker, _layoutName);
                    if (CacheViews && !cachedViews.ContainsKey(_layoutName)) {
                        cachedViews.Add(_layoutName, _renderer);
                    }
                }
                foreach (var placement in _layout.maps) {
                    var mmap = _tracker.GetEffectiveMetrics(placement);
                    Rectangle bounds = new Rectangle(
                        mmap.x,
                        mmap.y,
                        mmap.cellWidth * mmap.gridWidth,
                        mmap.cellHeight * mmap.gridHeight
                    );
                    mapBounds.Add(Tuple.Create(mmap, bounds));
                }
                Invalidate(new Rectangle(0,0, _renderer.Width, _renderer.Height));
            }
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

            if (_renderer != null) {
                e.Graphics.DrawImage(_renderer.Image, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == System.Windows.Forms.MouseButtons.Left || e.Button == System.Windows.Forms.MouseButtons.Right) {
                foreach (var indicator in _layout.indicators) {
                    if (indicator.Value.ToRect().Contains(e.Location)) {
                        var evt = IndicatorClicked;
                        if (evt != null) {
                            evt(this, new IndicatorEventArgs(indicator.Key, e.Button));
                            return;
                        }
                    }
                }
                foreach (var map in mapBounds) {
                    var bounds = map.Item1;
                    if (bounds.Contains(e.Location)) {
                        int x = (e.X - bounds.X) / map.Item0.cellWidth;
                        int y = (e.Y - bounds.Y) / map.Item0.cellHeight;

                        var evt = MapCellClicked;
                        if (evt != null) {
                            evt(this, new GridEventArgs(map.Item0.name, e.Button, new Point(x, y)));
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
            return new Size(_renderer.Width, _renderer.Height); 
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
            return new TrackerUpdate(this, this._tracker.Meta.State);
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
        public GridEventArgs(string name, MouseButtons button, Point coords) {
            this.Name = name;
            this.Button = button;
            this.Coords = coords;
        }

        public string Name { get; private set; }
        public MouseButtons Button { get; private set; }
        public Point Coords { get; private set; }
    }
}
