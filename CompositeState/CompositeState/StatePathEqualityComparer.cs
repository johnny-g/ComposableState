using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositeState
{

    /// <summary>
    /// Equality Comparer for Enum array, ie StatePath. Implements equality and
    /// hashcode.
    /// </summary>
    public class StatePathEqualityComparer : IEqualityComparer<Enum[]>
    {

        /// <summary>
        /// Returns true if <paramref name="x"/> and <paramref name="y"/> are 
        /// same object instance or are same sequence of Enum. False otherwise.
        /// </summary>
        /// <param name="x">An array of Enum.</param>
        /// <param name="y">An array of Enum.</param>
        /// <returns>Returns true if <paramref name="x"/> and <paramref name="y"/> are same object instance or are same sequence of Enum. False otherwise.</returns>
        public bool Equals(Enum[] x, Enum[] y)
        {
            bool isEqual = ReferenceEquals(x, y);
            if (!isEqual && x != null && y != null) { isEqual = x.SequenceEqual(y); }
            return isEqual;
        }

        /// <summary>
        /// Gets a hashcode value unique to sequence of Enum.
        /// </summary>
        /// <param name="obj">An array of Enum.</param>
        /// <see cref="https://stackoverflow.com/a/263416/189183"/>
        /// <returns>A unique hashcode for <paramref name="obj"/>.</returns>
        public int GetHashCode(Enum[] obj)
        {
            int hash = 17;
            if (obj != null) { foreach (Enum e in obj) { hash = hash * 23 + (e?.GetHashCode() ?? 0); } }
            return hash;
        }

    }

}