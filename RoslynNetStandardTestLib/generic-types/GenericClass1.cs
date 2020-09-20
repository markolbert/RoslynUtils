using System;
using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public class GenericClass1<T1, T2>
        where T1: IEnumerable<T1>
        where T2: new()
    {
        private T1 generic_field1;
        private T2 generic_field2;

        public T1 TOne { get; set; }
        public T2 TTwo { get; set; }
    }

    public class GenericClass1<T>
    {
        public T Property { get; set; }
        public T[] OneDimensionalArray { get; set; }
        public T[,] TwoDimensionalArray { get; set; }
    }

    public class DefinedGenericClass1 : GenericClass1<int>
    {
    }

    public class ComplexGeneric<T>
        where T : IEnumerable<T>
    {
        public TMethod SomeMethod<TMethod>( TMethod item )
            where TMethod : GenericClass1<T, int>
        {
            throw new NotImplementedException();
        }
    }
}