using System;
using System.Collections.Generic;
#pragma warning disable 169
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class SimpleGeneric<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        private Dictionary<T1, T2> generic_field1;
        private Dictionary<T2, T1> generic_field2;

        public T1 PropertyT1 { get; set; }
        public T2 PropertyT2 { get; set; }
        public T1[] ArrayPropertyT1 { get; set; }
        public T2[] ArrayPropertyT2 { get; set; }

        public TMethod[] GetArrayPropertyT3<TMethod>()
            where TMethod : new()
        {
            return Array.Empty<TMethod>();
        }
    }
}