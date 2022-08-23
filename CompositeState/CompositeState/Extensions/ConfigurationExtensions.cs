using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CompositeState
{

    public static class ConfigurationExtensions
    {
        public static IEnumerable<StateConfiguration> OrderByStartStateThenPreserveOrder(
            this IEnumerable<StateConfiguration> states,
            Enum start)
        {
            StateConfiguration[] ordered = new[] { states.Single(s => s.State.Equals(start)), };
            ordered = ordered.Concat(states.Except(ordered)).ToArray();
            return ordered;
        }

        public class TransitionTraversal
        {
            public Enum Input { get; set; }
            public Enum[] Next { get; set; }
            public Expression<Action> OnTransition { get; set; }
            public int Rank { get; set; }
        }

        public class StateTraversal
        {
            public StateConfiguration Configuration { get; set; }
            public Expression<Action>[] OnEnter { get; set; }
            public Expression<Action>[] OnExit { get; set; }
            public Enum[] State { get; set; }
            public TransitionTraversal[] Transitions { get; set; }
        }

        private static Enum[] GetNextFullStatePath(this IEnumerable<StateConfiguration> states, Enum start)
        {
            List<Enum> path = new List<Enum>();
            Enum current = start;
            StateConfiguration configuration = null;
            for (; states != null; )
            {
                configuration = states.SingleOrDefault(s => s.State.Equals(current));
                path.Add(current);
                current = configuration.SubState?.Start;
                states = configuration.SubState?.States;
            }
            return path.ToArray();
        }

        public static Table.StateTransitionTable ToStateTransitionTable(
            this StateMachineConfiguration configuration,
            bool isDebuggerDisplayEnabled = false)
        {
            Table.StateTransitionTable table = configuration.
                ToStateTransitions(isDebuggerDisplayEnabled).
                ToStateTransitionTable(isDebuggerDisplayEnabled);

            return table;
        }

        public static Table.StateTransitionTable ToStateTransitionTable(
            this IEnumerable<Linear.StateTransition> stateTransitions,
            bool isDebuggerDisplayEnabled = false)
        {
            var grouped = stateTransitions.
                GroupBy(s => s.State).
                ToArray();

            Table.StateTuple[] states = grouped.
                Select(g =>
                    new Table.StateTuple
                    {
                        DebuggerDisplay = isDebuggerDisplayEnabled ? 
                            g.Key.GetDotDelimited() : 
                            Table.StateTuple.DefaultDebuggerDisplay,

                        State = g.Key,
                        Transitions = g.
                            Select(s =>
                                new Table.TransitionTuple
                                {
                                    DebuggerDisplay = isDebuggerDisplayEnabled ?
                                        $"{s.State.GetDotDelimited()} -- {s.Input} --> {s.Next.GetDotDelimited()}" :
                                        Table.TransitionTuple.DefaultDebuggerDisplay,

                                    Input = s.Input,
                                    Next = s.Next != null ?
                                        grouped.TakeWhile(group => !group.Key.SequenceEqual(s.Next)).Count() :
                                        default(int?),
                                    Output = s.Output,
                                }).
                            ToArray(),
                    }).
                ToArray();

            return new Table.StateTransitionTable(states);
        }

        public static IEnumerable<Linear.StateTransition> ToStateTransitions(
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

            Linear.StateTransition[] stateTransitions = unrolled.
                SelectMany(
                    currentState =>
                    {
                        Action[] onExits = currentState.OnExit.
                            Where(e => e != null).
                            Select(e => e.Compile()).
                            ToArray();

                        TransitionTraversal[] transitions = currentState.Transitions.
                            GroupBy(t => t.Input).
                            Select(g => g.OrderByDescending(t => t.Rank).FirstOrDefault()).
                            ToArray();

                        if (!transitions.Any())
                        {
                            transitions = new[] { new TransitionTraversal { }, };
                        }

                        return transitions.
                            Select(
                                t => new Linear.StateTransition
                                {
                                    DebuggerDisplay = isDebuggerDisplayEnabled ? 
                                        (
                                            t.Next != null ? 
                                                $"{currentState.State.GetDotDelimited()} -- {t.Input} --> {t.Next.GetDotDelimited()}" : 
                                                $"{currentState.State.GetDotDelimited()} (no transition)"
                                        ) : 
                                        Linear.StateTransition.DefaultDebuggerDisplay,

                                    Input = t.Input,
                                    Next = t.Next,
                                    Output = GetOutput(t, unrolled, onExits),
                                    State = currentState.State,
                                });
                    }).
                ToArray();

            return stateTransitions;
        }

        private static string GetDotDelimited(this IEnumerable<Enum> values)
        {
            string dotDelimited = string.Join(".", values);
            return dotDelimited;
        }

        private static Action GetOutput(
            this TransitionTraversal currentTransition,
            IList<StateTraversal> states,
            IEnumerable<Action> currentStateOnExits)
        {
            Action onTransition = currentTransition.OnTransition?.Compile();

            Action[] onEnters = currentTransition.Next != null ?
                states.
                    Single(s => s.State.SequenceEqual(currentTransition.Next)).OnEnter.
                    Where(e => e != null).
                    Select(e => e.Compile()).
                    ToArray() :
                Array.Empty<Action>();

            return () =>
            {
                foreach (Action onExit in currentStateOnExits) { onExit(); }
                onTransition?.Invoke();
                foreach (Action onEnter in onEnters) { onEnter(); }
            };
        }

    }

}