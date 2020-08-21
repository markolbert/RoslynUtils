using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public class EnumerableClass<T> : IEnumerable<EnumerableClass<T>>
    {
        public IEnumerator<EnumerableClass<T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}