using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class PropertyProcessors : RoslynDbProcessors<IPropertySymbol>
    {
        public PropertyProcessors( 
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( dataLayer, context, loggerFactory() )
        {
            var rootProcessor = new PropertyProcessor(dataLayer, context, loggerFactory());

            Add( rootProcessor );
            Add( new ParameterProcessor( dataLayer, context, loggerFactory() ), rootProcessor );
        }

        protected override bool Initialize( IEnumerable<IPropertySymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<PropertyDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<PropertyParameterDb>();

            return true;
        }
    }
}