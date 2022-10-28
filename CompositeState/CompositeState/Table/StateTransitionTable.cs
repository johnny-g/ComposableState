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

        public StateTransitionTable(IEnumerable<StateTuple> states, IEnumerable<Enum> startState = null)
        {
            states = states ?? throw new ArgumentNullException(nameof(states), $"Cannot instantiate {nameof(StateTransitionTable)} with null {nameof(states)}.");
            if (!states.Any()) { throw new ArgumentException($"Cannot instantiate {nameof(StateTransitionTable)} with '{states.Count()}' {nameof(states)}.", nameof(states)); }

            int startStateIndex = DefaultStartStateIndex;
            if (startState != null)
            {
                if (!startState.Any()) { throw new ArgumentException($"Cannot instantiate {nameof(StateTransitionTable)} with {nameof(startState)} '{startState.GetDotDelimited()}'. Specify a valid {nameof(startState)} or specify null {nameof(startState)} to default to first {nameof(StateTuple.State)}.", nameof(startState)); }

                StateTuple startStateTuple = states.FirstOrDefault(s => s.State.SequenceEqual(startState));
                if (startStateTuple == null) { throw new ArgumentException($"Cannot instantiate {nameof(StateTransitionTable)} with {nameof(startState)} '{startState.GetDotDelimited()}'. Cannot find {nameof(startState)} '{startState.GetDotDelimited()}'. Specify a valid {nameof(startState)} or specify null {nameof(startState)} to default to first {nameof(StateTuple.State)}.", nameof(startState)); }

                startStateIndex = states.TakeWhile(s => s != startStateTuple).Count();
            }

            this.startState = currentState = startStateIndex;
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
                Enum[] from = states[currentState].State;
                currentState = transition.Next;
                Enum[] to = states[currentState].State;
                transition.Output?.Invoke(from, to);
            }

            return new StateMachineResponse
            {
                Result = result,
                State = states[currentState].State,
            };
        }

    }

}