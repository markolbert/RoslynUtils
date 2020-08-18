using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeNamespaceProcessor))]
    public class TypeDiscoveredTypesProcessor : BaseProcessorDb<List<ITypeSymbol>>
    {
        public TypeDiscoveredTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ProcessInternal( List<ITypeSymbol> typeSymbols )
        {
            var allOkay = true;

            foreach( var ntSymbol in typeSymbols )
            {
                allOkay &= ProcessSymbol( ntSymbol );
            }

            return allOkay;
        }

        private bool ProcessSymbol( ITypeSymbol ntSymbol )
        {
            var symbolInfo = SymbolInfo.Create( ntSymbol );

            switch( symbolInfo.TypeKind )
            {
                case TypeKind.Error:
                    Logger.Error<string>( "Unhandled or incorrect type error for named type '{0}'",
                        symbolInfo.SymbolName );

                    return false;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        symbolInfo.SymbolName,
                        symbolInfo.TypeKind );

                    return false;
            }

            if( !GetByFullyQualifiedName<Assembly>( symbolInfo.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !GetByFullyQualifiedName<Namespace>( symbolInfo.ContainingNamespace, out var dbNS ) )
                return false;

            var typeDefinitions = GetDbSet<TypeDefinition>();

            var dbSymbol = typeDefinitions
                    .FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

            if( dbSymbol == null )
            {
                dbSymbol = new TypeDefinition
                {
                    FullyQualifiedName = symbolInfo.SymbolName
                };

                typeDefinitions.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName( symbolInfo.Symbol );
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
