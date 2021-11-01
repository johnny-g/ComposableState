using System;
using System.Collections.Generic;

namespace CompositeState
{

    /// <summary>
    /// Result enumeration. When "fired", a StateMachine may either take no 
    /// action (ie no valid transition exists), or may transition (ie a valid
    /// transition exists).
    /// </summary>
    public enum StateMachineResult
    {
        /// <summary>
        /// No valid transition from current state was found.
        /// </summary>
        NoAction = 0,

        /// <summary>
        /// A valid transition from current state was found.
        /// </summary>
        Transitioned = 1,
    }

    /// <summary>
    /// A response class. Will return a StateMachine's response to a fire event
    /// and its current state.
    /// </summary>
    public class StateMachineResponse
    {
        /// <summary>
        /// A value indicating StateMachine's result from a fire event.
        /// </summary>
        public StateMachineResult Result { get; set; }

        /// <summary>
        /// Current StateMachine state.
        /// </summary>
        public Enum[] State { get; set; }
    }

    /// <summary>
    /// A StateMachine.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Current state. A collection of sub-states that uniquely identifies
        /// current discrete state. A breadcrumb trail into a nested machine.
        /// </summary>
        IEnumerable<Enum> State { get; }

        /// <summary>
        /// A fire event. Request a response to <paramref name="input"/>.
        /// </summary>
        /// <param name="input">An input.</param>
        /// <returns>A response indicating action and current state.</returns>
        StateMachineResponse Fire(Enum input);
    }

}