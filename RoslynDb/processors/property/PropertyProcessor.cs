using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : SimpleProcessorDb<IPropertySymbol>
    {
        public PropertyProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger)
            : base("adding Properties to the database", dataLayer, context, logger)
        {
        }

        protected override bool ProcessSymbol( IPropertySymbol symbol ) =>
            DataLayer.GetProperty( symbol, true, true ) != null;
    }
}
