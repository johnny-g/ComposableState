using System;

namespace CompositeState.Composite
{

    public class TransitionTuple
    {
        public Enum Input { get; set; }
        public Enum Next { get; set; }
        public Action OnTransition { get; set; }
    }

}