using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AutoTracker
{
    // Todo: scale calculations via helper methods

    class MarkerSelectorControl: Control
    {
        TrackerLayoutFile _layoutFile;
        string _layoutName;
        TrackerMarkerSetReference _markerSetPlacement;
        TrackerMapPlacement _mapPlacement;
        bool _wasLoaded = false;
        Bitmap _tileSource = null;
        int _tileHeight = 0;
        int _tileWidth = 0;
        int _tileCount = 0;

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

        public TrackerMapPlacement MapPlacement {
            get { return _mapPlacement; }
            set {
                if (_mapPlacement != value) {
                    _mapPlacement = value;
                    TryLoadContents();
                }
            }
        }

        



        private void TryLoadContents() {
            bool willBeLoaded = _layoutFile != null && _layoutName != null && _markerSetPlacement != null && _mapPlacement != null;

            if (_wasLoaded && !willBeLoaded) {
                this._tileSource = null;
                this.Invalidate();
            }

            if (willBeLoaded) {
                var metrics = _layoutFile.GetEffectiveMetrics(_mapPlacement);

                this._tileSource = _layoutFile.Meta.GetImage(_markerSetPlacement.source);
                this._tileWidth = metrics.cellWidth; //MapPlacement.cellWidth;
                this._tileHeight = metrics.cellHeight; //MapPlacement.cellHeight;
                this._tileCount = _tileSource.Width / _tileWidth;
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

                    int rowNum = 0;
                    int tilesPerRow = getTilesPerRow();
                    int tilesRemaining = _tileCount;
                    
                    while (tilesRemaining > 0) {
                        int tilesInThisRow = Math.Min(tilesRemaining, tilesPerRow);
                        Rectangle source = new Rectangle(tilesPerRow * rowNum * _tileWidth, 0, tilesInThisRow * _tileWidth, _tileHeight);
                        Rectangle dest = source;
                        dest.X = 0 + tweak;
                        dest.Y = _tileHeight * rowNum;

                        dest.Width *= Scale;
                        dest.Height *= Scale;

                        e.Graphics.DrawImage(_tileSource, dest, source, GraphicsUnit.Pixel);

                        rowNum++;
                        tilesRemaining -= tilesInThisRow;
                    }

                    //e.Graphics.DrawImage(_tileSource, destRect, rect, GraphicsUnit.Pixel);
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

            int rowWidth = getTilesPerRow() * _tileWidth;

            var result =  new Rectangle(_selectedIndex * _tileSource.Height, 0, _tileSource.Height, _tileSource.Height);

            while (result.Left >= rowWidth) {
                result.Y += _tileHeight;
                result.X -= rowWidth;
            }

            result.X *= _scale;
            result.Y *= _scale;
            result.Width *= _scale;
            result.Height *= _scale;

            return result;
        }

        int getTilesPerRow() {
            return this.Width / _tileWidth;
        }

        int PointToTileNum(int pixelX, int pixelY) {
            if ((_tileHeight | _tileWidth) == 0) return -1;
            if (pixelY < 0 | pixelX < 0) return -1;

            int x = pixelX / _scale;
            int y = pixelY / _scale;

            int rowWidth = getTilesPerRow() * _tileWidth;
            if (x >= rowWidth) return -1;

            while (_tileHeight > 0 && y > _tileHeight) {
                x += rowWidth;
                y -= _tileHeight;
            }

            int maxSel = _tileSource.Width / _tileWidth; 
            int value = x / _tileWidth;

            return Math.Min(maxSel, value);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            var mx = e.X / _scale;
            var my = e.Y / _scale;
            if (_tileSource != null) {
                int maxSel = _tileSource.Width / _tileSource.Height; // exclusive

                var selRect = getSelectionRectangle();
                //int value = e.X / selRect.Height;
                int value = PointToTileNum(e.X, e.Y);

                if (value >= 0 && value < maxSel) {
                    SelectedIndex = value;
                    var evt = IndexSelected;
                    if (evt != null) evt(this, EventArgs.Empty);
                }
            }
        }
    }
}
