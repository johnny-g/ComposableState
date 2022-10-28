using System;

namespace CompositeState.Linear
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class StateTransition
    {
        public static readonly string DefaultDebuggerDisplay = $"{{{typeof(StateTransition).FullName}}}";

        public string DebuggerDisplay { get; set; }
        public Enum Input { get; set; }
        public Enum[] Next { get; set; }
        public OnTransitionDelegate Output { get; set; }
        public Enum[] State { get; set; }
    }

}