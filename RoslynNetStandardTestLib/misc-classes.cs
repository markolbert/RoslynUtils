using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Roslyn.Tests
{
    public class LinqClass
    {
        private readonly List<string> _text = new List<string>();

        public List<string> GetFiltered( string text ) => _text
            .Where( t => t.IndexOf( text, StringComparison.OrdinalIgnoreCase ) >= 0 )
            .ToList();
    }

    public class GenericClass1<T1, T2>
        where T1: IEnumerable<T1>
        where T2: new()
    {
        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }

    public class GenericClass2<T1, T2>
        where T1 : GenericClass1<T1,T2>, IEnumerable<T1>
        where T2 : new()
    {
        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }

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

    public class GenericClass3<T1>
        where T1 : GenericClass1<EnumerableClass<int>, T1>, new()
    {
        public T1 TOne { get; set; }
    }
}
