using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class EventProcessor : SimpleProcessorDb<IEventSymbol>
    {
        public EventProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Events to the database", dataLayer, context, logger)
        {
        }

        protected override bool ProcessSymbol( IEventSymbol symbol ) =>
            DataLayer.GetEvent( symbol, true, true ) != null;
    }
}
