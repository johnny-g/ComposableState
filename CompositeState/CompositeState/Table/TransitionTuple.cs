using System;

namespace CompositeState.Table
{

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay}")]
    public class TransitionTuple
    {
        public static readonly string DefaultDebuggerDisplay = $"{{{typeof(TransitionTuple).FullName}}}";

        public string DebuggerDisplay { get; set; }
        public Enum Input { get; set; }
        public int Next { get; set; }
        public Action Output { get; set; }
    }

}