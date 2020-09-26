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
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) 
            : base( dataLayer, context, loggerFactory() )
        {
            var node = Add(new TypeAssemblyProcessor(dataLayer, context, loggerFactory()));
            node = Add( new TypeInScopeAssemblyInfoProcessor( dataLayer, context, loggerFactory() ), node );
            node = Add(new TypeNamespaceProcessor(dataLayer, context, loggerFactory()), node );
            node = Add(new SortedTypeProcessor(dataLayer, context, loggerFactory()), node);
            var taNode = Add( new TypeArgumentProcessor( dataLayer, context, loggerFactory() ), node );
            var tptNode = Add(new TypeParametricTypeProcessor(dataLayer, context, loggerFactory()), node);
            var ancestorNode = Add( new AncestorProcessor( dataLayer, context, loggerFactory() ), node );
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