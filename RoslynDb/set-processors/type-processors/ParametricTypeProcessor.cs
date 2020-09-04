using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeParameterSymbol>
    {
        public ParametricTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, docObjMapper, logger )
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

            if( typeSymbol is IArrayTypeSymbol arraySymbol 
                && arraySymbol.ElementType is ITypeParameterSymbol atpSymbol )
                yield return atpSymbol;
        }

        // symbol is guaranteed to be an ITypeParameterSymbol 
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol )
        {
            if( !GetByFullyQualifiedName<AssemblyDb>( symbol, out var assemblyDb ) )
                return false;

            if( !GetByFullyQualifiedName<NamespaceDb>( symbol, out var nsDb ) )
                return false;

            // Finding the container for the parametric type is complicated by the
            // fact parametric types can be contained by either a type or a method...
            // and we haven't yet processed any method symbols into the database
            // at this point. So we create a MethodPlaceholderDb object for such
            // method containers, and replace them when we actually process the
            // methods.
            object? containerDb = GetParametricTypeContainer( symbol );

            // this error should only occur if we're contained by a type
            // which somehow hasn't been processed into the database
            if( containerDb == null )
                return false;

            var dbSymbol = (ParametricTypeBaseDb?) GetTypeByFullyQualifiedName( symbol, true );

            if( dbSymbol == null )
                return false;

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolNamer.GetName( symbol );
            dbSymbol.AssemblyID = assemblyDb!.DocObjectID;
            dbSymbol.NamespaceId = nsDb!.DocObjectID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;
            dbSymbol.Constraints = symbol.GetParametricTypeConstraint();

            switch( containerDb )
            {
                case ImplementableTypeDb implTypeDb:
                    var parametricTypeDb = (ParametricTypeDb)dbSymbol;

                    if (implTypeDb.DocObjectID == 0)
                        parametricTypeDb.ContainingType = implTypeDb;
                    else parametricTypeDb.ContainingTypeID = implTypeDb.DocObjectID;

                    break;

                case MethodPlaceholderDb mpDb:
                    var methodParametricTypeDb = (MethodParametricTypeDb) dbSymbol;

                    if (mpDb.DocObjectID == 0)
                        methodParametricTypeDb.ContainingMethod = mpDb;
                    else methodParametricTypeDb.ContainingMethodID = mpDb.DocObjectID;

                    break;

                default:
                    Logger.Error<string>( "Unsupported parametric type container for symbol '{0}'",
                        SymbolNamer.GetFullyQualifiedName( symbol ) );

                    return false;
            }

            return true;
        }
    }
}
