﻿using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FieldProcessor : SimpleProcessorDb<IFieldSymbol>
    {
        public FieldProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger)
            : base("adding Fields to the database", dataLayer, context, logger)
        {
        }

        protected override bool ProcessSymbol( IFieldSymbol symbol ) =>
            DataLayer.GetField( symbol, true, true ) != null;
    }
}
