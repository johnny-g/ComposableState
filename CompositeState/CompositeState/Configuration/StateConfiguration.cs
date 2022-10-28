using System;
using System.Linq.Expressions;

namespace CompositeState
{

    /// <summary>
    /// An action to be performed when Transitioning to a State. Parameter is a
    /// StatePath identifying State we are Transitioning toward.
    /// </summary>
    /// <param name="to">A StatePath identifying which State we are entering.</param>
    public delegate void OnEnterDelegate(Enum[] to);

    /// <summary>
    /// An action to be performed when Transitioning from a State. Parameter is 
    /// a StatePath identifying State we are Transitioning from.
    /// </summary>
    /// <param name="from">A StatePath identifying which State we are leaving.</param>
    public delegate void OnExitDelegate(Enum[] from);

    /// <summary>
    /// Configuration of a State. A State contains Transitions away from this 
    /// State to another sibling State. A State may also contain a SubState.
    /// </summary>
    public class StateConfiguration
    {
        /// <summary>
        /// An action to be performed when Transitioning to a State. Parameter 
        /// is a StatePath identifying State we are Transitioning toward.
        /// </summary>
        public Expression<OnEnterDelegate> OnEnter { get; set; }
        /// <summary>
        /// An action to be performed when Transitioning from a State. Parameter
        /// is a StatePath identifying State we are Transitioning from.
        /// </summary>
        public Expression<OnExitDelegate> OnExit { get; set; }
        /// <summary>
        /// An enum uniquely identifying this State.
        /// </summary>
        public Enum State { get; set; }
        /// <summary>
        /// An optional SubState configuration. SubStates consume Transition 
        /// Inputs before current State.
        /// </summary>
        public StateMachineConfiguration SubState { get; set; }
        /// <summary>
        /// A collection of recognized Transitions from this State to other 
        /// defined States.
        /// </summary>
        public TransitionConfiguration[] Transitions { get; set; }
    }

}