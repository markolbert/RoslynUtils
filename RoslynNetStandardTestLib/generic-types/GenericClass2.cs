using System.Collections.Generic;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class GenericClass2<T1, T2>
        where T1 : GenericClass1<T1,T2>, IEnumerable<T1>
        where T2 : new()
    {
        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }
}