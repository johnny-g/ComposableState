using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeState.Table
{

    public class StateTransitionTable : IStateMachine
    {
        public static readonly int DefaultStartStateIndex = 0;

        private int currentState = DefaultStartStateIndex;
        private int startState = DefaultStartStateIndex;
        private StateTuple[] states = null;

        public StateTuple this[int stateIndex] { get => states[stateIndex]; }

        public IEnumerable<Enum> Start => states[startState].State;

        public IEnumerable<StateTuple> States => states;

        public StateTransitionTable(IEnumerable<StateTuple> states, int? startState = 0)
        {
            this.startState = startState ?? DefaultStartStateIndex;
            this.states = states.ToArray();

            currentState = this.startState;
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
                currentState = transition.Next.Value;
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