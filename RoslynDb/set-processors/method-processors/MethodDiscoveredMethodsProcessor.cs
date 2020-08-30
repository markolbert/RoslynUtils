using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodDiscoveredMethodsProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodDiscoveredMethodsProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolInfo, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            yield return methodSymbol;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol )
        {
            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if (!GetByFullyQualifiedName<FixedTypeDb>(symbol.ContainingType, out var dtDb))
                return false;

            if (!GetByFullyQualifiedName<FixedTypeDb>(symbol.ReturnType, out var rtDb))
                return false;

            // construct/update the method entity
            GetByFullyQualifiedName<Method>( symbol, out var methodDb, true );

            methodDb!.Name = SymbolInfo.GetName(symbol);
            methodDb.Kind = symbol.MethodKind;
            methodDb.ReturnTypeID = rtDb!.ID;
            methodDb.DefiningTypeID = dtDb!.ID;
            methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            methodDb.Accessibility = symbol.DeclaredAccessibility;
            methodDb.Synchronized = true;

            return true;
        }
    }
}
