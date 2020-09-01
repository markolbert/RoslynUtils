using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class MethodParametricTypeProcessor : BaseProcessorDb<IMethodSymbol, ITypeParameterSymbol>
    {
        public MethodParametricTypeProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            // we only need to process parametric types which are defined
            // within the context of a declaring method (ones defined within the
            // context of a type have already been handled by the typeprocessors)
            foreach( var typeParameter in methodSymbol.TypeParameters
                .Where(pt=>pt.DeclaringMethod != null  ))
            {
                yield return typeParameter;
            }
        }

        protected override bool ProcessSymbol(ITypeParameterSymbol symbol)
        {
            if (!ValidateAssembly(symbol, out var assemblyDb))
                return false;

            if (!ValidateNamespace(symbol, out var nsDb))
                return false;

            var dbSymbol = (ParametricTypeDb?)GetTypeByFullyQualifiedName(symbol, true);

            if (dbSymbol == null)
                return false;

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolNamer.GetName(symbol);
            dbSymbol.AssemblyID = assemblyDb!.ID;
            dbSymbol.NamespaceId = nsDb!.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;
            dbSymbol.Constraints = symbol.GetParametricTypeConstraint();

            return true;
        }
    }
}
