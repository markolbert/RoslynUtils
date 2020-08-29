using System;

namespace J4JSoftware.Roslyn.Deprecated
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false ) ]
    public class RoslynProcessorAttribute : Attribute
    {
        public RoslynProcessorAttribute( Type predecessorType )
        {
            if( predecessorType == null )
                throw new NullReferenceException( nameof(predecessorType) );

            PredecessorType = predecessorType;
        }

        public Type PredecessorType { get; }
    }
}