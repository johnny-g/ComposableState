using System;

namespace CompositeState.Configuration
{

    public class StateMachineConfiguration
    {
        public Enum Start { get; set; }
        public StateConfiguration[] States { get; set; }
    }

}
