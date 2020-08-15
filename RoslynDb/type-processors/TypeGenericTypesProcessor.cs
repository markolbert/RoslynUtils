using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeGenericTypesProcessor : BaseProcessorDb<List<ITypeSymbol>>
    {
        public TypeGenericTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ProcessInternal(List<ITypeSymbol> typeSymbols )
        {
            var allOkay = true;

            foreach( var symbol in typeSymbols
                .Where( ts => ts is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType )
                .Cast<INamedTypeSymbol>())
            {
                allOkay &= ProcessGeneric( symbol );
            }

            return allOkay;
        }

        private bool ProcessGeneric(INamedTypeSymbol generic)
        {
            if ( !generic.IsGenericType )
            {
                Logger.Error<string>("Type {0} is not generic", generic.Name);
                return false;
            }

            if( !GetByFullyQualifiedName<TypeDefinition>( generic, out var typeDefDb ) )
                return false;

            var allOkay = true;

            // first create/update all the TypeParameters related to this generic type
            foreach (var tpSymbol in generic.TypeParameters)
            {
                var tpDb = ProcessTypeParameter(typeDefDb!, tpSymbol);

                foreach (var conSymbol in tpSymbol.ConstraintTypes)
                {
                    allOkay &= ProcessTypeConstraints(tpDb, conSymbol);
                }
            }

            return allOkay;
        }

        private TypeParameter ProcessTypeParameter(TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol)
        {
            var typeParameters = GetDbSet<TypeParameter>();

            var tpDb = typeParameters
                .FirstOrDefault(x => x.Ordinal == tpSymbol.Ordinal && x.ContainingTypeID == typeDefDb.ID);

            if (tpDb == null)
            {
                tpDb = new TypeParameter
                {
                    ContainingTypeID = typeDefDb.ID,
                    Ordinal = tpSymbol.Ordinal
                };

                typeParameters.Add(tpDb);
            }

            tpDb.Synchronized = true;
            tpDb.Name = tpSymbol.Name;
            tpDb.Constraints = tpSymbol.GetTypeParameterConstraint();

            return tpDb;
        }

        private bool ProcessTypeConstraints(
            TypeParameter tpDb,
            ITypeSymbol constraintSymbol)
        {
            var symbolInfo = SymbolInfo.Create(constraintSymbol);

            if (!(symbolInfo.Symbol is INamedTypeSymbol) && symbolInfo.TypeKind != TypeKind.Array)
            {
                Logger.Error<string>(
                    "Constraining type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
                    symbolInfo.SymbolName);
                return false;
            }

            var typeDefinitions = GetDbSet<TypeDefinition>();

            var conDb = typeDefinitions
                .FirstOrDefault(td => td.FullyQualifiedName == symbolInfo.SymbolName);

            if (conDb == null)
            {
                Logger.Error<string>("Constraining type '{0}' not found in database", symbolInfo.SymbolName);
                return false;
            }

            var typeConstraints = GetDbSet<TypeConstraint>();

            var typeConstraintDb = typeConstraints
                .FirstOrDefault(c => c.TypeParameterID == tpDb.ID && c.ConstrainingTypeID == conDb.ID);

            if (typeConstraintDb == null)
            {
                typeConstraintDb = new TypeConstraint
                {
                    ConstrainingTypeID = conDb.ID,
                    TypeParameter = tpDb
                };

                typeConstraints.Add(typeConstraintDb);
            }

            typeConstraintDb.Synchronized = true;

            return true;
        }
    }
}
