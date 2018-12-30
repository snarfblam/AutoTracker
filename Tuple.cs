using System;
using System.Collections.Generic;
using System.Text;

namespace AutoTracker
{
    static class Tuple
    {
        public static Tuple<T0, T1> Create<T0, T1>(T0 i0, T1 i1) {
            return new Tuple<T0, T1>(i0, i1);
        }
    }

    class Tuple<T0, T1>
    {
        public Tuple(T0 i0, T1 i1) {
            this.Item0 = i0;
            this.Item1 = i1;
        }
        public T0 Item0 { get; private set; }
        public T1 Item1 { get; private set; }
    }
}
