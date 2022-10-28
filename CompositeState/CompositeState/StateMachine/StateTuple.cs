using System;

namespace CompositeState.StateMachine
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class StateTuple
    {
        public static readonly string DefaultDebuggerDisplay = $"{{{typeof(StateTuple).FullName}}}";

        public string DebuggerDisplay { get; set; }
        public Enum[] State { get; set; }
        public TransitionTuple[] Transitions { get; set; }
    }

}