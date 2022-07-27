using System;

namespace CompositeState.Composite
{

    public class StateTuple
    {
        public Action OnEnter { get; set; }
        public Action OnExit { get; set; }
        public Enum State { get; set; }
        public CompositeStateMachine SubState { get; set; }
        public TransitionTuple[] Transitions { get; set; }
    }

}