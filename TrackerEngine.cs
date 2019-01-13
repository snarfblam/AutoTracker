using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoTracker
{
    /// <summary>
    /// Uses 'associations' to process game state and update the tracker state.
    /// </summary>
    /// <remarks>JSON paths are specified using dot notation, in the form of 
    /// "keynode.subnode.subnode...". To specify a path relative to the root,
    /// use the notation ".subnode.subnode...". </remarks>
    class TrackerEngine
    {
        public IList<NodeAssociation> KeyNodes { get; private set; }
        public IList<NodeAssociation> Associations { get; private set; }
        public IList<NodeRule> CustomRules { get; private set; }

        List<NodeAssociation> _KeyNodes = new List<NodeAssociation>();
        List<NodeAssociation> _Associations = new List<NodeAssociation>();
        List<NodeRule> _CustomRules = new List<NodeRule>();

        public TrackerEngine() {
            this.KeyNodes = _KeyNodes.AsReadOnly();
            this.Associations = _Associations.AsReadOnly();
            this.CustomRules = _CustomRules.AsReadOnly();
        }

        public TrackerEngine(string jsonDefinition) {
            var definition = JsonTrackerFile.FromString(jsonDefinition);
            foreach (var keyNode in definition.keyNodes) {
                AddKeyNode(keyNode.Key, keyNode.Value);
            }
            foreach (var association in definition.associations) {
                if (association.Value is string) {
                    AddAssociation(association.Key, (string)association.Value);
                } else {
                    if (((JToken)association.Value).Type == JTokenType.String) {
                        AddAssociation(association.Key, (string)(JToken)association.Value);
                    } else {
                        AddAssociation(association.Key, (JToken)association.Value);
                    }
                }
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
        private void AddAssociation(string name, JToken expr) {
            _Associations.Add(new NodeAssociation(name, expr));
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
            var nodeGetter = (NodeResolver)delegate(string path) {
                var association = new NodeAssociation(null, path);
                return FindNode(json, keyNodes, association.KeyNode, association.Path);
            };
            var jsonGetter = (JsonValueGetterFunction)delegate(string path) {
                return (int)(nodeGetter(path) ?? 0);
            };
            // Identify our key nodes
            foreach (var node in this._KeyNodes) {
                //var jsonNode = FindNode(json, node.Path);
                var jsonNode = FindNode(json, null, null, node.Path);
                keyNodes.Add(node.Name, jsonNode);
            }

            // Find and apply our states
            foreach (var association in this._Associations) {
                if (association.Expression == null) {
                    var element = FindNode(json, keyNodes, association.KeyNode, association.Path);

                    var value = (int)(element ?? 0);
                    if (association.Filter != null) {
                        value = association.Filter(association.Name, value);
                    }
                    state.SetIndicatorLevel(association.Name, value);
                } else {
                    var value = TrackerFunction.EvaluateExpression((JToken)association.Expression, nodeGetter);
                    if(value != null) state.SetIndicatorLevel(association.Name, value.Value);

                }
            }



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
            this.Expression = null;

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
            this.Expression = null;

            var parts = path.Split('.');
            if (parts.Length < 2) throw new ArgumentException("Invalid path: missing root");

            this.KeyNode = parts[0];
            var thisPath = new string[parts.Length - 1];
            Array.Copy(parts, 1, thisPath, 0, thisPath.Length);

            this.Path = thisPath;
        }
        public NodeAssociation(string name, JToken expr) {
            this.Name = name;
            this.Filter = null;
            this.KeyNode = null;
            this.Expression = expr;
            this.Path = null;
        }
        public readonly string KeyNode;
        public readonly IList<string> Path;
        public readonly JToken Expression;
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
    /// <summary>
    /// Finds a node within a document given the specified path.
    /// </summary>
    public delegate JToken NodeResolver(string path);
    
    /// <summary>
    /// Returns the value of a node within a document given the specified path. The node must contain a literal value.
    /// </summary>
    // Todo: deprecated
    public delegate int JsonValueGetterFunction(string path);
    public delegate int NodeRuleFunction(JsonValueGetterFunction json);
    public delegate int NodeFilterFunction(string indicatorName, int value);

    public class JsonTrackerFile
    {
        public static JsonTrackerFile FromString(string json) {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonTrackerFile>(json);
        }
        public Dictionary<string, string> keyNodes { get; set; }
        public Dictionary<string, JToken> associations { get; set; }
    }
}
