using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class EventProcessors : RoslynDbProcessors<IEventSymbol>
    {
        public EventProcessors( 
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Event processing", dataLayer, context, loggerFactory() )
        {
            AddIndependentNode( new EventProcessor( dataLayer, context, loggerFactory() ) );
        }

        protected override bool Initialize( IEnumerable<IEventSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<EventDb>();

            return true;
        }
    }
}