﻿namespace J4JSoftware.Roslyn.Tests
{
    public class GenericClass3<T1>
        where T1 : GenericClass1<EnumerableClass<int>, T1>, new()
    {
        public T1 TOne { get; set; }
    }
}