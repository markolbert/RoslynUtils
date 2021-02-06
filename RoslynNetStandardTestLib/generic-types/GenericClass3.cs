#pragma warning disable 169
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    public class GenericClass3<T1>
        where T1 : GenericClass1<EnumerableClass<int>, T1>, new()
    {
        private T1 generic_field;

        public T1 TOne { get; set; }
    }
}
