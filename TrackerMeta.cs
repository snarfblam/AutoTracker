using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace AutoTracker
{
    class TrackerMeta
    {
        public TrackerLayoutFile File { get; private set; }

        public TrackerMeta(TrackerLayoutFile file) {
            this.File = file;
        }

        /// <summary>
        /// Gets/sets the path to look for files in.
        /// Should be the path the tracker layout file was loaded from.
        /// Automatically set if the file is loaded via filename.
        /// </summary>
        public string RootPath { get; set; }
        TrackerState _State = null;
        public TrackerState State {
            get {
                if (_State == null) _State = new TrackerState(File);
                return _State;
            }
        }

        Dictionary<string, Bitmap> images = new Dictionary<string, Bitmap>();

        /// <summary>
        /// Returns the specified image, loading it from disk if it has not
        /// already been loaded.
        /// </summary>
        public Bitmap GetImage(string path) {
            Bitmap result;

            if (!images.TryGetValue(path, out result)) {
                if(path.StartsWith("!/") || path.StartsWith("!\\")) {
                    var embeddedFileName = path.Substring(2);
                    var base64 = File.Files[embeddedFileName];
                    var bytes = Convert.FromBase64String(base64);
                    var stream = new MemoryStream(bytes);

                    result = (Bitmap)Image.FromStream(stream);
                }
                result = (Bitmap)Image.FromFile(Path.Combine(RootPath, path));
                images.Add(path, result);
            }

            return result;
        }

    }
}
