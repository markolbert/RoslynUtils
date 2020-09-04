using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class MethodProcessors 
        : SymbolProcessors<IMethodSymbol> // TopologicallySortedCollection<IAtomicProcessor<IEnumerable<IMethodSymbol>>, MethodProcessor>, ISymbolSetProcessor<IMethodSymbol>
    {
        public MethodProcessors( 
            IEnumerable<IAtomicProcessor<IMethodSymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override bool SetPredecessors()
        {
            return SetPredecessor<ArgumentProcessor, MethodProcessor>();
        }

        //// ensure the context object is able to reset itself so it can 
        //// handle multiple iterations
        //public bool Process( IEnumerable<IMethodSymbol> context )
        //{
        //    var allOkay = true;

        //    foreach( var processor in ExecutionSequence )
        //    {
        //        allOkay &= processor.Process( context );
        //    }

        //    return allOkay;
        //}
    }
}