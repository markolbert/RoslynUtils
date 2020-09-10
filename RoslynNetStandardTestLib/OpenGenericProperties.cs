namespace J4JSoftware.Roslyn.Tests
{
    public class OpenGenericProperties<T1, T2>
        where T1 : struct
        where T2 : struct
    {
        public SimpleGeneric<T1, T2> GenericProperty { get; protected set; }

        public int this[ SimpleGeneric<T1, T2> key ]
        {
            get => -1;
        }
    }
}