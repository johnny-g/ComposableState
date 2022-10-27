using System;
using System.Collections.Generic;
using Xunit;

namespace CompositeState
{

    public class StatePathEqualityComparerTests
    {

        public enum Test
        {
            A,
            B,
            C,
        }

        public static readonly Enum[] StatePathTestA = State.Path(Test.A);

        public static IEnumerable<object[]> Equals_WhenDifferent_Data =
            new[]
            {
                new object[] { StatePathTestA, State.Path(Test.B), },
                new object[] { StatePathTestA, null, },
                new object[] { null, StatePathTestA, },
            };

        public static IEnumerable<object[]> Equals_WhenIdentical_Data =
            new[]
            {
                new object[] { StatePathTestA, StatePathTestA, },
                new object[] { StatePathTestA, State.Path(Test.A), },
                new object[] { null, null, },
            };

        public static IEnumerable<object[]> GetHashCode_WhenIdentical_Data = Equals_WhenIdentical_Data;

        [Theory]
        [MemberData(nameof(Equals_WhenIdentical_Data))]
        public void Equals_WhenIdentical_ReturnsTrue(Enum[] statePathA, Enum[] statePathB)
        {
            StatePathEqualityComparer comparer = new StatePathEqualityComparer();
            Assert.True(comparer.Equals(statePathA, statePathB));
        }

        [Theory]
        [MemberData(nameof(Equals_WhenDifferent_Data))]
        public void Equals_WhenDifferent_ReturnsFalse(Enum[] statePathA, Enum[] statePathB)
        {
            StatePathEqualityComparer comparer = new StatePathEqualityComparer();
            Assert.False(comparer.Equals(statePathA, statePathB));
        }

        [Theory]
        [MemberData(nameof(GetHashCode_WhenIdentical_Data))]
        public void GetHashCode_WhenIdentical_ReturnsSameValue(Enum[] statePathA, Enum[] statePathB)
        {
            StatePathEqualityComparer comparer = new StatePathEqualityComparer();
            Assert.Equal(comparer.GetHashCode(statePathA), comparer.GetHashCode(statePathB));
        }

        [Fact]
        public void GetHashCode_WhenDifferent_ReturnsDifferentValue()
        {
            StatePathEqualityComparer comparer = new StatePathEqualityComparer();
            Assert.NotEqual(comparer.GetHashCode(State.Path(Test.A)), comparer.GetHashCode(State.Path(Test.B)));
        }

    }

}