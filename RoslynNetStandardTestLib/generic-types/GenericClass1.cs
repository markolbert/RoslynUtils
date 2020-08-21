using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public class GenericClass1<T1, T2>
        where T1: IEnumerable<T1>
        where T2: new()
    {
        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }
}