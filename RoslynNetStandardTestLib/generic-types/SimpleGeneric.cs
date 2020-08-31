using System;

namespace J4JSoftware.Roslyn.Tests
{
    public class SimpleGeneric<T1, T2>
    {
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