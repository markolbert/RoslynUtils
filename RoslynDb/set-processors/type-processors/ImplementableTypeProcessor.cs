using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ImplementableTypeProcessor : TypeProcessor<ITypeSymbol>
    {
        public ImplementableTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<ITypeSymbol> ExtractSymbols( object item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol )
            {
                Logger.Error<string>( "Unhandled ITypeSymbol '{0}'", typeSymbol.Name );
                yield break;
            }

            if( typeSymbol is IErrorTypeSymbol )
            {
                Logger.Error( "ITypeSymbol is an IErrorTypeSymbol, ignored" );
                yield break;
            }

            // we handle INamedTypeSymbols
            if( typeSymbol is INamedTypeSymbol ntSymbol )
                yield return ntSymbol;

            // we handle IArrayTypeSymbols, provided they aren't based on an ITypeParameterSymbol
            if( typeSymbol is IArrayTypeSymbol arraySymbol && !(arraySymbol.ElementType is ITypeParameterSymbol)  )
                yield return arraySymbol;
        }

        // symbol is guranteed to be an INamedTypeSymbol or an IArrayTypeSymbol that doesn't reference 
        // any ITypeParameters in its definitional chain
        protected override bool ProcessSymbol( ITypeSymbol symbol )
        {
            // we consider arrays as belonging to the assembly and namespace containing
            // their element type
            ITypeSymbol contextSymbol = symbol;

            if( symbol is IArrayTypeSymbol tempSymbol )
                contextSymbol = tempSymbol.ElementType;

            if( !ValidateAssembly( contextSymbol, out var assemblyDb ) )
                return false;

            if( !ValidateNamespace( contextSymbol, out var nsDb ) )
                return false;

            var dbSymbol = symbol switch
            {
                INamedTypeSymbol ntSymbol => GetEntityFromTypeSymbol( ntSymbol ),
                IArrayTypeSymbol arraySymbol => GetEntityFromTypeSymbol( arraySymbol ),
                _ => null
            };

            if( dbSymbol == null )
            {
                Logger.Error<string, TypeKind>( "Unsupported ITypeSymbol '{0}' ({1})", symbol.Name, symbol.TypeKind );
                return false;
            }

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName(symbol);
            dbSymbol.AssemblyID = assemblyDb!.ID;
            dbSymbol.NamespaceId = nsDb!.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;

            switch( dbSymbol )
            {
                case GenericTypeDb genericDb:
                    genericDb.DeclarationModifier = symbol.GetDeclarationModifier();
                    break;

                case FixedTypeDb fixedDb:
                    fixedDb.DeclarationModifier = symbol.GetDeclarationModifier();
                    break;
            }

            return true;
        }

        private TypeDb? GetEntityFromTypeSymbol( INamedTypeSymbol symbol, string? fqn = null )
        {
            fqn ??= SymbolInfo.GetFullyQualifiedName( symbol );

            if( symbol.IsGenericType )
            {
                if( !GetByFullyQualifiedName<GenericTypeDb>( symbol, out var genericDb ) )
                {
                    genericDb = new GenericTypeDb { FullyQualifiedName = fqn };
                    var genericTypes = GetDbSet<GenericTypeDb>();
                    genericTypes.Add( genericDb );
                }

                return genericDb!;
            }
            else
            {
                if( !GetByFullyQualifiedName<FixedTypeDb>( symbol, out var fixedDb ) )
                {
                    fixedDb = new FixedTypeDb { FullyQualifiedName = fqn };
                    var fixedTypes = GetDbSet<FixedTypeDb>();
                    fixedTypes.Add( fixedDb );
                }

                return fixedDb!;
            }
        }

        private TypeDb? GetEntityFromTypeSymbol( IArrayTypeSymbol symbol, string? fqn = null )
        {
            fqn ??= SymbolInfo.GetFullyQualifiedName(symbol);

            switch ( symbol.ElementType )
            {
                case INamedTypeSymbol ntSymbol:
                    return GetEntityFromTypeSymbol( ntSymbol, fqn );

                case IArrayTypeSymbol arraySymbol:
                    return GetEntityFromTypeSymbol( arraySymbol, fqn );

                default:
                    Logger.Error<string, TypeKind>( "Unsupported array element type '{0}' ({1})", 
                        symbol.Name,
                        symbol.TypeKind );

                    return null;
            }
        }
    }
}
