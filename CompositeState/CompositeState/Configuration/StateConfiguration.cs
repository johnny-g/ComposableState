using System;
using System.Linq.Expressions;

namespace CompositeState
{

    public class StateConfiguration
    {
        public Expression<Action> OnEnter { get; set; }
        public Expression<Action> OnExit { get; set; }
        public Enum State { get; set; }
        public StateMachineConfiguration SubState { get; set; }
        public TransitionConfiguration[] Transitions { get; set; }
    }

}