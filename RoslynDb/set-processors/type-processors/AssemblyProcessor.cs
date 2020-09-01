using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : BaseProcessorDb<ITypeSymbol, IAssemblySymbol>
    {
        public AssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( object item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if( typeSymbol.ContainingAssembly == null )
            {
                Logger.Information<string>("ITypeSymbol '{0}' does not have a ContainingAssembly", typeSymbol.Name);
                yield break;
            }

            yield return typeSymbol.ContainingAssembly!;
        }

        protected override bool ProcessSymbol( IAssemblySymbol symbol )
        {
            var assemblies = GetDbSet<AssemblyDb>();

            if( !GetByFullyQualifiedName<AssemblyDb>( symbol, out var dbSymbol ) )
            {
                dbSymbol = new AssemblyDb
                {
                    FullyQualifiedName = SymbolNamer.GetFullyQualifiedName( symbol ),
                    Name = SymbolNamer.GetName( symbol ),
                    DotNetVersion = symbol.Identity.Version
                };

                assemblies.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;

            return true;
        }
    }
}
