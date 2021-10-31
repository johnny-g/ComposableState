using System;
using System.Linq.Expressions;

namespace CompositeState
{

    public class TransitionConfiguration
    {
        public Enum Input { get; set; }
        public Enum Next { get; set; }
        public Expression<Action> OnExit { get; set; }
    }

}