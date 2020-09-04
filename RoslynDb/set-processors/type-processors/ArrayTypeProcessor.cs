using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArrayTypeProcessor : BaseProcessorDb<ITypeSymbol, IArrayTypeSymbol>
    {
        public ArrayTypeProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, symbolNamer, docObjMapper, logger )
        {
        }

        protected override IEnumerable<IArrayTypeSymbol> ExtractSymbols(object item)
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

            // we handle IArrayTypeSymbols, provided they aren't based on an ITypeParameterSymbol
            if (typeSymbol is IArrayTypeSymbol arraySymbol )
                yield return arraySymbol;
        }

        protected override bool ProcessSymbol( IArrayTypeSymbol symbol )
        {
            //// we consider arrays as belonging to the assembly and namespace containing
            //// their element type
            if( !GetByFullyQualifiedName<AssemblyDb>( symbol.ElementType, out var assemblyDb ) )
                return false;

            if( !GetByFullyQualifiedName<NamespaceDb>( symbol.ElementType, out var nsDb ) )
                return false;

            // arrays can be based on parametric types...which have to containers
            object? containerDb = null;

            if( symbol.ElementType is ITypeParameterSymbol tpElementSymbol )
            {
                containerDb = GetParametricTypeContainer( tpElementSymbol );

                if( containerDb == null )
                    return false;
            }

            var dbSymbol = GetTypeByFullyQualifiedName( symbol, true );

            if( dbSymbol == null )
            {
                Logger.Error<string, TypeKind>( "Unsupported ITypeSymbol '{0}' ({1})", symbol.Name, symbol.TypeKind );
                return false;
            }

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolNamer.GetName( symbol );
            dbSymbol.AssemblyID = assemblyDb!.DocObjectID;
            dbSymbol.NamespaceId = nsDb!.DocObjectID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.Nature = symbol.TypeKind;
            dbSymbol.InDocumentationScope = assemblyDb.InScopeInfo != null;

            // arrays can also be based on ParametricTypes, which don't have declaration modifiers
            if( dbSymbol is ImplementableTypeDb impDb )
                impDb.DeclarationModifier = symbol.GetDeclarationModifier();

            // if the array is based on a parametric type, specify its container
            if( symbol.ElementType is ITypeParameterSymbol )
            {
                switch( containerDb )
                {
                    case ImplementableTypeDb implTypeDb:
                        var parametricTypeDb = (ParametricTypeDb) dbSymbol;

                        if( implTypeDb.DocObjectID == 0 )
                            parametricTypeDb.ContainingType = implTypeDb;
                        else parametricTypeDb.ContainingTypeID = implTypeDb.DocObjectID;

                        break;

                    case MethodPlaceholderDb mpDb:
                        var methodParametricTypeDb = (MethodParametricTypeDb) dbSymbol;

                        if( mpDb.DocObjectID == 0 )
                            methodParametricTypeDb.ContainingMethod = mpDb;
                        else methodParametricTypeDb.ContainingMethodID = mpDb.DocObjectID;

                        break;

                    default:
                        Logger.Error<string>( "Unsupported parametric type container for symbol '{0}'",
                            SymbolNamer.GetFullyQualifiedName( symbol ) );

                        return false;
                }
            }

            return true;
        }
    }
}
