using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : BaseProcessorDb<IAssemblySymbol, IAssemblySymbol>
    {
        public AssemblyProcessor( 
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger ) 
            : base( dataLayer, logger )
        {
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is IAssemblySymbol assemblySymbol ))
            {
                Logger.Error("Supplied item is not an IAssemblySymbol");
                yield break;
            }

            yield return assemblySymbol!;
        }

        protected override bool ProcessSymbol( IAssemblySymbol symbol ) =>
            DataLayer.GetAssembly( symbol, true ) != null;

    }
}
