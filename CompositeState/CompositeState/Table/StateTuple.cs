using System;

namespace CompositeState.Table
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class StateTuple
    {
        public Enum[] State { get; set; }
        public TransitionTuple[] Transitions { get; set; }
        public string DebuggerDisplay { get; set; }
    }

}