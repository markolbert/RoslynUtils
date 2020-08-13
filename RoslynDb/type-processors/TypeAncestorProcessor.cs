using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAncestorProcessor : BaseProcessorDb<List<ITypeSymbol>>
    {
        public TypeAncestorProcessor(
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

            foreach( var ntSymbol in typeSymbols.Where( ts => ts.BaseType != null ) )
            {
                allOkay &= ProcessAncestors( ntSymbol );
            }

            return allOkay;
        }

        private bool ProcessAncestors(ITypeSymbol typeSymbol)
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( typeSymbol, out var typeDb ) )
                return false;

            if (!ProcessAncestor(typeDb!, typeSymbol.BaseType!))
                return false;

            var allOkay = true;

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                allOkay &= ProcessAncestor(typeDb!, interfaceSymbol);
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

        //private bool ProcessAncestorClosures( TypeDefinition typeDb, INamedTypeSymbol ancestorSymbol )
        //{
        //    var allOkay = true;

        //    var typeDefinitions = GetDbSet<TypeDefinition>();
        //    var typeClosures = GetDbSet<TypeClosure>();

        //    for( int idx = 0; idx < ancestorSymbol.TypeArguments.Length; idx++ )
        //    {
        //        if( ancestorSymbol.TypeArguments[ idx ] is ITypeParameterSymbol )
        //            continue;

        //        var symbolInfo = SymbolInfo.Create( ancestorSymbol.TypeArguments[ idx ] );

        //        if( !( symbolInfo.Symbol is INamedTypeSymbol ) && symbolInfo.TypeKind != TypeKind.Array )
        //        {
        //            Logger.Error<string>(
        //                "Closing type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
        //                symbolInfo.SymbolName );
        //            allOkay = false;

        //            continue;
        //        }

        //        var conDb = typeDefinitions
        //            .FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

        //        if( conDb == null )
        //        {
        //            Logger.Error<string>( "Closing type '{0}' not found in database", symbolInfo.SymbolName );
        //            allOkay = false;

        //            continue;
        //        }

        //        var closureDb = typeClosures
        //            .FirstOrDefault( c => c.TypeBeingClosedID == typeDb.ID 
        //                                  && c.ClosingTypeID == conDb.ID
        //                                  && c.Ordinal == idx );

        //        if( closureDb == null )
        //        {
        //            closureDb = new TypeClosure
        //            {
        //                ClosingType = conDb,
        //                TypeBeingClosed = typeDb!,
        //                Ordinal = idx
        //            };

        //            typeClosures.Add( closureDb );
        //        }

        //        closureDb.Synchronized = true;
        //    }

        //    return allOkay;
        //}
    }
}
