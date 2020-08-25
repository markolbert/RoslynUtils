using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class MethodProcessors 
        : TopologicallySortedCollection<IAtomicProcessor<List<IMethodSymbol>>, MethodTypeParameterProcessor>, ISymbolSetProcessor<IMethodSymbol>
    {
        public MethodProcessors( 
            IEnumerable<IAtomicProcessor<List<IMethodSymbol>>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override void SetPredecessors()
        {
            SetPredecessor<MethodDiscoveredMethodsProcessor, MethodTypeParameterProcessor>();
            SetPredecessor<MethodArgumentProcessor, MethodDiscoveredMethodsProcessor>();
        }

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