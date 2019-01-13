using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoTracker
{
    /// <summary>
    /// Provides functionality that can be utilized by JSON tracker files
    /// </summary>
    /// <remarks>Consider prefixing functions to avoid naming collisions with other libraries,
    /// for example, beginning all functions in a library with "myLib_".</remarks>
    public sealed class TrackerFunction
    {
        public string Name { get; private set; }
        public IList<string> Parameters { get; private set; }
        public TrackerFuncExecutor Executor { get; private set; }
        public bool IsVariadic { get { return Parameters.Count > 0 && Parameters[Parameters.Count - 1] == Variardic; } }

        public TrackerFunction(string name, IList<string> prams, TrackerFuncExecutor exec) {
            this.Name = name;
            this.Executor = exec;
            if (prams == null) {
                prams = new string[0];
            } else {
                this.Parameters = new List<string>(prams);
            }

            for (int i = 0; i < this.Parameters.Count; i++) {
                var varArgPermitted = (i == this.Parameters.Count - 1);
                var pram = this.Parameters[i];
                if (pram == _variadic && !varArgPermitted) throw new ArgumentException("Variadic parameter can only the the last parameter.");
            }
        }

        #region static

        static Dictionary<string, TrackerFunction> _functions = new Dictionary<string, TrackerFunction>();

        static TrackerFunction() {
            AddStandardFunctions();
        }

        public static void AddFunction(TrackerFunction f) {
            if (_functions.ContainsKey(f.Name)) throw new ArgumentException("Function " + f.Name + " already defined.");
            
            _functions.Add(f.Name, f);
        }

        static int? InvokeFunction(string name, System.Collections.IList arguments, NodeResolver document) {
            var finalArgs = new List<int?>();

            var func = GetFunction(name);
            if (func == null) return null;

            int paramCount = func.Parameters.Count;
            if (func.IsVariadic) paramCount--;

            if (arguments.Count < paramCount) {
                TrackerErrorLog.LogError("Function " + func.Name + " called with only " + arguments.Count + "arguments.");
            }
            if (arguments.Count > paramCount && !func.IsVariadic) {
                TrackerErrorLog.LogError("Function " + func.Name + " called with too many arguments.");
            }

            for (int i = 0; i < arguments.Count; i++) {
                finalArgs.Add(EvaluateArg(arguments[i], document));
            }

            while (finalArgs.Count < paramCount) finalArgs.Add(null);

            return func.Executor(finalArgs);
        }

        public static int? EvaluateExpression(JToken expr, NodeResolver resolver) {
            return EvaluateArg(expr, resolver);
        }
        private static int? EvaluateArg(object arg, NodeResolver document) {
            if (arg == null) return null;
            if (arg is int?) return (int?)arg;
            if (arg is int) return (int)arg;

            if (arg is JToken) {
                if (arg is JObject) {
                    var jArg = (JObject)arg;
                    if (jArg.Count != 1) {
                        TrackerErrorLog.LogError("Invalid object. Only one child allowed.");
                        return null;
                    }
                    string funcName = null;
                    JToken argList = null;
                    // It's kind of dumb that you can't just numerically index the properties
                    foreach (var p in jArg.Properties()) {
                        funcName = p.Name;
                        argList = p.Value;

                        if (!(argList is JArray)) {
                            TrackerErrorLog.LogError("Function parameters must be passed into an array.");
                            return null;
                        }
                        if (funcName.Length == 0 || funcName[0] != '@') {
                            TrackerErrorLog.LogError("Unexpected object. If this is supposed to be a function, prefix \"" + funcName + "\" with \"@\".");
                            return null;
                        }
                        funcName = funcName.Substring(1);
                    }

                    return InvokeFunction(funcName, (JArray)argList, document);

                } else {
                    var jArg = (JToken)arg;
                    if (jArg.Type == JTokenType.String) {
                        // Document property reference
                        return (int)document((string)jArg);
                    } else if (jArg.Type == JTokenType.Integer) {
                        return (int)jArg;
                    }
                }
            }

            TrackerErrorLog.LogError("Invalid argument type: " + arg.GetType().ToString());
            if (arg is int) return (int)arg;
            return null;
        }

        /// <summary>
        /// Returns the specified function, or null if the function is not found.
        /// </summary>
        public static TrackerFunction GetFunction(string name) {
            TrackerFunction result;
            if (_functions.TryGetValue(name, out result)) return result;

            return null;
        }

        static readonly string _variadic = "__variadic__";
        /// <summary>
        /// Represents an arbitrary number of parameters. Can be used as the last or only parameter
        /// for a tracker function.
        /// </summary>
        public static string Variardic { get { return _variadic; } }

        #endregion

        #region Standard Functions

        private static void AddStandardFunctions() {
            AddFunction(new TrackerFunction(
                "PlusOne",
                new string[] { "value" },
                delegate(IList<int?> args) { return args[0] + 1; }
            ));
        }

        #endregion
    }

    /// <summary>
    /// Implements logic for a tracker function.
    /// </summary>
    /// <param name="args">Arguments provided to the function. Omitted arguments will be null.</param>
    /// <returns>An integer value, or null</returns>
    /// <remarks>An executor should handle null arguments gracefully. In the event
    /// of invalid arguments the executor may return null. The returned null might
    /// then be used as an argument to another executor, or returned as the final
    /// result of the expression, in which case it will be treated as a non-error
    /// condition and ignored. Alternatively an executor may throw a 
    /// TrackerExecutorException, which halts evaluation of the expression and causes
    /// the error to be logged.
    /// </remarks>
    public delegate int? TrackerFuncExecutor(IList<int?> args);

    /// <summary>
    /// Represents an error thrown by a tracker function executor to indicate that a fatal error 
    /// occurred in the evaluation of a value. The value will 
    /// </summary>
    [Serializable]
    public class TrackerExecutorException : Exception
    {
        public TrackerExecutorException() { }
        public TrackerExecutorException(string message) : base(message) { }
        public TrackerExecutorException(string message, Exception inner) : base(message, inner) { }
        protected TrackerExecutorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
