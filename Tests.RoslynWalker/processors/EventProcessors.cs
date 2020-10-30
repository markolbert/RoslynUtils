using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class EventProcessors : RoslynDbProcessors<IEventSymbol>
    {
        public EventProcessors( 
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory 
        ) : base( "Event processing", dataLayer, context, loggerFactory() )
        {
            AddValue( new EventProcessor( dataLayer, context, loggerFactory() ) );
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