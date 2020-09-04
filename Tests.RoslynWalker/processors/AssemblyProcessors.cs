using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class AssemblyProcessors 
        : SymbolProcessors<IAssemblySymbol>
    {
        public AssemblyProcessors( 
            IEnumerable<IAtomicProcessor<IAssemblySymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        // there's only one processor for assemblies
        protected override bool SetPredecessors() => true;
    }
}