using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamedTypeProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public NamedTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, logger )
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
            dbSymbol.Name = SymbolNamer.GetName(symbol);
            dbSymbol.AssemblyID = assemblyDb!.DocObjectID;
            dbSymbol.NamespaceId = nsDb!.DocObjectID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;
            ((ImplementableTypeDb) dbSymbol).DeclarationModifier = symbol.GetDeclarationModifier();

            return true;
        }
    }
}
