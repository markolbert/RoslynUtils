using System;

namespace RoslynNetStandardTestLib
{
    // added to test SharpDoc
    [AttributeUsage( validOn: AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true )]
    public class DummyAttribute : Attribute
    {
#pragma warning disable 67
        // ReSharper disable once EventNeverSubscribedTo.Global
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
