using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeNamespaceProcessor))]
    public class TypeDiscoveredTypesProcessor : BaseProcessorDb<ITypeSymbol, ITypeSymbol>
    {
        public TypeDiscoveredTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<ITypeSymbol> ExtractSymbols( object item )
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            switch (typeSymbol.TypeKind)
            {
                case TypeKind.Error:
                    Logger.Error<string>("Unhandled or incorrect type error for named type '{0}'",
                        typeSymbol.Name);

                    yield break;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        typeSymbol.Name,
                        typeSymbol.TypeKind);

                    yield break;
            }

            yield return typeSymbol;
        }

        protected override bool ProcessSymbol( ITypeSymbol symbol )
        {
            var symbolInfo = SymbolInfo.Create(symbol);

            if (!GetByFullyQualifiedName<Assembly>(symbolInfo.ContainingAssembly, out var dbAssembly))
                return false;

            if (!GetByFullyQualifiedName<Namespace>(symbolInfo.ContainingNamespace, out var dbNS))
                return false;

            if (!GetByFullyQualifiedName<TypeDefinition>(symbolInfo.Symbol, out var dbSymbol))
            {
                dbSymbol = new TypeDefinition
                {
                    FullyQualifiedName = symbolInfo.SymbolName
                };

                var typeDefinitions = GetDbSet<TypeDefinition>();
                typeDefinitions.Add(dbSymbol);
            }

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName(symbolInfo.Symbol);
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.Symbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.Symbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = dbAssembly.InScopeInfo != null;

            return true;
        }
    }
}
