using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeNamespaceProcessor))]
    public class TypeDiscoveredTypesProcessor : BaseProcessor<INamedTypeSymbol, TypeProcessorContext>, ITypeProcessor
    {
        private readonly ISymbolName _symbolName;

        public TypeDiscoveredTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolName symbolName,
            IJ4JLogger logger
        )
            : base( dbContext, logger )
        {
            _symbolName = symbolName;
        }

        protected override bool ProcessInternal( TypeProcessorContext context )
        {
            var allOkay = true;

            foreach( var ntSymbol in context.TypeSymbols )
            {
                allOkay &= ProcessSymbol( context.SyntaxWalker, ntSymbol );
            }

            return allOkay;
        }

        private bool ProcessSymbol( ISyntaxWalker syntaxWalker, INamedTypeSymbol ntSymbol )
        {
            var symbolInfo = new SymbolInfo( ntSymbol, _symbolName );

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

            var fqAssemblyName = _symbolName.GetFullyQualifiedName( symbolInfo.Symbol.ContainingAssembly );
            var dbAssembly = DbContext.Assemblies.FirstOrDefault( a => a.FullyQualifiedName == fqAssemblyName );
            
            if( dbAssembly == null )
            {
                Logger.Error<string>( "Couldn't find assembly {0} in database", fqAssemblyName );
                return false;
            }

            var fqNSName = _symbolName.GetFullyQualifiedName( symbolInfo.Symbol.ContainingNamespace );
            var dbNS = DbContext.Namespaces.FirstOrDefault( ns => ns.FullyQualifiedName == fqNSName );

            if( dbNS == null )
            {
                Logger.Error<string>("Couldn't find namespace {0} in database", fqNSName);
                return false;
            }

            var dbSymbol = DbContext.TypeDefinitions
                    .FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

            if( dbSymbol == null )
            {
                dbSymbol = new TypeDefinition
                {
                    FullyQualifiedName = symbolInfo.SymbolName
                };

                DbContext.TypeDefinitions.Add( dbSymbol );
            }

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = _symbolName.GetName( symbolInfo.OriginalSymbol );
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbolInfo.Symbol.ContainingAssembly );

            return true;
        }

        public bool Equals( ITypeProcessor? other )
        {
            if (other == null)
                return false;

            return other.SupportedType == SupportedType;
        }
    }
}
