using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class MethodProcessors 
        : TopologicallySortedCollection<IAtomicProcessor<IEnumerable<IMethodSymbol>>, MethodParametricTypeProcessor>, ISymbolSetProcessor<IMethodSymbol>
    {
        public MethodProcessors( 
            IEnumerable<IAtomicProcessor<IEnumerable<IMethodSymbol>>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override void SetPredecessors()
        {
            SetPredecessor<MethodDiscoveredMethodsProcessor, MethodParametricTypeProcessor>();
            SetPredecessor<MethodArgumentProcessor, MethodDiscoveredMethodsProcessor>();
        }

        // ensure the context object is able to reset itself so it can 
        // handle multiple iterations
        public bool Process( IEnumerable<IMethodSymbol> context )
        {
            var allOkay = true;

            foreach( var processor in ExecutionSequence )
            {
                allOkay &= processor.Process( context );
            }

            return allOkay;
        }
    }
}