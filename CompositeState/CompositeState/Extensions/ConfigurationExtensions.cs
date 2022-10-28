using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CompositeState
{

    public static class ConfigurationExtensions
    {
        private static IEqualityComparer<Enum[]> StatePathComparer = new StatePathEqualityComparer();

        public class StateTraversal
        {
            public StateConfiguration Configuration { get; set; }
            public Expression<OnEnterDelegate>[] OnEnter { get; set; }
            public Expression<OnExitDelegate>[] OnExit { get; set; }
            public Enum[] State { get; set; }
            public TransitionTraversal[] Transitions { get; set; }
        }

        public class TransitionTraversal
        {
            public Enum Input { get; set; }
            public Enum[] Next { get; set; }
            public Expression<OnTransitionDelegate> OnTransition { get; set; }
            public int Rank { get; set; }
        }

        public static string GetDotDelimited(this IEnumerable<Enum> values)
        {
            values = values ?? throw new ArgumentNullException(nameof(values), $"Cannot generate dot-delimited literal from null {nameof(values)}.");

            string dotDelimited = string.Join(".", values);
            return dotDelimited;
        }

        public static IEnumerable<StateConfiguration> OrderByStartStateThenPreserveOrder(
            this IEnumerable<StateConfiguration> states,
            Enum start)
        {
            StateConfiguration[] ordered = new[] { states.Single(s => s.State.Equals(start)), };
            ordered = ordered.Concat(states.Except(ordered)).ToArray();
            return ordered;
        }

        public static StateTransitionTable ToStateTransitionTable(
            this StateMachineConfiguration configuration,
            bool isDebuggerDisplayEnabled = false)
        {
            StateTransitionTable table = configuration.
                ToStateTransitions(isDebuggerDisplayEnabled).
                ToStateTransitionTable(isDebuggerDisplayEnabled);

            return table;
        }

        public static StateTransitionTable ToStateTransitionTable(
            this IEnumerable<StateTransition> stateTransitions,
            bool isDebuggerDisplayEnabled = false)
        {
            var stateTransitionsGrouped = stateTransitions.GroupBy(s => s.State, StatePathComparer).ToArray();

            var statesWithTransitions = stateTransitionsGrouped.Select(t => t.Key).ToArray();
            var statesWithNoTransitions = stateTransitions.Select(s => s.Next).Distinct(StatePathComparer).ToArray();
            var union = statesWithTransitions.Concat(statesWithNoTransitions.Except(statesWithTransitions, StatePathComparer)).ToArray();

            StateTuple[] stateTuples = stateTransitionsGrouped.
                Select(g =>
                    new StateTuple
                    {
                        DebuggerDisplay = isDebuggerDisplayEnabled ?
                            g.Key.GetDotDelimited() :
                            StateTuple.DefaultDebuggerDisplay,

                        State = g.Key,
                        Transitions = g.
                            Select(s =>
                                new TransitionTuple
                                {
                                    DebuggerDisplay = isDebuggerDisplayEnabled ?
                                        $"{s.State.GetDotDelimited()} -- {s.Input} --> {s.Next.GetDotDelimited()}" :
                                        TransitionTuple.DefaultDebuggerDisplay,

                                    Input = s.Input,
                                    Next = union.TakeWhile(u => !u.SequenceEqual(s.Next)).Count(),
                                    Output = s.Output,
                                }).
                            ToArray(),
                    }).
                Concat(
                    union.
                        Except(statesWithTransitions, StatePathComparer).
                        Select(s =>
                            new StateTuple
                            {
                                DebuggerDisplay = isDebuggerDisplayEnabled ?
                                    s.GetDotDelimited() :
                                    StateTuple.DefaultDebuggerDisplay,

                                State = s,
                                Transitions = Array.Empty<TransitionTuple>(),
                            })).
                ToArray();

            return new StateTransitionTable(stateTuples);
        }

        public static IEnumerable<StateTransition> ToStateTransitions(
            this StateMachineConfiguration configuration,
            bool isDebuggerDisplayEnabled = false)
        {
            Stack<StateTraversal> visit = new Stack<StateTraversal>(
                configuration.States.
                    OrderByStartStateThenPreserveOrder(configuration.Start).
                    Reverse().
                    Select(s =>
                        new StateTraversal
                        {
                            Configuration = s,
                            OnEnter = new[] { s.OnEnter, },
                            OnExit = new[] { s.OnExit, },
                            State = new[] { s.State, },
                            Transitions = (s.Transitions ?? Array.Empty<TransitionConfiguration>()).
                                Select(t =>
                                    new TransitionTraversal
                                    {
                                        Input = t.Input,
                                        Next = configuration.States.GetNextFullStatePath(t.Next),
                                        OnTransition = t.OnTransition,
                                        Rank = 1,
                                    }).
                                ToArray(),
                        }));

            List<StateTraversal> unrolled = new List<StateTraversal>();
            for (; visit.Any();)
            {
                StateTraversal current = visit.Pop();
                if (current.Configuration.SubState == null) { unrolled.Add(current); }
                else
                {
                    foreach (StateConfiguration child in current.Configuration.SubState.States.OrderByStartStateThenPreserveOrder(current.Configuration.SubState.Start).Reverse())
                    {
                        Enum[] childState = current.State.Concat(new[] { child.State, }).ToArray();
                        TransitionTraversal[] childTransitions = (child.Transitions ?? Array.Empty<TransitionConfiguration>()).
                            Select(t =>
                                new TransitionTraversal
                                {
                                    Input = t.Input,
                                    Next = current.State.
                                        Concat(current.Configuration.SubState.States.GetNextFullStatePath(t.Next)).
                                        ToArray(),
                                    OnTransition = t.OnTransition,
                                    Rank = childState.Length,
                                }).
                            ToArray();

                        visit.Push(
                            new StateTraversal
                            {
                                Configuration = child,
                                OnEnter = current.OnEnter.Concat(new[] { child.OnEnter, }).ToArray(),
                                OnExit = new[] { child.OnExit, }.Concat(current.OnExit).ToArray(),
                                State = childState,
                                Transitions = current.Transitions.Concat(childTransitions).ToArray(),
                            });
                    }
                }
            }

            StateTransition[] stateTransitions = unrolled.
                Where(currentState => currentState.Transitions.Any()).
                SelectMany(
                    currentState =>
                    {
                        OnExitDelegate[] onExits = currentState.OnExit.
                            Where(e => e != null).
                            Select(e => e.Compile()).
                            ToArray();

                        TransitionTraversal[] transitions = currentState.Transitions.
                            GroupBy(t => t.Input).
                            Select(g => g.OrderByDescending(t => t.Rank).FirstOrDefault()).
                            ToArray();

                        return transitions.
                            Select(
                                t => new StateTransition
                                {
                                    DebuggerDisplay = isDebuggerDisplayEnabled ? 
                                        $"{currentState.State.GetDotDelimited()} -- {t.Input} --> {t.Next.GetDotDelimited()}" :
                                        StateTransition.DefaultDebuggerDisplay,

                                    Input = t.Input,
                                    Next = t.Next,
                                    Output = t.GetOutput(unrolled, onExits),
                                    State = currentState.State,
                                });
                    }).
                ToArray();

            return stateTransitions;
        }

        // private methods

        private static Enum[] GetNextFullStatePath(this IEnumerable<StateConfiguration> states, Enum start)
        {
            List<Enum> path = new List<Enum>();
            Enum current = start;
            StateConfiguration configuration = null;
            for (; states != null;)
            {
                configuration = states.SingleOrDefault(s => s.State.Equals(current));
                path.Add(current);
                current = configuration.SubState?.Start;
                states = configuration.SubState?.States;
            }
            return path.ToArray();
        }

        private static OnTransitionDelegate GetOutput(
            this TransitionTraversal currentTransition,
            IList<StateTraversal> states,
            IEnumerable<OnExitDelegate> currentStateOnExits)
        {
            OnTransitionDelegate onTransition = currentTransition.OnTransition?.Compile();

            OnEnterDelegate[] onEnters = currentTransition.Next != null ?
                states.
                    Single(s => s.State.SequenceEqual(currentTransition.Next)).OnEnter.
                    Where(e => e != null).
                    Select(e => e.Compile()).
                    ToArray() :
                Array.Empty<OnEnterDelegate>();

            return (from, to) =>
            {
                foreach (OnExitDelegate onExit in currentStateOnExits) { onExit(from); }
                onTransition?.Invoke(from, to);
                foreach (OnEnterDelegate onEnter in onEnters) { onEnter(to); }
            };
        }

    }

}