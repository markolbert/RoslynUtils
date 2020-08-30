using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamedTypeProcessor : TypeProcessor<INamedTypeSymbol>
    {
        public NamedTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols(object item)
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if (typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol)
            {
                Logger.Error<string>("Unhandled ITypeSymbol '{0}'", typeSymbol.Name);
                yield break;
            }

            if (typeSymbol is IErrorTypeSymbol)
            {
                Logger.Error("ITypeSymbol is an IErrorTypeSymbol, ignored");
                yield break;
            }

            // we handle INamedTypeSymbols
            if (typeSymbol is INamedTypeSymbol ntSymbol)
                yield return ntSymbol;
        }

        protected override bool ProcessSymbol(INamedTypeSymbol symbol)
        {
            //// we consider arrays as belonging to the assembly and namespace containing
            //// their element type
            //ITypeSymbol contextSymbol = symbol;

            //if (symbol is IArrayTypeSymbol tempSymbol)
            //    contextSymbol = tempSymbol.ElementType;

            if (!ValidateAssembly(symbol, out var assemblyDb))
                return false;

            if (!ValidateNamespace(symbol, out var nsDb))
                return false;

            var dbSymbol = GetTypeByFullyQualifiedName(symbol, true);

            if (dbSymbol == null)
            {
                Logger.Error<string, TypeKind>("Unsupported ITypeSymbol '{0}' ({1})", symbol.Name, symbol.TypeKind);
                return false;
            }

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName(symbol);
            dbSymbol.AssemblyID = assemblyDb!.ID;
            dbSymbol.NamespaceId = nsDb!.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;
            ((ImplementableTypeDb) dbSymbol).DeclarationModifier = symbol.GetDeclarationModifier();

            return true;
        }
    }
}
