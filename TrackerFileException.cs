using System;
using System.Collections.Generic;
using System.Text;

namespace AutoTracker
{

    [Serializable]
    public class TrackerFileException : Exception
    {
        public TrackerFileException() { }
        public TrackerFileException(string message) : base(message) { }
        public TrackerFileException(string message, Exception inner) : base(message, inner) { }
        protected TrackerFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
