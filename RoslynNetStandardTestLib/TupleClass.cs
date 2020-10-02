using System;

namespace J4JSoftware.Roslyn.Tests
{
    public class TupleClass<T>
    {
        public (int intVal, string stringVal) GetNamedTuple()
        {
            throw new NotImplementedException();
        }

        public (int, string ) GetUnnamedTuple()
        {
            throw new NotImplementedException();
        }

        public void UseNamedTuple( (int intVal, string stringVal) tupleArg )
        {
            throw new NotImplementedException();

        }

        public void UseUnnamedTuple( (int, string) tupleArg )
        {
            throw new NotImplementedException();
        }

        public (T tVal, int intVal) GetNamedTypeGenericTuple()
        {
            throw new NotImplementedException();
        }

        public (X xVal, int intVal) GetNamedMethodGenericTuple<X>()
        {
            throw new NotImplementedException();
        }
    }
}