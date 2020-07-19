using System;

namespace J4JSoftware.Roslyn
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false ) ]
    public class PredecessorWalkerAttribute : Attribute
    {
        public PredecessorWalkerAttribute( Type walkerType )
        {
            if( walkerType == null )
                throw new NullReferenceException( nameof(walkerType) );

            if( typeof(ISyntaxWalker).IsAssignableFrom( walkerType ) )
                WalkerType = walkerType;
            else
                throw new InvalidCastException(
                    $"Type {walkerType.Name} is not assignable to {nameof(ISyntaxWalker)}" );
        }

        public Type WalkerType { get; }
    }
}