using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeState.Linear
{

    public class StateTransitionTable : IStateMachine
    {
        public class StateTuple
        {
            public Enum[] State { get; set; }
            public TransitionTuple[] Transitions { get; set; }
        }

        public class TransitionTuple
        {
            public Enum Input { get; set; }
            public int Next { get; set; }
            public Action Output { get; set; }
        }

        private int currentState = 0;
        private StateTuple[] states = null;

        public StateTransitionTable(IEnumerable<StateTuple> states)
        {
            this.states = states.ToArray();
        }

        // IStateMachine members

        public IEnumerable<Enum> State => states[currentState].State;

        public StateMachineResponse Fire(Enum input)
        {
            StateMachineResult result = StateMachineResult.NoAction;

            TransitionTuple transition = states[currentState].Transitions.SingleOrDefault(t => t.Input.Equals(input));
            if (transition != null)
            {
                result = StateMachineResult.Transitioned;
                currentState = transition.Next;
                transition.Output?.Invoke();
            }

            return new StateMachineResponse
            {
                Result = result,
                State = states[currentState].State,
            };
        }

    }

}