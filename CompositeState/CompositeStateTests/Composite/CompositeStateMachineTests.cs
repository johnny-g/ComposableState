using System;
using Xunit;

namespace CompositeState.Composite
{

    public class CompositeStateMachineTests
    {

        public enum Input { Continue, GoBack, }

        public enum Level1State { A, B, C, }
        public enum Level2State { D, E, F, }

        public static readonly StateMachineConfiguration Level2Configuration =
            new StateMachineConfiguration
            {
                Start = Level2State.D,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = Level2State.D,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level2State.E, }, }
                    },
                    new StateConfiguration
                    {
                        State = Level2State.E,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = Input.Continue, Next = Level2State.F, },
                        }
                    },
                    new StateConfiguration
                    {
                        State = Level2State.F,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.GoBack, Next = Level2State.E, }, }
                    },
                },
            };

        public static readonly StateMachineConfiguration Level1Configuration =
            new StateMachineConfiguration
            {
                Start = Level1State.A,
                States = new[]
                {
                    new StateConfiguration 
                    {
                        State = Level1State.A,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.B, }, }
                    },
                    new StateConfiguration
                    {
                        State = Level1State.B,
                        SubState = Level2Configuration,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = Input.Continue, Next = Level1State.C, },
                            new TransitionConfiguration { Input = Input.GoBack, Next = Level1State.A, },
                        }
                    },
                    new StateConfiguration
                    {
                        State = Level1State.C,
                        Transitions = new[] { new TransitionConfiguration { Input = Input.GoBack, Next = Level1State.B, }, }
                    },
                },
            };

        // tests

        [Fact]
        public void Fire_WhenInputHasTransition_ReturnsTransitioned()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            StateMachineResponse actual = machine.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(new Enum[] { Level1State.B, Level2State.D, }, actual.State);
        }

        [Fact]
        public void Fire_WhenInputHasNoTransition_ReturnsNoAction()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            StateMachineResponse actual = machine.Fire(Input.GoBack);

            Assert.Equal(StateMachineResult.NoAction, actual.Result);
            Assert.Equal(new Enum[] { Level1State.A, }, actual.State);
        }

        [Fact]
        public void Fire_WhenSubStateAndInputHasTransition_ReturnsTransitionedWithinSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();
            machine.Fire(Input.Continue);

            StateMachineResponse actual = machine.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(new Enum[] { Level1State.B, Level2State.E, }, actual.State);
        }

        [Fact]
        public void Fire_WhenSubStateAndInputHasNoTransition_ReturnsTransitionedOutOfSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();
            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);

            StateMachineResponse actual = machine.Fire(Input.GoBack);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(new Enum[] { Level1State.A, }, actual.State);
        }

        [Fact]
        public void Fire_WhenReenteringSubState_ReturnsTransitionedAndStartOfSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();
            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);
            machine.Fire(Input.GoBack);

            StateMachineResponse actual = machine.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(new Enum[] { Level1State.B, Level2State.D, }, actual.State);
        }

    }

}