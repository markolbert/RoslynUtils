using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Tests
{
    public interface IDummyInterface3<in T>
        where T : DummyAttribute
    {
        string GetValue( T item );

        bool TestGenericMethod<TMethod>()
            where TMethod : class, IDummyInterface1;
    }
}