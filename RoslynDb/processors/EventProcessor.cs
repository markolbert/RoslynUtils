using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class EventProcessor : BaseProcessorDb<IEventSymbol, IEventSymbol>
    {
        public EventProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base(dataLayer, context, logger)
        {
        }

        protected override List<IEventSymbol> ExtractSymbols( IEnumerable<IEventSymbol> inputData )
        {
            return inputData.ToList();
        }

        protected override bool ProcessSymbol( IEventSymbol symbol ) =>
            DataLayer.GetEvent( symbol, true, true ) != null;
    }
}
