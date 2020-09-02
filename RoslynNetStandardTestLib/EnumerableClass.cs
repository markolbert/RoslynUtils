using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public class EnumerableClass<T> : IEnumerable<EnumerableClass<T>>
    {
        private readonly List<T> _items = new List<T>();

        public T this[ int idx ]
        {
            get => _items[ idx ];
            set => _items[ idx ] = value;
        }

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