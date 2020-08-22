using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeGenericTypesProcessor : BaseProcessorDb<INamedTypeSymbol, List<ITypeSymbol>>
    {
        public TypeGenericTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override bool ExtractSymbol( object item, out INamedTypeSymbol? result )
        {
            if( item is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType )
                result = ntSymbol;
            else result = null;

            return result != null;
        }

        protected override bool ProcessSymbol(INamedTypeSymbol generic)
        {
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

            // create/update all TypeArguments related to this generic type
            for( var taOrdinal = 0; taOrdinal < generic.TypeArguments.Length; taOrdinal++)
            {
                ProcessTypeArgument( typeDefDb!, generic.TypeArguments[taOrdinal], taOrdinal );
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

            if( !GetByFullyQualifiedName<TypeDefinition>( constraintSymbol, out var conDb ) )
                return false;

            var typeConstraints = GetDbSet<TypeConstraint>();

            var typeConstraintDb = typeConstraints
                .FirstOrDefault(c => c.TypeParameterBaseID == tpDb.ID && c.ConstrainingTypeID == conDb!.ID);

            if (typeConstraintDb == null)
            {
                typeConstraintDb = new TypeConstraint
                {
                    ConstrainingTypeID = conDb!.ID,
                    TypeParameterBase = tpDb
                };

                typeConstraints.Add(typeConstraintDb);
            }

            typeConstraintDb.Synchronized = true;

            return true;
        }

        private void ProcessTypeArgument(TypeDefinition typeDefDb, ITypeSymbol taSymbol, int taOrdinal )
        {
            // don't process anything other than INamedTypeSymbols or IArrayTypeSymbols
            if( !( taSymbol is INamedTypeSymbol ) && !( taSymbol is IArrayTypeSymbol ) )
                return;

            var typeArguments = GetDbSet<TypeArgument>();

            var taDb = typeArguments
                .FirstOrDefault(x => x.Ordinal == taOrdinal && x.TypeDefinitionID == typeDefDb.ID);

            if (taDb == null)
            {
                taDb = new TypeArgument
                {
                    TypeDefinition = typeDefDb,
                    Ordinal = taOrdinal
                };

                typeArguments.Add(taDb);
            }

            taDb.Synchronized = true;
            taDb.Name = taSymbol.Name;
        }
    }
}
