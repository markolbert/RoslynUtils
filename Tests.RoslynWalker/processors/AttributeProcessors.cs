using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class AttributeProcessors : RoslynDbProcessors<ISymbol>
    {
        public AttributeProcessors( 
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Attribute processing", dataLayer, context, loggerFactory() )
        {
            AddIndependentNode( new AttributeProcessor( dataLayer, context, loggerFactory() ) );
        }

        //protected override bool Initialize( IEnumerable<ISymbol> symbols )
        //{
        //    if( !base.Initialize( symbols ) )
        //        return false;

        //    DataLayer.MarkSharpObjectUnsynchronized<EventDb>();

        //    return true;
        //}
    }
}