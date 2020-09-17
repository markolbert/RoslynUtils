using System;

namespace J4JSoftware.Roslyn
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SharpObjectTypeAttribute : Attribute
    {
        public SharpObjectTypeAttribute( SharpObjectType soType )
        {
            if( soType == SharpObjectType.Unknown )
                throw new ArgumentException(
                    $"Cannot declare an ISharpObject entity as a {nameof(SharpObjectType.Unknown)}" );

            SharpObjectType = soType;
        }

        public SharpObjectType SharpObjectType { get; }
    }
}