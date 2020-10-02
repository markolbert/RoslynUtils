using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessorDb<ITypeSymbol, IAssemblySymbol>
    {
        public TypeAssemblyProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base("adding Type Assemblies to the database", dataLayer, context, logger)
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( IEnumerable<ITypeSymbol> inputData )
        {
            var retVal = new List<IAssemblySymbol>();

            foreach( var symbol in inputData )
            {
                var assemblySymbol = symbol is IArrayTypeSymbol arraySymbol
                    ? arraySymbol.ElementType.ContainingAssembly
                    : symbol.ContainingAssembly;

                if( assemblySymbol == null )
                    Logger.Information<string>( "ITypeSymbol '{0}' does not have a ContainingAssembly", symbol.Name );
                else retVal.Add( assemblySymbol );
            }

            return retVal;
        }

        protected override bool ProcessSymbol(IAssemblySymbol symbol) =>
            DataLayer.GetAssembly(symbol, true, true) != null;
    }
}
