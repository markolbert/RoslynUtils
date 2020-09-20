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
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer, loggerFactory() )
        {
            Add( new NamespaceProcessor( dataLayer, loggerFactory() ) );
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