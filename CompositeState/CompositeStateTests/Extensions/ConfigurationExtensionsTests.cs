using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CompositeState.Linear;
using Xunit;

namespace CompositeState
{

    public class ConfigurationExtensionsTests
    {

        public enum Input { Continue, GoBack, }

        public enum FlatState { A, B, C, }

        public static readonly StateMachineConfiguration FlatConfiguration =
            new StateMachineConfiguration
            {
                Start = FlatState.A,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = FlatState.A,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = FlatState.B, }, },
                    },
                    new StateConfiguration
                    {
                        State = FlatState.B,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = Input.Continue, Next = FlatState.C, },
                            new TransitionConfiguration { Input = Input.GoBack, Next = FlatState.A, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = FlatState.C,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.GoBack, Next = FlatState.B, }, },
                    },
                },
            };

        public enum Level1State { A, B, C, }
        public enum Level2State { D, E, F, }

        public static readonly StateMachineConfiguration Level2Configuration =
            new StateMachineConfiguration
            {
                Start = Level2State.D,
                States = new[]
                {
                    new StateConfiguration { State = Level2State.D, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.E, }, }, },
                    new StateConfiguration { State = Level2State.E, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.F, }, }, },
                    new StateConfiguration { State = Level2State.F, },
                },
            };

        public static readonly StateMachineConfiguration Level1Configuration =
            new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[] 
                {
                    new StateConfiguration { State = Level1State.A, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.B, }, }, },
                    new StateConfiguration { State = Level1State.B, SubState = Level2Configuration, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.C, }, }, },
                    new StateConfiguration { State = Level1State.C, },
                },
            };

        public enum Level3State { G, H, I, }

        public static readonly StateMachineConfiguration Complex3Configuration =
            new StateMachineConfiguration
            {
                Start = Level3State.G,
                States = new[]
                {
                    new StateConfiguration { State = Level3State.G, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level3State.H, }, }, },
                    new StateConfiguration { State = Level3State.H, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level3State.I, }, }, },
                    new StateConfiguration { State = Level3State.I, },
                },
            };

        public static readonly StateMachineConfiguration Complex2Configuration =
            new StateMachineConfiguration
            {
                Start = Level2State.D,
                States = new[]
                {
                    new StateConfiguration { State = Level2State.D, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.E, }, }, },
                    new StateConfiguration { State = Level2State.E, SubState = Complex3Configuration, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.F, }, }, },
                    new StateConfiguration { State = Level2State.F, },
                },
            };

        public static readonly StateMachineConfiguration Complex1Configuration =
            new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[]
                {
                    new StateConfiguration { State = Level1State.A, SubState = Complex2Configuration, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.B, }, }, },
                    new StateConfiguration { State = Level1State.B, SubState = Complex2Configuration, Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.C, }, }, },
                    new StateConfiguration { State = Level1State.C, SubState = Complex3Configuration, },
                },
            };

        public static readonly IEnumerable<object[]> GetDotDelimited_WhenNotNull_Data =
            new[]
            {
                new object[] { new Enum[] { Level1State.A, Level2State.D, Level3State.G, }, "A.D.G", },
                new object[] { new Enum[] { Level1State.A, }, "A", },
                new object[] { new Enum[] { }, string.Empty, },
            };

        //  tests

        [Theory]
        [MemberData(nameof(GetDotDelimited_WhenNotNull_Data))]
        public void GetDotDelimited_WhenNotNull_ReturnsLiteral(Enum[] values, string expected)
        {
            string actual = values.GetDotDelimited();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDotDelimited_WhenNull_ThrowsArgumentNull()
        {
            Enum[] statePath = null;

            ArgumentNullException actual = Assert.Throws<ArgumentNullException>("values", () => statePath.GetDotDelimited());

            Assert.Equal("Cannot generate dot-delimited literal from null values. (Parameter 'values')", actual.Message);
        }

        [Fact]
        public void ToStateTransitions_WhenSimpleHierarchicalConfiguration_ReturnsStateTransitions()
        {
            StateMachineConfiguration subStateE = new StateMachineConfiguration
            {
                Start = Level3State.G,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = Level3State.G,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level3State.H, }, },
                    },
                    new StateConfiguration
                    {
                        State = Level3State.H,
                        Transitions = new TransitionConfiguration[] { },
                    },
                },
            };

            StateMachineConfiguration subStateB = new StateMachineConfiguration
            {
                Start = Level2State.D,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = Level2State.D,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.E, }, },
                    },
                    new StateConfiguration
                    {
                        State = Level2State.E,
                        SubState = subStateE,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.F, }, },
                    },
                    new StateConfiguration
                    {
                        State = Level2State.F,
                        Transitions = new TransitionConfiguration[] { },
                    },
                },
            };

            StateMachineConfiguration configuration = new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = Level1State.A,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.B, }, },
                    },
                    new StateConfiguration
                    {
                        State = Level1State.B,
                        SubState = subStateB,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.C, }, },
                    },
                    new StateConfiguration
                    {
                        State = Level1State.C,
                        Transitions = new TransitionConfiguration[] { },
                    },
                },
            };

            StateTransition[] stateTransitions = configuration.
                ToStateTransitions(isDebuggerDisplayEnabled: true).
                ToArray();

            Assert.Collection(
                stateTransitions,
                s =>
                {
                    Assert.Equal(State.Path(Level1State.A), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.B, Level2State.D), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.B, Level2State.D), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.G), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.G), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.H), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.H), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.B, Level2State.F), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.B, Level2State.F), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.C), s.Next);
                });
        }

        [Fact]
        public void ToStateTransitions_WhenSimpleCyclicConfiguration_ReturnsStateTransitions()
        {
            StateMachineConfiguration configuration = new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = Level1State.A,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = Input.Continue, Next = Level1State.B, },
                            new TransitionConfiguration { Input = Input.GoBack, Next = Level1State.A, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = Level1State.B,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.GoBack, Next = Level1State.A, }, },
                    },
                },
            };

            StateTransition[] stateTransitions = configuration.
                ToStateTransitions(isDebuggerDisplayEnabled: true).
                ToArray();

            Assert.Collection(
                stateTransitions,
                s =>
                {
                    Assert.Equal(State.Path(Level1State.A), s.State);
                    Assert.Equal(Input.Continue, s.Input);
                    Assert.Equal(State.Path(Level1State.B), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.A), s.State);
                    Assert.Equal(Input.GoBack, s.Input);
                    Assert.Equal(State.Path(Level1State.A), s.Next);
                },
                s =>
                {
                    Assert.Equal(State.Path(Level1State.B), s.State);
                    Assert.Equal(Input.GoBack, s.Input);
                    Assert.Equal(State.Path(Level1State.A), s.Next);
                });
        }

        [Fact]
        public void ToStateTransitionTable_WhenFlatConfiguration_ReturnsStateTransitionTable()
        {
            StateMachineConfiguration configuration = FlatConfiguration;

            Table.StateTransitionTable actual = configuration.ToStateTransitionTable();

            Assert.Equal(State.Path(FlatState.A), actual.Start);
            Assert.Collection(
                actual.States,
                s =>
                {
                    Assert.Equal(State.Path(FlatState.A), s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            int expectedNextStateIndex = 1;
                            Assert.Equal(Input.Continue, t.Input);
                            Assert.Equal(expectedNextStateIndex, t.Next);

                            Enum[] expectedNextState = State.Path(FlatState.B);
                            Enum[] actualNextState = actual[t.Next].State;
                            Assert.Equal(expectedNextState, actualNextState);
                        });
                },
                s =>
                {
                    Assert.Equal(State.Path(FlatState.B), s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            int expectedNextStateIndex = 2;
                            Assert.Equal(Input.Continue, t.Input);
                            Assert.Equal(expectedNextStateIndex, t.Next);

                            Enum[] expectedNextState = State.Path(FlatState.C);
                            Enum[] actualNextState = actual[t.Next].State;
                            Assert.Equal(expectedNextState, actualNextState);
                        },
                        t =>
                        {
                            int expectedNextStateIndex = 0;
                            Assert.Equal(Input.GoBack, t.Input);
                            Assert.Equal(expectedNextStateIndex, t.Next);

                            Enum[] expectedNextState = State.Path(FlatState.A);
                            Enum[] actualNextState = actual[t.Next].State;
                            Assert.Equal(expectedNextState, actualNextState);
                        });
                },
                s =>
                {
                    Assert.Equal(State.Path(FlatState.C), s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            int expectedNextStateIndex = 1;
                            Assert.Equal(Input.GoBack, t.Input);
                            Assert.Equal(expectedNextStateIndex, t.Next);

                            Enum[] expectedNextState = State.Path(FlatState.B);
                            Enum[] actualNextState = actual[t.Next].State;
                            Assert.Equal(expectedNextState, actualNextState);
                        });
                });
        }

        [Fact]
        public void ToStateTransitionTable_WhenTieredConfiguration_ReturnsStateTransitionTable()
        {
            StateMachineConfiguration configuration = Level1Configuration;

            Table.StateTransitionTable actual = configuration.ToStateTransitionTable();

            Assert.Equal(State.Path(Level1State.A), actual.Start);
            Assert.Collection(
                actual.States,
                s => Assert.Equal(State.Path(Level1State.A), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.D), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.E), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.F), s.State),
                s => Assert.Equal(State.Path(Level1State.C), s.State));
        }

        [Fact]
        public void ToStateTransitionTable_WhenComplexConfiguration_ReturnsStateTransitionTable()
        {
            StateMachineConfiguration configuration = Complex1Configuration;

            Table.StateTransitionTable actual = configuration.ToStateTransitionTable();

            Assert.Equal(State.Path(Level1State.A, Level2State.D), actual.Start);
            Assert.Collection(
                actual.States,
                s => Assert.Equal(State.Path(Level1State.A, Level2State.D), s.State),
                s => Assert.Equal(State.Path(Level1State.A, Level2State.E, Level3State.G), s.State),
                s => Assert.Equal(State.Path(Level1State.A, Level2State.E, Level3State.H), s.State),
                s => Assert.Equal(State.Path(Level1State.A, Level2State.E, Level3State.I), s.State),
                s => Assert.Equal(State.Path(Level1State.A, Level2State.F), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.D), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.G), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.H), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.E, Level3State.I), s.State),
                s => Assert.Equal(State.Path(Level1State.B, Level2State.F), s.State),
                s => Assert.Equal(State.Path(Level1State.C, Level3State.G), s.State),
                s => Assert.Equal(State.Path(Level1State.C, Level3State.H), s.State),
                s => Assert.Equal(State.Path(Level1State.C, Level3State.I), s.State));
        }

    }

}