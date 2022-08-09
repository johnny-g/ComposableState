using System;

namespace CompositeState.Linear
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class StateTransition
    {
        public string DebuggerDisplay { get; set; }
        public Enum Input { get; set; }
        public Enum[] Next { get; set; }
        public Action Output { get; set; }
        public Enum[] State { get; set; }
    }

}