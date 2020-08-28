using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class TypeGenericTypesProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public TypeGenericTypesProcessor(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols( object item )
        {
            if( item is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType )
                yield return ntSymbol;
        }

        protected override bool ProcessSymbol(INamedTypeSymbol generic)
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( generic, out var typeDefDb ) )
                return false;

            var allOkay = true;

            // first create/update all the TypeParameters related to this generic type
            foreach (var tpSymbol in generic.TypeParameters)
            {
                allOkay &= ProcessTypeParameter(typeDefDb!, tpSymbol);
            }

            // create/update all TypeArguments related to this generic type
            for( var taOrdinal = 0; taOrdinal < generic.TypeArguments.Length; taOrdinal++)
            {
                ProcessTypeArgument( typeDefDb!, generic.TypeArguments[taOrdinal], taOrdinal );
            }

            return allOkay;
        }

        private bool ProcessTypeParameter(TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol)
        {
            var typeParameters = GetDbSet<TypeParameter>();

            var constraints = tpSymbol.GetTypeParameterConstraint();

            var fqnTypeConstraints = tpSymbol.ConstraintTypes
                .Select( ct => SymbolInfo.GetFullyQualifiedName( ct ) )
                .ToList();

            // see if the TypeParameter entity is already in the database
            // match on Constraints and identical TypeConstraints. Do this in two phases,
            // first retrieving TypeParameters based solely on the Constraints property
            var possibleDb = typeParameters
                .Include( x => x.TypeConstraints )
                .ThenInclude( x => x.ConstrainingType )
                .Where( x => x.Constraints == constraints && x.Name == tpSymbol.Name )
                .ToList();

            var tpDb = possibleDb.Count switch
            {
                0 => null,
                _ => possibleDb
                    .FirstOrDefault( x =>
                        !x.TypeConstraints
                            .Select( tc => tc.ConstrainingType.FullyQualifiedName )
                            .Except( fqnTypeConstraints ).Any()
                        && x.TypeConstraints.Count == fqnTypeConstraints.Count
                    )
            };

            var allOkay = true;

            if (tpDb == null)
            {
                tpDb = new TypeParameter();
                typeParameters.Add(tpDb);

                // create the constraint entities
                foreach( var tcSymbol in tpSymbol.ConstraintTypes )
                {
                    allOkay &= ProcessTypeConstraints( tpDb, tcSymbol );
                }
            }

            tpDb.Synchronized = true;
            tpDb.Name = tpSymbol.Name;
            tpDb.Constraints = constraints;

            // see if the TypeDefinitionTypeParameter entity exists, and
            // create it if it doesn't
            ProcessTypeParameterUsage( tpSymbol, typeDefDb, tpDb );

            SaveChanges();

            return allOkay;
        }

        private void ProcessTypeParameterUsage( ITypeParameterSymbol tpSymbol, TypeDefinition typeDefDb, TypeParameter tpDb )
        {
            var tdTypeParameters = GetDbSet<TypeDefinitionTypeParameter>();

            var tdtpDb = tdTypeParameters
                .FirstOrDefault(x => x.TypeParameterID == tpDb.ID && x.ReferencingTypeID == typeDefDb.ID);

            if( tdtpDb == null )
            {
                tdtpDb = new TypeDefinitionTypeParameter();

                if( typeDefDb.ID == 0 )
                    tdtpDb.ReferencingType = typeDefDb;
                else tdtpDb.ReferencingTypeID = typeDefDb.ID;

                if( tpDb.ID == 0 )
                    tdtpDb.TypeParameter = tpDb;
                else tdtpDb.TypeParameterID = tpDb.ID;

                tdTypeParameters.Add( tdtpDb );
            }

            tdtpDb.Name = tpSymbol.Name;
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
                .FirstOrDefault(c => c.TypeParameterID == tpDb.ID && c.ConstrainingTypeID == conDb!.ID);

            if (typeConstraintDb == null)
            {
                typeConstraintDb = new TypeConstraint
                {
                    ConstrainingTypeID = conDb!.ID,
                    TypeParameter = tpDb
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

            var typeArguments = GetDbSet<TypeDefinitionTypeArgument>();

            var taDb = typeArguments
                .FirstOrDefault(x => x.Ordinal == taOrdinal && x.TypeDefinitionID == typeDefDb.ID);

            if (taDb == null)
            {
                taDb = new TypeDefinitionTypeArgument
                {
                    TypeDefinition = typeDefDb,
                    Ordinal = taOrdinal
                };

                typeArguments.Add(taDb);
            }

            taDb.Synchronized = true;
        }
    }
}
