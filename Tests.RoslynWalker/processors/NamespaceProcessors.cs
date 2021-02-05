using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class NamespaceProcessors : RoslynDbProcessors<INamespaceSymbol>
    {
        public NamespaceProcessors( 
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Namespace processing", dataLayer, context, loggerFactory() )
        {
            AddIndependentNode( new NamespaceProcessor( dataLayer, context, loggerFactory() ) );
        }

        protected override bool Initialize( List<INamespaceSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<NamespaceDb>();

            return true;
        }
    }
}