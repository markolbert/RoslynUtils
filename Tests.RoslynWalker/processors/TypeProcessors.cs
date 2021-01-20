using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors : RoslynDbProcessors<ITypeSymbol>
    {
        public TypeProcessors( 
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            Func<IJ4JLogger> loggerFactory 
        ) 
            : base( "Type processing", dataLayer, context, loggerFactory() )
        {
            var node = AddIndependentNode(new TypeAssemblyProcessor(dataLayer, context, loggerFactory()));
            node = AddDependentNode( new TypeInScopeAssemblyInfoProcessor( dataLayer, context, loggerFactory() ), node.Value );
            node = AddDependentNode(new TypeNamespaceProcessor(dataLayer, context, loggerFactory()), node.Value );
            node = AddDependentNode(new SortedTypeProcessor(dataLayer, context, loggerFactory()), node.Value);
            AddDependentNode( new TypeArgumentProcessor( dataLayer, context, loggerFactory() ), node.Value );
            AddDependentNode(new TypeParametricTypeProcessor(dataLayer, context, loggerFactory()), node.Value);
            AddDependentNode( new AncestorProcessor( dataLayer, context, loggerFactory() ), node.Value );
        }

        protected override bool Initialize( IEnumerable<ITypeSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<FixedTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<GenericTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<ParametricTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<ParametricMethodTypeDb>( false );
            DataLayer.MarkUnsynchronized<TypeAncestorDb>( false );
            DataLayer.MarkUnsynchronized<TypeArgumentDb>();

            return true;
        }
    }
}