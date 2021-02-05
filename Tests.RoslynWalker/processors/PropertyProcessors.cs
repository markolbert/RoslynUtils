using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class PropertyProcessors : RoslynDbProcessors<IPropertySymbol>
    {
        public PropertyProcessors( 
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Property processing", dataLayer, context, loggerFactory() )
        {
            var rootProcessor = new PropertyProcessor(dataLayer, context, loggerFactory());

            AddIndependentNode( rootProcessor );
            AddDependentNode( new ParameterProcessor( dataLayer, context, loggerFactory() ), rootProcessor );
        }

        protected override bool Initialize( List<IPropertySymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<PropertyDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<PropertyParameterDb>();

            return true;
        }
    }
}