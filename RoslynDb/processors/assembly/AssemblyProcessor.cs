using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : BaseProcessorDb<IAssemblySymbol, IAssemblySymbol>
    {
        public AssemblyProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base(dataLayer, context, logger)
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( IEnumerable<IAssemblySymbol> source )
        {
            return source.ToList();
        }

        protected override bool ProcessSymbol( IAssemblySymbol symbol ) =>
            DataLayer.GetAssembly( symbol, true, true ) != null;
    }
}
