using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArgumentProcessor : BaseProcessorDb<IMethodSymbol, IParameterSymbol>
    {
        public ArgumentProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Method Arguments to the database", dataLayer, context, logger)
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
