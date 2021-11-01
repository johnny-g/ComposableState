using System.Collections.Generic;
using System.Linq;
using CompositeState.Composite;

namespace CompositeState
{

    public static class ConfigurationExtensions
    {

        private static readonly TransitionConfiguration[] EmptyTransitions = new TransitionConfiguration[] { };

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
                                    State = s.State,
                                    SubState = mapped.SingleOrDefault(m => m.Key == s.SubState).Value,
                                    Transitions = (s.Transitions ?? EmptyTransitions).
                                        Select(t =>
                                            new TransitionTuple
                                            {
                                                Input = t.Input,
                                                Next = t.Next,
                                                OnExit = t.OnExit?.Compile(),
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