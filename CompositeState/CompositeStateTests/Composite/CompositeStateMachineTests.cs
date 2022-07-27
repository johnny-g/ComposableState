using System;
using System.Collections.Generic;
using Xunit;

namespace CompositeState.Composite
{

    public class CompositeStateMachineTests
    {

        public enum Input { Continue, GoBack, Skip, }

        public enum Level1State { Start, A, B, C, }
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

        [Fact]
        public void Fire_WhenStateHasOnEnter_DoesNotExecuteOnInstantiation()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(aOnEnter: () => log.Add("aOnEnter"));

            IStateMachine machine = configuration.ToCompositeStateMachine();

            Assert.Empty(log);
        }

        [Fact]
        public void Fire_WhenStateHasOnEnter_DoesExecuteAfterTransition()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(
                aOnEnter: () => log.Add("aOnEnter"),
                bOnEnter: () => log.Add("bOnEnter"),
                dOnEnter: () => log.Add("dOnEnter"),
                eOnEnter: () => log.Add("eOnEnter"));
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);

            Assert.Collection(
                log,
                l => Assert.Equal("aOnEnter", l),
                l => Assert.Equal("dOnEnter", l),
                l => Assert.Equal("eOnEnter", l),
                l => Assert.Equal("bOnEnter", l));
        }

        [Fact]
        public void Fire_WhenStateHasOnEnterAndSkipSubState_DoesNotExecuteSubStateOnEnter()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(
                aOnEnter: () => log.Add("aOnEnter"),
                bOnEnter: () => log.Add("bOnEnter"),
                dOnEnter: () => log.Add("dOnEnter"),
                eOnEnter: () => log.Add("eOnEnter"));
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            machine.Fire(Input.Continue);
            machine.Fire(Input.Skip);

            Assert.Collection(
                log,
                l => Assert.Equal("aOnEnter", l),
                l => Assert.Equal("dOnEnter", l),
                l => Assert.Equal("bOnEnter", l));
        }

        [Fact]
        public void Fire_WhenStateHasOnExit_DoesExecuteBeforeTransition()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(
                aOnExit: () => log.Add("aOnExit"),
                bOnExit: () => log.Add("bOnExit"),
                dOnExit: () => log.Add("dOnExit"),
                eOnExit: () => log.Add("eOnExit"));
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);

            Assert.Collection(
                log,
                l => Assert.Equal("dOnExit", l),
                l => Assert.Equal("eOnExit", l),
                l => Assert.Equal("aOnExit", l));
        }

        [Fact]
        public void Fire_WhenStateHasOnExitAndSkipSubState_DoesNotExecuteSubStateOnExit()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(
                aOnExit: () => log.Add("aOnExit"),
                bOnExit: () => log.Add("bOnExit"),
                dOnExit: () => log.Add("dOnExit"),
                eOnExit: () => log.Add("eOnExit"));
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            machine.Fire(Input.Continue);
            machine.Fire(Input.Skip);

            Assert.Collection(
                log,
                l => Assert.Equal("dOnExit", l),
                l => Assert.Equal("aOnExit", l));
        }

        [Fact]
        public void Fire_WhenTransitionHasOnTransition_DoesExecuteOnTransition()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(
                continueAToBOnTransition: () => log.Add("continueAToBOnTransition"),
                skipAToBOnTransition: () => log.Add("skipAToBOnTransition"),
                continueDToEOnTransition: () => log.Add("continueDToEOnTransition"));
            CompositeStateMachine machine = configuration.ToCompositeStateMachine();

            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);
            machine.Fire(Input.Continue);

            Assert.Collection(
                log,
                l => Assert.Equal("continueDToEOnTransition", l),
                l => Assert.Equal("continueAToBOnTransition", l));
        }

        // private methods

        private static StateMachineConfiguration GetStateMachineWithActions(
            Action aOnEnter = null, Action aOnExit = null,
            Action bOnEnter = null, Action bOnExit = null,
            Action dOnEnter = null, Action dOnExit = null,
            Action eOnEnter = null, Action eOnExit = null,
            Action continueAToBOnTransition = null,
            Action skipAToBOnTransition = null,
            Action continueDToEOnTransition = null)
        {
            aOnEnter = aOnEnter ?? (() => { });
            aOnExit = aOnExit ?? (() => { });
            bOnEnter = bOnEnter ?? (() => { });
            bOnExit = bOnExit ?? (() => { });
            dOnEnter = dOnEnter ?? (() => { });
            dOnExit = dOnExit ?? (() => { });
            eOnEnter = eOnEnter ?? (() => { });
            eOnExit = eOnExit ?? (() => { });
            continueAToBOnTransition = continueAToBOnTransition ?? (() => { });
            skipAToBOnTransition = skipAToBOnTransition ?? (() => { });
            continueDToEOnTransition = continueDToEOnTransition ?? (() => { });

            StateMachineConfiguration level2 =
                new StateMachineConfiguration
                {
                    Start = Level2State.D,
                    States = new[]
                    {
                        new StateConfiguration
                        {
                            OnEnter = () => dOnEnter(),
                            OnExit = () => dOnExit(),
                            State = Level2State.D,
                            Transitions = new[]
                            {
                                new TransitionConfiguration
                                {
                                    Input = Input.Continue,
                                    Next = Level2State.E,
                                    OnTransition = () => continueDToEOnTransition(),
                                },
                            }
                        },
                        new StateConfiguration
                        {
                            OnEnter = () => eOnEnter(),
                            OnExit = () => eOnExit(),
                            State = Level2State.E,
                        },
                    },
                };

            StateMachineConfiguration level1 = 
                new StateMachineConfiguration
                {
                    Start = Level1State.Start,
                    States = new[]
                    {
                        new StateConfiguration
                        {
                            State = Level1State.Start,
                            Transitions = new[] { new TransitionConfiguration { Input = Input.Continue, Next = Level1State.A, }, }
                        },
                        new StateConfiguration
                        {
                            OnEnter = () => aOnEnter(),
                            OnExit = () => aOnExit(),
                            State = Level1State.A,
                            SubState = level2,
                            Transitions = new[]
                            {
                                new TransitionConfiguration
                                {
                                    Input = Input.Continue,
                                    Next = Level1State.B,
                                    OnTransition = () => continueAToBOnTransition(),
                                },
                                new TransitionConfiguration
                                {
                                    Input = Input.Skip,
                                    Next = Level1State.B,
                                    OnTransition = () => skipAToBOnTransition(),
                                },
                            }
                        },
                        new StateConfiguration
                        {
                            OnEnter = () => bOnEnter(),
                            OnExit = () => bOnExit(),
                            State = Level1State.B,
                        },
                    },
                };

            return level1;
        }

    }

}