using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AncestorProcessor : BaseProcessorDb<ITypeSymbol, ITypeSymbol>
    {
        public AncestorProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<ITypeSymbol> ExtractSymbols( object item )
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

            yield return typeSymbol;
        }

        protected override bool ProcessSymbol( ITypeSymbol typeSymbol )
        {
            var typeDb = GetTypeByFullyQualifiedName( typeSymbol );

            if( typeDb == null )
            {
                Logger.Error<string, TypeKind>( "Couldn't find ITypeSymbol '{0}' in database ({1})",
                    SymbolNamer.GetFullyQualifiedName( typeSymbol ), 
                    typeSymbol.TypeKind );

                return false;
            }

            // if typeSymbol is System.Object, which has no base type, we're done
            if( typeSymbol.BaseType == null )
                return true;

            if( !ProcessAncestor( typeDb!, typeSymbol.BaseType ) )
                return false;

            var allOkay = true;

            foreach( var interfaceSymbol in typeSymbol.Interfaces )
            {
                allOkay &= ProcessAncestor( typeDb!, interfaceSymbol );
            }

            return allOkay;
        }

        private bool ProcessAncestor( TypeDb typeDb, INamedTypeSymbol ancestorSymbol )
        {
            var ancestorDb = GetTypeByFullyQualifiedName( ancestorSymbol );

            if( ancestorDb == null )
            {
                Logger.Error<string>( "Couldn't find ancestor type '{0}' in the database",
                    SymbolNamer.GetFullyQualifiedName( ancestorSymbol ) );

                return false;
            }

            var typeAncestors = GetDbSet<TypeAncestorDb>();

            var typeAncestorDb = typeAncestors
                .FirstOrDefault( ti => ti.ChildTypeID == typeDb!.ID && ti.AncestorTypeID == ancestorDb!.ID );

            if( typeAncestorDb == null )
            {
                typeAncestorDb = new TypeAncestorDb
                {
                    AncestorTypeID = ancestorDb!.ID,
                    ChildTypeID = typeDb!.ID
                };

                typeAncestors.Add( typeAncestorDb );
            }

            typeAncestorDb.Synchronized = true;

            return true;
        }
    }
}
