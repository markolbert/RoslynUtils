namespace J4JSoftware.Roslyn.Tests
{
    public class GenericMethodClass<T>
    {
        public bool MethodGenericClassType(T arg1) => true;

        public TOut? GenericMethod<TOut>( T arg1 )
            where TOut : class, T
        {
            if( arg1 == null )
                return null;

            return (TOut) arg1!;
        }
    }
}
