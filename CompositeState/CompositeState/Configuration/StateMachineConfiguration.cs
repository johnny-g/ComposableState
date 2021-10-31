using System;

namespace CompositeState
{

    public class StateMachineConfiguration
    {
        public Enum Start { get; set; }
        public StateConfiguration[] States { get; set; }
    }

}
