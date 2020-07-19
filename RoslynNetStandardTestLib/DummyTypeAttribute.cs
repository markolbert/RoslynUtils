using System;

namespace RoslynNetStandardTestLib
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