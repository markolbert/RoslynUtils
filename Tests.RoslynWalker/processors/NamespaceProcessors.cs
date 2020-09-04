using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class NamespaceProcessors 
        : SymbolProcessors<INamespaceSymbol>
    {
        public NamespaceProcessors( 
            IEnumerable<IAtomicProcessor<INamespaceSymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        // there's only one processor for namespaces
        protected override bool SetPredecessors() => true;
    }
}