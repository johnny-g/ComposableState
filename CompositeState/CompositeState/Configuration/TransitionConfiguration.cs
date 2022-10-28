using System;
using System.Linq.Expressions;

namespace CompositeState
{

    /// <summary>
    /// An action to be performed when Transitioning from one State to another.
    /// Parameters are StatePaths identifying State we are Transitioning from 
    /// and State we are Transitioning toward.
    /// </summary>
    /// <param name="from">A StatePath identifying which State we are leaving.</param>
    /// <param name="to">A StatePath identifying which State we are entering.</param>
    public delegate void OnTransitionDelegate(Enum[] from, Enum[] to);

    /// <summary>
    /// Configuration for a Transition from a State.
    /// </summary>
    public class TransitionConfiguration
    {
        /// <summary>
        /// An input that initiates Transition from containing State to Next
        /// State.
        /// </summary>
        public Enum Input { get; set; }
        /// <summary>
        /// Next State that containing State should Transition toward when
        /// StateMachine is given Input.
        /// </summary>
        public Enum Next { get; set; }
        /// <summary>
        /// An action to be performed when Transitioning from containing State
        /// to Next State.
        /// </summary>
        public Expression<OnTransitionDelegate> OnTransition { get; set; }
    }

}