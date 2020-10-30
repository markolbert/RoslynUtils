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
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Method processing", dataLayer, context, loggerFactory() )
        {
            var rootProcessor = new MethodProcessor( dataLayer, context, loggerFactory() );

            AddValue( rootProcessor );
            AddDependency( new ArgumentProcessor( dataLayer, context, loggerFactory() ), rootProcessor );
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