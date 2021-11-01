using System;
using CompositeState.Composite;
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
                    new StateConfiguration { State = Level2State.D, },
                    new StateConfiguration { State = Level2State.E, },
                    new StateConfiguration { State = Level2State.F, },
                },
            };

        public static readonly StateMachineConfiguration Level1Configuration =
            new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[] 
                {
                    new StateConfiguration { State = Level1State.A, },
                    new StateConfiguration { State = Level1State.B, SubState = Level2Configuration, },
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
                    new StateConfiguration { State = Level3State.G, },
                    new StateConfiguration { State = Level3State.H, },
                    new StateConfiguration { State = Level3State.I, },
                },
            };

        public static readonly StateMachineConfiguration Complex2Configuration =
            new StateMachineConfiguration
            {
                Start = Level2State.D,
                States = new[]
                {
                    new StateConfiguration { State = Level2State.D, },
                    new StateConfiguration { State = Level2State.E, SubState = Complex3Configuration, },
                    new StateConfiguration { State = Level2State.F, },
                },
            };

        public static readonly StateMachineConfiguration Complex1Configuration =
            new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[]
                {
                    new StateConfiguration { State = Level1State.A, SubState = Complex2Configuration, },
                    new StateConfiguration { State = Level1State.B, SubState = Complex2Configuration, },
                    new StateConfiguration { State = Level1State.C, SubState = Complex3Configuration, },
                },
            };

        //  tests

        [Fact]
        public void ToCompositeStateMachine_WhenFlatConfiguration_ReturnsStateMachineWithStatesAndTransitions()
        {
            StateMachineConfiguration configuration = FlatConfiguration;

            CompositeStateMachine actual = configuration.ToCompositeStateMachine();

            Assert.Equal(FlatState.A, actual.Start);
            Assert.Equal(new Enum[] { FlatState.A, }, actual.State);
            Assert.Collection(
                actual.States,
                s =>
                {
                    Assert.Equal(FlatState.A, s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            Assert.Equal(Input.Continue, t.Input);
                            Assert.Equal(FlatState.B, t.Next);
                        });
                },
                s =>
                {
                    Assert.Equal(FlatState.B, s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            Assert.Equal(Input.Continue, t.Input);
                            Assert.Equal(FlatState.C, t.Next);
                        },
                        t =>
                        {
                            Assert.Equal(Input.GoBack, t.Input);
                            Assert.Equal(FlatState.A, t.Next);
                        });
                },
                s =>
                {
                    Assert.Equal(FlatState.C, s.State);
                    Assert.Collection(
                        s.Transitions,
                        t =>
                        {
                            Assert.Equal(Input.GoBack, t.Input);
                            Assert.Equal(FlatState.B, t.Next);
                        });
                });
        }

        [Fact]
        public void ToCompositeStateMachine_WhenTieredConfiguration_ReturnsStateMachineWithStates()
        {
            StateMachineConfiguration configuration = Level1Configuration;

            CompositeStateMachine actual = configuration.ToCompositeStateMachine();

            Assert.Equal(Level1State.A, actual.Start);
            Assert.Equal(new Enum[] { Level1State.A, }, actual.State);
            Assert.Collection(
                actual.States,
                s => Assert.Equal(Level1State.A, s.State),
                s =>
                {
                    Assert.Equal(Level1State.B, s.State);
                    Assert.NotNull(s.SubState);
                    CompositeStateMachine actualSubState = s.SubState;
                    Assert.Equal(Level2State.D, actualSubState.Start);
                    Assert.Collection(
                        actualSubState.States,
                        t => Assert.Equal(Level2State.D, t.State),
                        t => Assert.Equal(Level2State.E, t.State),
                        t => Assert.Equal(Level2State.F, t.State));
                },
                s => Assert.Equal(Level1State.C, s.State));
        }

        [Fact]
        public void ToCompositeStateMachine_WhenComplexConfiguration_ReturnsStateMachineWithSingletonSubStateMachines()
        {
            StateMachineConfiguration configuration = Complex1Configuration;

            CompositeStateMachine actual = configuration.ToCompositeStateMachine();

            Assert.Equal(new Enum[] { Level1State.A, Level2State.D, }, actual.State);
            Assert.Same(actual[Level1State.A].SubState, actual[Level1State.B].SubState);
            Assert.Same(actual[Level1State.C].SubState, actual[Level1State.B].SubState[Level2State.E].SubState);
        }

    }

}