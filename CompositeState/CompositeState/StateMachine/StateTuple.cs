using System;

namespace CompositeState
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