using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class NamespaceProcessors : RoslynDbProcessors<INamespaceSymbol>
    {
        public NamespaceProcessors( 
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer, context, loggerFactory() )
        {
            Add( new NamespaceProcessor( dataLayer, context, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<INamespaceSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<NamespaceDb>();

            return true;
        }
    }
}