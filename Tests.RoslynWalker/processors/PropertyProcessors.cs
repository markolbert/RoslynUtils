using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class PropertyProcessors 
        : SymbolProcessors<IPropertySymbol> 
    {
        public PropertyProcessors( 
            IEnumerable<IAtomicProcessor<IPropertySymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override bool SetPredecessors()
        {
            return SetPredecessor<ParameterProcessor, PropertyProcessor>();
        }
    }
}