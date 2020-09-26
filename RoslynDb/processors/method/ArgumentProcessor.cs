using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArgumentProcessor : BaseProcessorDb<IMethodSymbol, IParameterSymbol>
    {
        public ArgumentProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base(dataLayer, context, logger)
        {
        }

        protected override List<IParameterSymbol> ExtractSymbols( IEnumerable<IMethodSymbol> inputData )
        {
            return inputData.SelectMany( m => m.Parameters ).ToList();
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol ) =>
            DataLayer.GetArgument( symbol, true, true ) != null;

    }
}
