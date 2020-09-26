using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParameterProcessor : BaseProcessorDb<IPropertySymbol, IParameterSymbol>
    {
        public ParameterProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base(dataLayer, context, logger)
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
