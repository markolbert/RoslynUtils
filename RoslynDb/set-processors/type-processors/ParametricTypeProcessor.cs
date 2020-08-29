using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeProcessor : TypeProcessor<ITypeParameterSymbol>
    {
        public ParametricTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( object item )
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

            // we handle ITypeParameterSymbols, which can either be the symbol itself
            // or the ElementType of the symbol if it's an IArrayTypeSymbol
            if( typeSymbol is ITypeParameterSymbol tpSymbol )
                yield return tpSymbol;

            if( typeSymbol is IArrayTypeSymbol arraySymbol &&
                arraySymbol.ElementType is ITypeParameterSymbol atpSymbol )
                yield return atpSymbol;
        }

        // symbol is guranteed to be an ITypeParameterSymbol
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol )
        {
            if( !ValidateAssembly( symbol, out var assemblyDb ) )
                return false;

            if( !ValidateNamespace( symbol, out var nsDb ) )
                return false;

            var dbSymbol = GetEntityFromTypeSymbol( symbol );

            if( dbSymbol == null )
                return false;

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName(symbol);
            dbSymbol.AssemblyID = assemblyDb!.ID;
            dbSymbol.NamespaceId = nsDb!.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;
            dbSymbol.Constraints = symbol.GetParametricTypeConstraint();

            return true;
        }

        private ParametricTypeDb? GetEntityFromTypeSymbol(ITypeParameterSymbol symbol )
        {
            var fqn = SymbolInfo.GetFullyQualifiedName(symbol);

            if ( symbol.DeclaringType == null )
            {
                Logger.Error<string>( "ITypeParameterSymbol '{0}' does not have a DeclaringType property", fqn );
                return null;
            }

            TypeDb? containingTypeDb = null;

            if (GetByFullyQualifiedName<GenericTypeDb>(symbol.DeclaringType, out var genericDb))
                containingTypeDb = genericDb!;
            else
            {
                if( GetByFullyQualifiedName<FixedTypeDb>( symbol.DeclaringType, out var fixedDb ) )
                    containingTypeDb = fixedDb!;
            }

            if( containingTypeDb == null )
            {
                Logger.Error<string>( "ITypeParameterSymbol.DeclaringType '{0}' not defined in the database",
                    SymbolInfo.GetFullyQualifiedName( symbol.DeclaringType ) );

                return null;
            }

            if (!GetByFullyQualifiedName<ParametricTypeDb>(symbol, out var parametricDb))
            {
                parametricDb = new ParametricTypeDb { FullyQualifiedName = fqn };

                if (containingTypeDb.ID == 0)
                    parametricDb.ContainingType = containingTypeDb;
                else parametricDb.ContainingTypeID = containingTypeDb.ID;

                var parametricTypes = GetDbSet<ParametricTypeDb>();
                parametricTypes.Add(parametricDb);
            }

            return parametricDb!;
        }
    }
}
