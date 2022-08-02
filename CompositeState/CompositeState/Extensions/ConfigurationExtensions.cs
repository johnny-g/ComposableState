using System;
using System.Collections.Generic;
using System.Linq;
using CompositeState.Composite;
using CompositeState.Linear;

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

        private static readonly TransitionConfiguration[] EmptyTransitions = new TransitionConfiguration[] { };

        public class TransitionTraversal
        {
            public Enum Input { get; set; }
            public Enum[] Next { get; set; }
            public int Rank { get; set; }
        }

        public class StateTraversal
        {
            public StateConfiguration Configuration { get; set; }
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

        public static StateTransitionTable ToStateTransitionTable(
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
                            State = new[] { s.State, },
                            Transitions = s.Transitions.
                                Select(t => 
                                    new TransitionTraversal
                                    {
                                        Input = t.Input,
                                        Next = configuration.States.GetNextFullStatePath(t.Next),
                                        Rank = 1,
                                    }).
                                ToArray(),
                        }));

            List<StateTraversal> unrolled = new List<StateTraversal>();
            for (; visit.Any(); )
            {
                StateTraversal current = visit.Pop();
                if (current.Configuration.SubState == null) { unrolled.Add(current); }
                else
                {
                    foreach (StateConfiguration child in current.Configuration.SubState.States.OrderByStartStateThenPreserveOrder(current.Configuration.SubState.Start).Reverse())
                    {
                        Enum[] childState = current.State.Concat(new[] { child.State, }).ToArray();
                        TransitionTraversal[] childTransitions = child.Transitions.
                            Select(t =>
                                new TransitionTraversal
                                {
                                    Input = t.Input,
                                    Next = current.State.
                                        Concat(current.Configuration.SubState.States.GetNextFullStatePath(t.Next)).
                                        ToArray(),
                                    Rank = childState.Length,
                                }).
                            ToArray();

                        visit.Push(
                            new StateTraversal
                            {
                                Configuration = child,
                                State = childState,
                                Transitions = current.Transitions.Concat(childTransitions).ToArray(),
                            });
                    }
                }
            }

            StateTransitionTable.StateTuple[] states = new StateTransitionTable.StateTuple[unrolled.Count];
            for (int i = 0; i < states.Length; i++)
            {
                StateTraversal current = unrolled[i];

                TransitionTraversal[] transitions = current.Transitions.
                    GroupBy(t => t.Input).
                    Select(g => g.OrderByDescending(t => t.Rank).FirstOrDefault()).
                    ToArray();

                states[i] = new StateTransitionTable.StateTuple
                {
                    DebuggerDisplay = isDebuggerDisplayEnabled ? $"{string.Join<Enum>(".", current.State)} ({transitions.Length} transitions)" : string.Empty,
                    State = current.State,
                    Transitions = transitions.
                        Select(t => 
                            new StateTransitionTable.TransitionTuple
                            {
                                DebuggerDisplay = isDebuggerDisplayEnabled ? $"{string.Join<Enum>(".", current.State)} -- {t.Input} --> {string.Join<Enum>(".", t.Next)}" : string.Empty,
                                Input = t.Input,
                                Next = unrolled.IndexOf(unrolled.Single(s => s.State.SequenceEqual(t.Next))),
                                Output = null,
                            }).
                        ToArray(),
                };
            }

            return new StateTransitionTable(states);
        }

        public static CompositeStateMachine ToCompositeStateMachine(this StateMachineConfiguration configuration)
        {
            Dictionary<StateMachineConfiguration, CompositeStateMachine> mapped = new Dictionary<StateMachineConfiguration, CompositeStateMachine>();
            Stack<StateMachineConfiguration> unmapped = new Stack<StateMachineConfiguration>();
            Queue<StateMachineConfiguration> visit = new Queue<StateMachineConfiguration>(new[] { configuration, });

            for (; visit.Any() || unmapped.Any();)
            {
                StateMachineConfiguration current = visit.Any() ? visit.Dequeue() : unmapped.Pop();

                if (!mapped.ContainsKey(current))
                {
                    IEnumerable<StateMachineConfiguration> currentUnmapped = current.States.
                        Where(s => s.SubState != null && !mapped.ContainsKey(s.SubState)).
                        Select(s => s.SubState);

                    if (currentUnmapped.Any())
                    {
                        unmapped.Push(current);
                        foreach (StateMachineConfiguration c in currentUnmapped) { visit.Enqueue(c); }
                    }
                    else
                    {
                        StateTuple[] tuples = current.States.
                            Select(s =>
                                new StateTuple
                                {
                                    OnEnter = s.OnEnter?.Compile(),
                                    OnExit = s.OnExit?.Compile(),
                                    State = s.State,
                                    SubState = mapped.SingleOrDefault(m => m.Key == s.SubState).Value,
                                    Transitions = (s.Transitions ?? EmptyTransitions).
                                        Select(t =>
                                            new TransitionTuple
                                            {
                                                Input = t.Input,
                                                Next = t.Next,
                                                OnTransition = t.OnTransition?.Compile(),
                                            }).
                                        ToArray(),
                                }).
                            ToArray();

                        CompositeStateMachine machine = new CompositeStateMachine(tuples, current.Start);

                        mapped.Add(current, machine);
                    }
                }
            }

            return mapped[configuration];
        }

    }

}