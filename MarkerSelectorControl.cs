using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AutoTracker
{
    class MarkerSelectorControl: Control
    {
        TrackerLayoutFile _layoutFile;
        string _layoutName;
        TrackerMarkerSetReference _markerSetPlacement;
        string _markerSetName;
        bool _wasLoaded = false;
        Bitmap _tileSource = null;

        int _selectedIndex = 0;
        int _scale = 1;

        /// <summary>Raised when the value of SelectedIndex changes</summary>
        public event EventHandler SelectedIndexChanged;
        /// <summary>Raised when the value of SelectedIndex changes as a result of user input</summary>
        public event EventHandler IndexSelected;

        public MarkerSelectorControl() {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);
        }

        public int SelectedIndex {
            get { return _selectedIndex; }
            set {
                if (_selectedIndex != value) {
                    Invalidate(getSelectionRectangle());
                    _selectedIndex = value;
                    Invalidate(getSelectionRectangle());
                    var evt = SelectedIndexChanged;
                    if (evt != null) evt(this, EventArgs.Empty);
                }
            }
        }

        public int Scale {
            get { return _scale; }
            set {
                if (_scale != value) {
                    _scale = value;
                    if (_scale < 1) Scale = 1;
                    Invalidate();
                }
            }
        }

        public TrackerLayoutFile LayoutFile {
            get { return _layoutFile; }
            set {
                if (_layoutFile != value) {
                    _layoutFile = value;
                    TryLoadContents();
                }
            }
        }

        public string LayoutName {
            get { return _layoutName; }
            set {
                if (_layoutName != value) {
                    _layoutName = value;
                    TryLoadContents();
                }
            }
        }

        public TrackerMarkerSetReference MarkerSetPlacement {
            get { return _markerSetPlacement; }
            set {
                if (_markerSetPlacement != value) {
                    _markerSetPlacement = value;
                    TryLoadContents();
                }
            }
        }

        public string MarkerSetName {
            get { return _markerSetName; }
            set {
                if (_markerSetName != value) {
                    _markerSetName = value;
                    TryLoadContents();
                }
            }
        }



        private void TryLoadContents() {
            bool willBeLoaded = _layoutFile != null && _layoutName != null && _markerSetPlacement != null && _markerSetName != null;

            if (_wasLoaded && !willBeLoaded) {
                this._tileSource = null;
                this.Invalidate();
            }

            if (willBeLoaded) {
                this._tileSource = _layoutFile.Meta.GetImage(_markerSetPlacement.source);
                this.Invalidate();
            }

            _wasLoaded = willBeLoaded;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            var origPixelOffset = e.Graphics.PixelOffsetMode;

            if (_scale != 1) {
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            }

            e.Graphics.Clear(BackColor);
            if (_tileSource != null) {
                var rect = new Rectangle(0, 0, _tileSource.Width, _tileSource.Height);
                var destRect = new Rectangle(0, 0, rect.Width * Scale, rect.Height * Scale);
                var selRect = getSelectionRectangle();
                using (var b = new SolidBrush(Color.FromArgb(96, SystemColors.Highlight))) {
                    var tweak = ((_scale * 2) - 1) / 4;
                    e.Graphics.FillRectangle(b, selRect);
                    destRect.X += tweak;
                    e.Graphics.DrawImage(_tileSource, destRect, rect, GraphicsUnit.Pixel);
                    destRect.X -= tweak;
                    e.Graphics.FillRectangle(b, selRect);
                }
                selRect.Width--;
                selRect.Height--;
                e.Graphics.PixelOffsetMode = origPixelOffset;
                e.Graphics.DrawRectangle(SystemPens.Highlight, selRect);
            }
            
        }


        Rectangle getSelectionRectangle() {
            if (_tileSource == null) return new Rectangle(0, 0, 0, 0);
            return new Rectangle(_selectedIndex * _tileSource.Height * _scale, 0, _tileSource.Height * _scale, _tileSource.Height * _scale);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            var mx = e.X / _scale;
            var my = e.Y / _scale;
            if (_tileSource != null) {
                int maxSel = _tileSource.Width / _tileSource.Height; // exclusive

                var selRect = getSelectionRectangle();
                if (e.Y >= selRect.Top && e.Y < selRect.Bottom) {
                    int value = e.X / selRect.Height;

                    if (value >= 0 && value < maxSel) {
                        SelectedIndex = value;
                        var evt = IndexSelected;
                        if (evt != null) evt(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
