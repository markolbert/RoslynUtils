using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessorDb<List<ITypeSymbol>>
    {
        private readonly SymbolEqualityComparer<IAssemblySymbol> _comparer;

        public TypeAssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
            _comparer = new SymbolEqualityComparer<IAssemblySymbol>( SymbolInfo );
        }

        protected override bool ProcessInternal( List<ITypeSymbol> typeSymbols )
        {
            var assemblies = GetDbSet<Assembly>();

            foreach( var assemblySymbol in typeSymbols.Select( ts => ts.ContainingAssembly )
                .Where( x => x != null )
                .Distinct( _comparer ) )
            {
                if( GetByFullyQualifiedName<Assembly>( assemblySymbol, out var dbSymbol ) )
                    continue;

                dbSymbol = new Assembly
                {
                    FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( assemblySymbol ),
                    Name = SymbolInfo.GetName( assemblySymbol ),
                    DotNetVersion = assemblySymbol.Identity.Version
                };

                assemblies.Add( dbSymbol );
            }

            return true;
        }
    }
}
