using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class FieldProcessors  : SymbolProcessors<IFieldSymbol> 
    {
        public FieldProcessors( 
            IEnumerable<IAtomicProcessor<IFieldSymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        // fields only have a single processor
        protected override bool SetPredecessors() => true;
    }
}