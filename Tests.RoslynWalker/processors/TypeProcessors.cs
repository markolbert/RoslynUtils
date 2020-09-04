using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors 
        : SymbolProcessors<ITypeSymbol> // TopologicallySortedCollection<IAtomicProcessor<IEnumerable<ITypeSymbol>>, TypeAssemblyProcessor>, ISymbolSetProcessor<ITypeSymbol>
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
                && SetPredecessor<NamedTypeProcessor, TypeNamespaceProcessor>()
                && SetPredecessor<ParametricTypeProcessor, NamedTypeProcessor>()
                && SetPredecessor<ArrayTypeProcessor, ParametricTypeProcessor>()
                && SetPredecessor<AncestorProcessor, ArrayTypeProcessor>()
                && SetPredecessor<TypeArgumentProcessor, AncestorProcessor>();
        }

        //// ensure the context object is able to reset itself so it can 
        //// handle multiple iterations
        //public bool Process( IEnumerable<ITypeSymbol> context )
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