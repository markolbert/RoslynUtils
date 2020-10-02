﻿using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base("adding Methods to the database", dataLayer, context, logger)
        {
        }

        protected override List<IMethodSymbol> ExtractSymbols( IEnumerable<IMethodSymbol> inputData )
        {
            return inputData.ToList();
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol ) =>
            DataLayer.GetMethod( symbol, true, true ) != null;
    }
}
