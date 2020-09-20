using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class MethodProcessors : RoslynDbProcessors<IMethodSymbol>
    {
        public MethodProcessors( 
            IRoslynDataLayer dataLayer,
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer, loggerFactory() )
        {
            var rootProcessor = new MethodProcessor( dataLayer, loggerFactory() );

            Add( rootProcessor );
            Add( new ArgumentProcessor( dataLayer, loggerFactory() ), rootProcessor );
        }

        protected override bool Initialize( IEnumerable<IMethodSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<MethodDb>(false);
            DataLayer.MarkSharpObjectUnsynchronized<ParametricMethodTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<ArgumentDb>();

            return true;
        }
    }
}