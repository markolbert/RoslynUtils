using System;

namespace J4JSoftware.Roslyn.Tests
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DummyTypeAttribute : Attribute
    {
        public DummyTypeAttribute( Type dummyType )
        {
            DummyType = dummyType;
        }

        public Type DummyType { get; }
    }
}