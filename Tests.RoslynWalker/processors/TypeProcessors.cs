using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors : SymbolProcessors<ITypeSymbol> 
    {
        public TypeProcessors( 
            IEnumerable<IAtomicProcessor<ITypeSymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override bool SetPredecessors()
        {
            return SetPredecessor<TypeNamespaceProcessor, TypeAssemblyProcessor>()
                   && SetPredecessor<NonGenericTypeProcessor, TypeNamespaceProcessor>()
                   && SetPredecessor<NonParametricTypeProcessor, NonGenericTypeProcessor>()
                   && SetPredecessor<ParametricTypeProcessor, NonParametricTypeProcessor>()
                   && SetPredecessor<FinalNamedTypeProcessor, ParametricTypeProcessor>()
                   && SetPredecessor<ArrayTypeProcessor, FinalNamedTypeProcessor>()
                   && SetPredecessor<TypeParametricTypeProcessor, ArrayTypeProcessor>()
                   && SetPredecessor<AncestorProcessor, ArrayTypeProcessor>()
                   && SetPredecessor<TypeArgumentProcessor, AncestorProcessor>();
        }
    }
}