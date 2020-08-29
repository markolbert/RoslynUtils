using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors 
        : TopologicallySortedCollection<IAtomicProcessor<IEnumerable<ITypeSymbol>>, AssemblyProcessor>, ISymbolSetProcessor<ITypeSymbol>
    {
        public TypeProcessors( 
            IEnumerable<IAtomicProcessor<IEnumerable<ITypeSymbol>>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override void SetPredecessors()
        {
            SetPredecessor<NamespaceProcessor, AssemblyProcessor>();
            SetPredecessor<ImplementableTypeProcessor, NamespaceProcessor>();
            SetPredecessor<ParametricTypeProcessor, ImplementableTypeProcessor>();
            //SetPredecessor<TypeGenericTypesProcessor, TypeDiscoveredTypesProcessor>();
            //SetPredecessor<TypeAncestorProcessor, TypeGenericTypesProcessor>();
        }

        // ensure the context object is able to reset itself so it can 
        // handle multiple iterations
        public bool Process( IEnumerable<ITypeSymbol> context )
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