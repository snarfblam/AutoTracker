using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoTracker
{
    class Packer
    {
        Dictionary<string, string> files = new Dictionary<string, string>();
        static string[][] filenamePaths = {
            "layouts/*/backgrounds/!filelist".Split('/'),
            "layouts/*/maps/*/backgrounds/!filelist".Split('/'),
            "layouts/*/maps/*/markerSets/*/source/!file".Split('/'),
            "maps/*/backgrounds/!filelist".Split('/'),
            "maps/*/markerSets/*/source/!file".Split('/'),
        };

        private Packer() {
        }
        public static void PackFile(string path, string output) {
            var packer = new Packer();
            packer.DoPack(path, output);
        }
        private void DoPack(string path, string output) {
            var root = System.IO.Path.GetDirectoryName(path);

            JToken obj;
            using (var reader = System.IO.File.OpenText(path)) {
                using (var jReader = new Newtonsoft.Json.JsonTextReader(reader)) {
                    obj = JToken.ReadFrom(jReader);
                }
            }
            
            foreach (var objPath in filenamePaths) {
                ProcessPath(obj, objPath);
            }

            foreach (var referencedFile in files) {
                var externalFile = System.IO.Path.Combine(root, referencedFile.Key);
                var embeddedFile = referencedFile.Value;

                var b64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(externalFile));
                obj["files"][embeddedFile] = b64;
            }

            using (var writer = System.IO.File.CreateText(output)) {
                using (var jWrite = new Newtonsoft.Json.JsonTextWriter(writer)) {
                    obj.WriteTo(jWrite);
                }
            }
        }

        /// <summary>
        /// Populates this.files with a list of referenced files (keys) and the corresponding embedded filenames (values),
        /// and replaces these references to external files with the embedded paths.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        private void ProcessPath(JToken obj, string[] path) {
            ProcessPath(obj, path, 0);
        }
        private void ProcessPath(JToken obj, string[] path, int pathIndex) {
            JToken parent = null;
            string currentName = null;
            JToken current = obj;

            while (pathIndex < path.Length) {
                var part = path[pathIndex];

                if (part == "*") {
                    if (current is JObject) {
                        foreach (var child in current.Children()) {
                            ProcessPath(((JProperty)child).Value, path, pathIndex + 1);
                        }
                    } else if (current is JArray) {
                        foreach (var child in current.Children()) {
                            ProcessPath(child, path, pathIndex + 1);
                        }
                    }
                    return;
                } else if (part == "!file") {
                    if (current.Type == JTokenType.String && parent != null) {
                        var oldFile = (string)current;
                        var newFile = GetPackedPath(oldFile);
                        parent[currentName] = "!/" + newFile;
                    }
                    pathIndex++;
                } else if (part == "!filelist") {
                    var jObj = current as JObject;
                    if (jObj != null) {
                        List<string> keys = new List<string>();
                        foreach (var p in jObj.Properties()) keys.Add(p.Name);

                        foreach (var key in keys) {
                            var oldFile = (string)jObj[key];
                            var newFile = GetPackedPath(oldFile);
                            jObj[key] = "!/" + newFile;
                        }
                    }
                    var jArr = current as JArray;
                    if (jArr != null) {
                        for (var i = 0; i < jArr.Count; i++) {
                            var oldFile = (string)jArr[i];
                            var newFile = GetPackedPath(oldFile);
                            jArr[i] = "!/" + newFile;
                        }
                    }
                    pathIndex++;
                } else {
                    parent = current;
                    currentName = part;
                    current = current[part];
                    pathIndex++;
                }
            }
        }

        static IList<object> from(System.Collections.IEnumerable e) {
            IList<object> result = new List<object>();
            foreach (var i in e) result.Add(i);
            return result;
        }

        private void ProcessBackgroundArray(JToken bgs) {
            if (bgs == null || !(bgs is JArray)) return;

            for (var i = 0; i < ((JArray)bgs).Count; i++) {
                bgs[i] = "!/" + GetPackedPath((string)bgs[i]);
            }
        }

        private string GetPackedPath(string path) {
            // File may already contained packed files
            if (path.StartsWith("!")) return path;

            // If file is already queued for packing, just give its packed name
            string packedFileName;
            if (files.TryGetValue(path, out packedFileName)) return packedFileName;

            // Queue file and generate packed name
            packedFileName = "packedfile" + files.Count.ToString();
            files.Add(path, packedFileName);
            return packedFileName;
        }
    }
}
