using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class FieldProcessors : RoslynDbProcessors<IFieldSymbol>
    {
        public FieldProcessors( 
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Field processing", dataLayer, context, loggerFactory() )
        {
            AddIndependentNode( new FieldProcessor( dataLayer, context, loggerFactory() ) );
        }

        protected override bool Initialize( List<IFieldSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<FieldDb>();

            return true;
        }
    }
}