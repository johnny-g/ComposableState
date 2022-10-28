using System;

namespace CompositeState
{

    public static class State
    {
        public static Enum[] Path(params Enum[] stateSegment)
        {
            return stateSegment ?? Array.Empty<Enum>();
        }
    }

}