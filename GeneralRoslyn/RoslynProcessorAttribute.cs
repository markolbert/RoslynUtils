using System;

namespace J4JSoftware.Roslyn
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false ) ]
    public class RoslynProcessorAttribute : Attribute
    {
        public RoslynProcessorAttribute( Type predecessorType )
        {
            if( predecessorType == null )
                throw new NullReferenceException( nameof(predecessorType) );

            PredecessorType = predecessorType;

            //if( typeof(ISyntaxWalker).IsAssignableFrom( predecessorType ) )
            //    PredecessorType = predecessorType;
            //else
            //    throw new InvalidCastException(
            //        $"Type {predecessorType.Name} is not assignable to {nameof(ISyntaxWalker)}" );
        }

        public Type PredecessorType { get; }
    }
}