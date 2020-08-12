using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessorDb<TypeProcessorContext>
    {
        public TypeAssemblyProcessor(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ProcessInternal( TypeProcessorContext context )
        {
            var assemblies = GetDbSet<Assembly>();

            foreach( var assemblySymbol in context.TypeSymbols.Select( ts => ts.ContainingAssembly ) )
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
