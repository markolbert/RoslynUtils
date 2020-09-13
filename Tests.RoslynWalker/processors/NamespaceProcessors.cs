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
            EntityFactories factories,
            Func<IJ4JLogger> loggerFactory 
        ) : base( factories, loggerFactory() )
        {
            Add( new NamespaceProcessor( factories, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<INamespaceSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<NamespaceDb>(true);

            return true;
        }

        //// there's only one processor for namespaces
        //protected override bool SetPredecessors() => true;
    }
}