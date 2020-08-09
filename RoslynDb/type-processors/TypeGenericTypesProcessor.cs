using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities.types;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    [RoslynProcessor(typeof(TypeDiscoveredTypesProcessor))]
    public class TypeGenericTypesProcessor : BaseProcessor<INamedTypeSymbol, TypeProcessorContext>, ITypeProcessor
    {
        private readonly ISymbolName _symbolName;

        public TypeGenericTypesProcessor(
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

            foreach( var ntSymbol in context.TypeSymbols.Where( ts => ts.IsGenericType ) )
            {
                allOkay &= ProcessGeneric( ntSymbol );
            }

            return allOkay;
        }

        private bool ProcessGeneric(INamedTypeSymbol generic)
        {
            var fqName = _symbolName.GetFullyQualifiedName(generic);

            if ( !generic.IsGenericType )
            {
                Logger.Error<string>("Type {0} is not generic", fqName);
                return false;
            }

            var typeDefDb = DbContext.TypeDefinitions
                .FirstOrDefault( td => td.FullyQualifiedName == fqName );

            if( typeDefDb == null )
            {
                Logger.Error<string>( "Couldn't find generic type {0} in the database", fqName );
                return false;
            }

            var allOkay = true;

            // first create/update all the TypeParameters related to this generic type
            foreach (var tpSymbol in generic.TypeParameters)
            {
                var tpDb = ProcessTypeParameter(typeDefDb!, tpSymbol);

                foreach (var conSymbol in tpSymbol.ConstraintTypes)
                {
                    allOkay &= ProcessTypeConstraints(typeDefDb!, tpSymbol, conSymbol);
                }
            }

            return allOkay;
        }

        private TypeParameter ProcessTypeParameter(TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol)
        {
            var tpDb = DbContext.TypeParameters
                .FirstOrDefault(x => x.Ordinal == tpSymbol.Ordinal && x.ContainingTypeID == typeDefDb.ID);

            if (tpDb == null)
            {
                tpDb = new TypeParameter
                {
                    ContainingTypeID = typeDefDb.ID,
                    Ordinal = tpSymbol.Ordinal
                };

                DbContext.TypeParameters.Add(tpDb);
            }

            tpDb.Synchronized = true;
            tpDb.Name = tpSymbol.Name;
            tpDb.Constraints = tpSymbol.GetTypeParameterConstraint();

            return tpDb;
        }

        private bool ProcessTypeConstraints(
            TypeDefinition typeDefDb,
            ITypeParameterSymbol tpSymbol,
            ITypeSymbol conSymbol)
        {
            var symbolInfo = new SymbolInfo(conSymbol, _symbolName);

            if (!(symbolInfo.Symbol is INamedTypeSymbol) && symbolInfo.TypeKind != TypeKind.Array)
            {
                Logger.Error<string>(
                    "Constraining type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
                    symbolInfo.SymbolName);
                return false;
            }

            var conDb = DbContext.TypeDefinitions
                .FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

            if (conDb == null)
            {
                Logger.Error<string>("Constraining type '{0}' not found in database", symbolInfo.SymbolName);
                return false;
            }

            var closureDb = DbContext.TypeClosures
                .FirstOrDefault( c => c.TypeBeingClosedID == typeDefDb.ID && c.ClosingTypeID == conDb.ID );

            if (closureDb == null)
            {
                closureDb = new TypeClosure
                {
                    ClosingType = conDb,
                    TypeBeingClosed = typeDefDb!,
                    Ordinal = tpSymbol.Ordinal
                };

                DbContext.TypeClosures.Add(closureDb);
            }

            closureDb.Synchronized = true;

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
