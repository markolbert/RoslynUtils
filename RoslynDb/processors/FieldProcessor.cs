using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldProcessor : BaseProcessorDb<IFieldSymbol, IFieldSymbol>
    {
        public FieldProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base("adding Fields to the database", dataLayer, context, logger)
        {
        }

        protected override List<IFieldSymbol> ExtractSymbols( IEnumerable<IFieldSymbol> inputData )
        {
            return inputData.ToList();
        }

        protected override bool ProcessSymbol( IFieldSymbol symbol ) =>
            DataLayer.GetField( symbol, true, true ) != null;
    }
}
