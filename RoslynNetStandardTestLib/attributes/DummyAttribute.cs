using System;

namespace J4JSoftware.Roslyn.Tests
{
    [AttributeUsage( validOn: AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true )]
    public class DummyAttribute : Attribute
    {
#pragma warning disable 67
        public event EventHandler<int> Ralph;
#pragma warning restore 67
        
#pragma warning disable 8618
        public DummyAttribute( string arg1, Type arg2 )
#pragma warning restore 8618
        {
        }

        public int TestField;
    }
}
