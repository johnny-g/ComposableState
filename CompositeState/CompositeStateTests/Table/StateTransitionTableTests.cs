using System;
using System.Collections.Generic;
using Xunit;

namespace CompositeState.Table
{

    public class StateTransitionTableTests
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
        public void Constructor_WhenNullStates_ThrowsArgumentNull()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>("states", () => new StateTransitionTable(null));

            Assert.Equal("Cannot instantiate StateTransitionTable with null states. (Parameter 'states')", actual.Message);
        }

        [Fact]
        public void Constructor_WhenNoStates_ThrowsArgument()
        {
            ArgumentException actual = Assert.Throws<ArgumentException>("states", () => new StateTransitionTable(new StateTuple[] { }));

            Assert.Equal("Cannot instantiate StateTransitionTable with '0' states. (Parameter 'states')", actual.Message);
        }

        [Fact]
        public void Constructor_WhenNoStartState_ThrowsArgument()
        {
            StateTuple[] states = new[] { new StateTuple { State = new Enum[] { Level1State.A, }, Transitions = new TransitionTuple[] { }, }, };

            ArgumentException actual = Assert.Throws<ArgumentException>("startState", () => new StateTransitionTable(states, new Enum[] { }));

            Assert.Equal("Cannot instantiate StateTransitionTable with startState ''. Specify a valid startState or specify null startState to default to first State. (Parameter 'startState')", actual.Message);
        }

        [Fact]
        public void Constructor_WhenInvalidStartState_ThrowsArgument()
        {
            StateTuple[] states = new[] { new StateTuple { State = new Enum[] { Level1State.A, }, Transitions = new TransitionTuple[] { }, }, };

            ArgumentException actual = Assert.Throws<ArgumentException>("startState", () => new StateTransitionTable(states, new Enum[] { Level1State.B, }));

            Assert.Equal("Cannot instantiate StateTransitionTable with startState 'B'. Cannot find startState 'B'. Specify a valid startState or specify null startState to default to first State. (Parameter 'startState')", actual.Message);
        }

        [Fact]
        public void Fire_WhenInputHasTransition_ReturnsTransitioned()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            StateTransitionTable table = configuration.ToStateTransitionTable();

            StateMachineResponse actual = table.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(State.Path(Level1State.B, Level2State.D), actual.State);
        }

        [Fact]
        public void Fire_WhenInputHasNoTransition_ReturnsNoAction()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            StateTransitionTable table = configuration.ToStateTransitionTable();

            StateMachineResponse actual = table.Fire(Input.GoBack);

            Assert.Equal(StateMachineResult.NoAction, actual.Result);
            Assert.Equal(State.Path(Level1State.A), actual.State);
        }

        [Fact]
        public void Fire_WhenSubStateAndInputHasTransition_ReturnsTransitionedWithinSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            StateTransitionTable table = configuration.ToStateTransitionTable();
            table.Fire(Input.Continue);

            StateMachineResponse actual = table.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(State.Path(Level1State.B, Level2State.E), actual.State);
        }

        [Fact]
        public void Fire_WhenSubStateAndInputHasNoTransition_ReturnsTransitionedOutOfSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            StateTransitionTable table = configuration.ToStateTransitionTable();
            table.Fire(Input.Continue);
            table.Fire(Input.Continue);

            StateMachineResponse actual = table.Fire(Input.GoBack);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(State.Path(Level1State.A), actual.State);
        }

        [Fact]
        public void Fire_WhenReenteringSubState_ReturnsTransitionedAndStartOfSubState()
        {
            StateMachineConfiguration configuration = Level1Configuration;
            StateTransitionTable table = configuration.ToStateTransitionTable();
            table.Fire(Input.Continue);
            table.Fire(Input.Continue);
            table.Fire(Input.GoBack);

            StateMachineResponse actual = table.Fire(Input.Continue);

            Assert.Equal(StateMachineResult.Transitioned, actual.Result);
            Assert.Equal(State.Path(Level1State.B, Level2State.D), actual.State);
        }

        [Fact]
        public void Fire_WhenStateHasOnEnter_DoesNotExecuteOnInstantiation()
        {
            List<string> log = new List<string>();
            StateMachineConfiguration configuration = GetStateMachineWithActions(aOnEnter: () => log.Add("aOnEnter"));

            StateTransitionTable table = configuration.ToStateTransitionTable();

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
            StateTransitionTable table = configuration.ToStateTransitionTable();

            table.Fire(Input.Continue);
            table.Fire(Input.Continue);
            table.Fire(Input.Continue);

            Assert.Collection(
                log,
                l => Assert.Equal("aOnEnter", l),
                l => Assert.Equal("dOnEnter", l),
                l => Assert.Equal("aOnEnter", l),
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
            StateTransitionTable table = configuration.ToStateTransitionTable();

            table.Fire(Input.Continue);
            table.Fire(Input.Skip);

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
            StateTransitionTable table = configuration.ToStateTransitionTable();

            table.Fire(Input.Continue);
            table.Fire(Input.Continue);
            table.Fire(Input.Continue);

            Assert.Collection(
                log,
                l => Assert.Equal("dOnExit", l),
                l => Assert.Equal("aOnExit", l),
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
            StateTransitionTable table = configuration.ToStateTransitionTable();

            table.Fire(Input.Continue);
            table.Fire(Input.Skip);

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
            StateTransitionTable table = configuration.ToStateTransitionTable();

            table.Fire(Input.Continue);
            table.Fire(Input.Continue);
            table.Fire(Input.Continue);

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
                            OnEnter = _ => dOnEnter(),
                            OnExit = _ => dOnExit(),
                            State = Level2State.D,
                            Transitions = new[]
                            {
                                new TransitionConfiguration
                                {
                                    Input = Input.Continue,
                                    Next = Level2State.E,
                                    OnTransition = (_, _) => continueDToEOnTransition(),
                                },
                            }
                        },
                        new StateConfiguration
                        {
                            OnEnter = _ => eOnEnter(),
                            OnExit = _ => eOnExit(),
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
                            OnEnter = _ => aOnEnter(),
                            OnExit = _ => aOnExit(),
                            State = Level1State.A,
                            SubState = level2,
                            Transitions = new[]
                            {
                                new TransitionConfiguration
                                {
                                    Input = Input.Continue,
                                    Next = Level1State.B,
                                    OnTransition = (_, _) => continueAToBOnTransition(),
                                },
                                new TransitionConfiguration
                                {
                                    Input = Input.Skip,
                                    Next = Level1State.B,
                                    OnTransition = (_, _) => skipAToBOnTransition(),
                                },
                            }
                        },
                        new StateConfiguration
                        {
                            OnEnter = _ => bOnEnter(),
                            OnExit = _ => bOnExit(),
                            State = Level1State.B,
                        },
                    },
                };

            return level1;
        }

    }

}