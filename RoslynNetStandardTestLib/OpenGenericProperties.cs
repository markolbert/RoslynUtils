namespace J4JSoftware.Roslyn.Tests
{
    public class OpenGenericProperties<T1, T2>
    {
        public SimpleGeneric<T1, T2> GenericProperty { get; protected set; }

        public int this[SimpleGeneric<T1, T2> key]
        {
            get => -1;
        }
    }
}