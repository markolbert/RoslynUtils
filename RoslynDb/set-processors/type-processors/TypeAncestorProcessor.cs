using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAncestorProcessor : BaseProcessorDb<ITypeSymbol, List<ITypeSymbol>>
    {
        public TypeAncestorProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<ITypeSymbol> ExtractSymbols( object item )
        {
            if( item is ITypeSymbol typeSymbol )
                yield return typeSymbol;
        }

        protected override bool ProcessSymbol( ITypeSymbol typeSymbol )
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( typeSymbol, out var typeDb ) )
                return false;

            // typeSymbol must be System.Object, which has no base type
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

        private bool ProcessAncestor( TypeDefinition typeDb, INamedTypeSymbol ancestorSymbol )
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( ancestorSymbol, out var implTypeDb ) )
                return false;

            var typeAncestors = GetDbSet<TypeAncestor>();

            var ancestorDb = typeAncestors
                .FirstOrDefault( ti => ti.ChildTypeID == typeDb!.ID && ti.AncestorTypeID == implTypeDb!.ID );

            if( ancestorDb == null )
            {
                ancestorDb = new TypeAncestor
                {
                    AncestorTypeID = implTypeDb!.ID,
                    ChildTypeID = typeDb!.ID
                };

                typeAncestors.Add( ancestorDb );
            }

            ancestorDb.Synchronized = true;

            return true;
        }
    }
}
