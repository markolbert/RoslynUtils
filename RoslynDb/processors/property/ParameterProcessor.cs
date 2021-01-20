using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParameterProcessor : BaseProcessorDb<IPropertySymbol, IParameterSymbol>
    {
        public ParameterProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Property Parameters to the database", dataLayer, context, logger)
        {
        }

        protected override List<IParameterSymbol> ExtractSymbols( IEnumerable<IPropertySymbol> inputData )
        {
            return inputData.SelectMany( p => p.Parameters ).ToList();
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol ) =>
            DataLayer.GetPropertyParameter( symbol, true, true ) != null;
    }
}
