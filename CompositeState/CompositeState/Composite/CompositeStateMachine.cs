using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeState.Composite
{

    public class CompositeStateMachine : IStateMachine
    {
        public static readonly Enum[] EmptyState = new Enum[] { };

        private readonly Enum start = null;
        private readonly StateTuple[] states = null;
        private StateTuple current = null;

        public StateTuple this[Enum state] => states.SingleOrDefault(s => s.State.Equals(state));

        public Enum Start => start;

        public IEnumerable<StateTuple> States => states;

        public CompositeStateMachine(IEnumerable<StateTuple> states, Enum start)
        {
            this.states = states.ToArray();
            this.start = start;

            Reset();
        }

        // IStateMachine members

        public IEnumerable<Enum> State => new[] { current.State, }.Concat(current.SubState?.State ?? EmptyState);

        public StateMachineResponse Fire(Enum input)
        {
            // 1. transition sub-state
            StateMachineResult result = current.SubState?.Fire(input).Result ?? StateMachineResult.NoAction;

            // 2. if sub-state did not transition, then attempt transition
            if (result == StateMachineResult.NoAction)
            {
                TransitionTuple transition = current.Transitions.SingleOrDefault(t => t.Input.Equals(input));

                if (transition != null)
                {
                    current.SubState?.OnExit();
                    current.OnExit?.Invoke();
                    current.SubState?.Reset();
                    current = states.Single(s => s.State.Equals(transition.Next));
                    current.OnEnter?.Invoke();
                    current.SubState?.OnEnter();

                    result = StateMachineResult.Transitioned;
                }
            }

            return new StateMachineResponse
            {
                Result = result,
                State = State.ToArray(),
            };
        }

        // public methods

        public void OnEnter()
        {
            current.OnEnter?.Invoke();
            current.SubState?.OnEnter();
        }

        public void OnExit()
        {
            current.SubState?.OnExit();
            current.OnExit?.Invoke();
        }

        public void Reset()
        {
            current = states.Single(s => s.State.Equals(start));
        }
    }

}