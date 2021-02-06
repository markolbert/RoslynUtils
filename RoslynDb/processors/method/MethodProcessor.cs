using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : SimpleProcessorDb<IMethodSymbol>
    {
        public MethodProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger)
            : base("adding Methods to the database", dataLayer, context, logger)
        {
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol ) =>
            DataLayer.GetMethod( symbol, true, true ) != null;
    }
}
