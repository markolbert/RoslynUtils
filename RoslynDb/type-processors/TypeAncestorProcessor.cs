using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities.types;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeGenericTypesProcessor))]
    public class TypeAncestorProcessor : BaseProcessor<INamedTypeSymbol, TypeProcessorContext>, ITypeProcessor
    {
        private readonly ISymbolName _symbolName;

        public TypeAncestorProcessor(
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

            foreach( var ntSymbol in context.TypeSymbols.Where( ts => ts.BaseType != null ) )
            {
                allOkay &= ProcessAncestors( ntSymbol );
            }

            return allOkay;
        }

        private bool ProcessAncestors(INamedTypeSymbol typeSymbol)
        {
            var fqName = _symbolName.GetFullyQualifiedName( typeSymbol );

            var typeDb = DbContext.TypeDefinitions.FirstOrDefault( td => td.FullyQualifiedName == fqName );

            if( typeDb == null )
            {
                Logger.Error<string>( "Couldn't find type {0} in the database", fqName );
                return false;
            }

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
            var fqName = _symbolName.GetFullyQualifiedName( ancestorSymbol );

            var implTypeDb = DbContext.TypeDefinitions.FirstOrDefault( td => td.FullyQualifiedName == fqName );

            if( implTypeDb == null )
            {
                Logger.Error<string>( "Couldn't find ancestor type {0} in database", fqName );
                return false;
            }

            var ancestorDb = DbContext.TypeAncestors
                .FirstOrDefault( ti => ti.ChildTypeID == typeDb!.ID && ti.ImplementingTypeID == implTypeDb!.ID );

            if( ancestorDb == null )
            {
                ancestorDb = new TypeAncestor
                {
                    ImplementingTypeID = implTypeDb!.ID,
                    ChildTypeID = typeDb!.ID
                };

                DbContext.TypeAncestors.Add( ancestorDb );
            }

            ancestorDb.Synchronized = true;

            return ProcessAncestorClosures( typeDb, ancestorSymbol );
        }

        private bool ProcessAncestorClosures( TypeDefinition typeDb, INamedTypeSymbol ancestorSymbol )
        {
            var allOkay = true;

            for( int idx = 0; idx < ancestorSymbol.TypeArguments.Length; idx++ )
            {
                if( ancestorSymbol.TypeArguments[ idx ] is ITypeParameterSymbol )
                    continue;

                var symbolInfo = new SymbolInfo( ancestorSymbol.TypeArguments[ idx ], _symbolName );

                if( !( symbolInfo.Symbol is INamedTypeSymbol ) && symbolInfo.TypeKind != TypeKind.Array )
                {
                    Logger.Error<string>(
                        "Closing type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
                        symbolInfo.SymbolName );
                    allOkay = false;

                    continue;
                }

                var conDb = DbContext.TypeDefinitions
                    .FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

                if( conDb == null )
                {
                    Logger.Error<string>( "Closing type '{0}' not found in database", symbolInfo.SymbolName );
                    allOkay = false;

                    continue;
                }

                var closureDb = DbContext.TypeClosures
                    .FirstOrDefault( c => c.TypeBeingClosedID == typeDb.ID && c.ClosingTypeID == conDb.ID );

                if( closureDb == null )
                {
                    closureDb = new TypeClosure
                    {
                        ClosingType = conDb,
                        TypeBeingClosed = typeDb!,
                        Ordinal = idx
                    };

                    DbContext.TypeClosures.Add( closureDb );
                }

                closureDb.Synchronized = true;
            }

            return allOkay;
        }

        public bool Equals( ITypeProcessor? other )
        {
            if (other == null)
                return false;

            return other.SupportedType == SupportedType;
        }
    }
}
