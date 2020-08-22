using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessorDb<IAssemblySymbol, List<ITypeSymbol>>
    {
        public TypeAssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ExtractSymbol( object item, out IAssemblySymbol? result )
        {
            result = null;

            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                return false;
            }

            result = typeSymbol.ContainingAssembly;

            return result != null;
        }

        protected override bool ProcessSymbol( IAssemblySymbol symbol )
        {
            var assemblies = GetDbSet<Assembly>();

            if( !GetByFullyQualifiedName<Assembly>( symbol, out var dbSymbol ) )
            {
                dbSymbol = new Assembly
                {
                    FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( symbol ),
                    Name = SymbolInfo.GetName( symbol ),
                    DotNetVersion = symbol.Identity.Version
                };

                assemblies.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;

            return true;
        }
    }
}
