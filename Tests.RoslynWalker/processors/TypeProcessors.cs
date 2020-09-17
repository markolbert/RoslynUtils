using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors : RoslynDbProcessors<ITypeSymbol>
    {
        public TypeProcessors( 
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) 
            : base( factories, loggerFactory() )
        {
            var node = Add(new TypeAssemblyProcessor(factories, loggerFactory()));
            node = Add(new TypeNamespaceProcessor(factories, loggerFactory()), node );
            node = Add(new SortedTypeProcessor(factories, loggerFactory()), node);
            var taNode = Add( new TypeArgumentProcessor( factories, loggerFactory() ), node );
            var tptNode = Add(new TypeParametricTypeProcessor(factories, loggerFactory()), node);
            var ancestorNode = Add( new AncestorProcessor( factories, loggerFactory() ), node );
        }

        protected override bool Initialize( IEnumerable<ITypeSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<FixedTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<GenericTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<TypeParametricTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<MethodParametricTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<ParametricTypeDb>();
            EntityFactories.MarkUnsynchronized<TypeAncestorDb>();
            EntityFactories.MarkUnsynchronized<TypeArgumentDb>(true);

            return true;
        }

        //protected override bool SetPredecessors()
        //{
        //    return SetPredecessor<TypeNamespaceProcessor, TypeAssemblyProcessor>()
        //           && SetPredecessor<SortedTypeProcessor, TypeNamespaceProcessor>();
        //           //&& SetPredecessor<NonGenericTypeProcessor, TypeNamespaceProcessor>()
        //           //&& SetPredecessor<NonParametricTypeProcessor, NonGenericTypeProcessor>()
        //           //&& SetPredecessor<ParametricTypeProcessor, NonParametricTypeProcessor>()
        //           //&& SetPredecessor<FinalNamedTypeProcessor, ParametricTypeProcessor>()
        //           //&& SetPredecessor<ArrayTypeProcessor, FinalNamedTypeProcessor>()
        //           //&& SetPredecessor<TypeParametricTypeProcessor, ArrayTypeProcessor>()
        //           //&& SetPredecessor<AncestorProcessor, ArrayTypeProcessor>()
        //           //&& SetPredecessor<TypeArgumentProcessor, AncestorProcessor>();
        //}
    }
}