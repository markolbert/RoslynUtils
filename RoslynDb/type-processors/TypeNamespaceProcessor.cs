using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeAssemblyProcessor))]
    public class TypeNamespaceProcessor : BaseProcessorDb<List<ITypeSymbol>>
    {
        private readonly SymbolEqualityComparer<INamespaceSymbol> _comparer;

        public TypeNamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
            _comparer = new SymbolEqualityComparer<INamespaceSymbol>(SymbolInfo);
        }

        protected override bool ProcessInternal(List<ITypeSymbol> typeSymbols )
        {
            var allOkay = true;

            var namespaces = GetDbSet<Namespace>();

            foreach( var nsSymbol in typeSymbols.Select( ts => ts.ContainingNamespace )
                .Where( x => x != null )
                .Distinct( _comparer ) )
            {
                if( GetByFullyQualifiedName<Namespace>( nsSymbol, out var dbSymbol ) )
                {
                    dbSymbol!.Synchronized = true;
                    continue;
                }

                if( !GetByFullyQualifiedName<Assembly>( nsSymbol.ContainingAssembly, out var dbAssembly ) )
                    allOkay = false;
                else
                {
                    dbSymbol = new Namespace
                    {
                        FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( nsSymbol ),
                        Name = SymbolInfo.GetName( nsSymbol )
                    };

                    namespaces.Add( dbSymbol );
                }
            }

            return allOkay;
        }
    }
}
