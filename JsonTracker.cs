using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoTracker
{
    /// <summary>
    /// Parses state from JSON data and applies it to a TrackerState object
    /// </summary>
    /// <remarks>JSON paths are specified using dot notation, in the form of 
    /// "keynode.subnode.subnode...". To specify a path relative to the root,
    /// use the notation ".subnode.subnode...". </remarks>
    class JsonTracker
    {
        public IList<NodeAssociation> KeyNodes { get; private set; }
        public IList<NodeAssociation> Associations { get; private set; }
        public IList<NodeRule> CustomRules { get; private set; }

        List<NodeAssociation> _KeyNodes = new List<NodeAssociation>();
        List<NodeAssociation> _Associations = new List<NodeAssociation>();
        List<NodeRule> _CustomRules = new List<NodeRule>();

        public JsonTracker() {
            this.KeyNodes = _KeyNodes.AsReadOnly();
            this.Associations = _Associations.AsReadOnly();
            this.CustomRules = _CustomRules.AsReadOnly();
        }

        public JsonTracker(string jsonDefinition) {
            var definition = JsonTrackerFile.FromString(jsonDefinition);
            foreach (var keyNode in definition.keyNodes) {
                AddKeyNode(keyNode.Key, keyNode.Value);
            }
            foreach (var association in definition.associations) {
                AddAssociation(association.Key, association.Value);
            }
        }

        public void AddKeyNode(string name, string path) {
            _KeyNodes.Add(new NodeAssociation(name, null, path.Split('.')));
        }
        public void AddAssociation(string name, string path) {
            _Associations.Add(new NodeAssociation(name, path));
        }
        public void AddAssociation(string name, string path, NodeFilterFunction filter) {
            _Associations.Add(new NodeAssociation(name, path, filter));
        }
        public void AddRule(string name, NodeRuleFunction rule) {
            this._CustomRules.Add(new NodeRule(name, rule));
        }

        /// <summary>
        /// Applies the specified JSON data to the specified tracker state
        /// </summary>
        public void Process(string json, TrackerState state) {
            Process(JObject.Parse(json), state);
        }
        /// <summary>
        /// Applies the specified JSON data to the specified tracker state
        /// </summary>
        public void Process(JObject json, TrackerState state) {
            Dictionary<string, JToken> keyNodes = new Dictionary<string, JToken>();

            // Identify our key nodes
            foreach (var node in this._KeyNodes) {
                //var jsonNode = FindNode(json, node.Path);
                var jsonNode = FindNode(json, null, null, node.Path);
                keyNodes.Add(node.Name, jsonNode);
            }

            // Find and apply our states
            foreach (var association in this._Associations) {
                var element = FindNode(json, keyNodes, association.KeyNode, association.Path);

                var value = (int)(element ?? 0);
                if (association.Filter != null) {
                    value = association.Filter(association.Name, value);
                }
                state.SetIndicatorLevel(association.Name, value);
            }

            var jsonGetter = (JsonValueGetterFunction)delegate(string path) {
                var association = new NodeAssociation(null, path);
                var element = FindNode(json, keyNodes, association.KeyNode, association.Path);
                return (int)(element ?? 0);

            };
            foreach (var rule in this._CustomRules) {
                var result = rule.Rule(jsonGetter);
                state.SetIndicatorLevel(rule.IndicatorName, result);
            }
        }

        /// <summary>Returns the specified node, or null if the node is not found.</summary>
        /// <remarks>'keyNodes' may be null if 'relativeTo' is null.</remarks>
        private JToken FindNode(JToken root, Dictionary<string, JToken> keyNodes, string relativeTo, IList<string> path) {
            JToken node = root;
            if (!string.IsNullOrEmpty(relativeTo)) {
                if (!keyNodes.TryGetValue(relativeTo, out node)) return null;
            }

            for (var i = 0; i < path.Count; i++) {
                if (node is JArray) {
                    var index = int.Parse(path[i]);
                    var array = (JArray)node;

                    if (index < 0 | index >= array.Count) return null;
                    node = node[index];
                } else if (node is JObject) {
                    if (!((JObject)node).TryGetValue(path[i], out node)) return null;
                }
            }

            return node;
        }
    }
    public struct NodeAssociation
    {
        public NodeAssociation(string name, string keyNode, IList<string> path)
            : this(name, keyNode, path, null) {
        }
        public NodeAssociation(string name, string keyNode, IList<string> path, NodeFilterFunction filter) {
            this.Name = name;
            this.Filter = filter;

            this.KeyNode = keyNode ?? string.Empty;
            var thisPath = new string[path.Count];
            for (var i = 0; i < path.Count; i++) thisPath[i] = path[i];

            this.Path = thisPath;
        }
        public NodeAssociation(string name, string path)
            : this(name, path, (NodeFilterFunction)null) {
        }
        public NodeAssociation(string name, string path, NodeFilterFunction filter) {
            this.Name = name;
            this.Filter = filter;

            var parts = path.Split('.');
            if (parts.Length < 2) throw new ArgumentException("Invalid path: missing root");

            this.KeyNode = parts[0];
            var thisPath = new string[parts.Length - 1];
            Array.Copy(parts, 1, thisPath, 0, thisPath.Length);

            this.Path = thisPath;
        }
        public readonly string KeyNode;
        public readonly IList<string> Path;
        public string Name;
        public NodeFilterFunction Filter;
    }

    public struct NodeRule
    {
        public NodeRule(string name, NodeRuleFunction rule) {
            this.IndicatorName = name;
            this.Rule = rule;
        }
        public readonly string IndicatorName;
        public readonly NodeRuleFunction Rule;
    }
    public delegate int JsonValueGetterFunction(string path);
    public delegate int NodeRuleFunction(JsonValueGetterFunction json);
    public delegate int NodeFilterFunction(string indicatorName, int value);

    public class JsonTrackerFile
    {
        public static JsonTrackerFile FromString(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonTrackerFile>(json);
        }
        public Dictionary<string, string> keyNodes { get; set; }
        public Dictionary<string, string> associations { get; set; }
    }
}
