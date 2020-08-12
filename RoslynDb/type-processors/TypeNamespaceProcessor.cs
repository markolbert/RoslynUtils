using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeAssemblyProcessor))]
    public class TypeNamespaceProcessor : BaseProcessorDb<TypeProcessorContext>
    {
        public TypeNamespaceProcessor(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ProcessInternal( TypeProcessorContext context )
        {
            var allOkay = true;

            var namespaces = GetDbSet<Namespace>();

            foreach ( var nsSymbol in context.TypeSymbols.Select( ts => ts.ContainingNamespace ) )
            {
                if (GetByFullyQualifiedName<Namespace>(nsSymbol, out var dbSymbol))
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
