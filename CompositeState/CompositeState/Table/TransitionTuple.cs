using System;

namespace CompositeState.Table
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class TransitionTuple
    {
        public Enum Input { get; set; }
        public int Next { get; set; }
        public Action Output { get; set; }
        public string DebuggerDisplay { get; set; }
    }

}