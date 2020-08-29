using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class MethodTypeParameterProcessor : BaseProcessorDb<IMethodSymbol, ITypeParameterSymbol>
    {
        public MethodTypeParameterProcessor( 
            RoslynDbContext dbContext, 
            ISymbolInfoFactory symbolInfo, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            foreach( var typeParameter in methodSymbol.TypeParameters )
            {
                yield return typeParameter;
            }
        }

        protected override bool ProcessSymbol(ITypeParameterSymbol symbol)
        {
            return true;
            //var typeParameters = GetDbSet<TypeParameter>();

            //var constraints = symbol.GetTypeParameterConstraint();

            //var fqnTypeConstraints = symbol.ConstraintTypes
            //    .Select(ct => SymbolInfo.GetFullyQualifiedName(ct))
            //    .ToList();

            //// see if the TypeParameter entity is already in the database
            //// match on Constraints and identical TypeConstraints. Do this in two phases,
            //// first retrieving TypeParameters based solely on the Constraints property
            //var possibleDb = typeParameters
            //    .Include(x => x.TypeConstraints)
            //    .ThenInclude(x => x.ConstrainingType)
            //    .Where(x => x.Constraints == constraints && x.Name == symbol.Name)
            //    .ToList();

            //var tpDb = possibleDb.Count switch
            //{
            //    0 => null,
            //    _ => possibleDb
            //        .FirstOrDefault(x =>
            //            !x.TypeConstraints
            //                .Select(tc => tc.ConstrainingType.FullyQualifiedName)
            //                .Except(fqnTypeConstraints).Any()
            //            && x.TypeConstraints.Count == fqnTypeConstraints.Count
            //        )
            //};

            //var allOkay = true;

            //if (tpDb == null)
            //{
            //    tpDb = new TypeParameter();
            //    typeParameters.Add(tpDb);

            //    // create the constraint entities
            //    foreach (var tcSymbol in symbol.ConstraintTypes)
            //    {
            //        allOkay &= ProcessTypeConstraints(tpDb, tcSymbol);
            //    }
            //}

            //tpDb.Synchronized = true;
            //tpDb.Constraints = constraints;

            //SaveChanges();

            //return allOkay;
        }

        //private bool ProcessTypeConstraints( TypeParameter tpDb, ITypeSymbol constraintSymbol)
        //{
        //    var symbolInfo = SymbolInfo.Create(constraintSymbol);

        //    if (!(symbolInfo.Symbol is INamedTypeSymbol) && symbolInfo.TypeKind != TypeKind.Array)
        //    {
        //        Logger.Error<string>(
        //            "Constraining type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
        //            symbolInfo.SymbolName);
        //        return false;
        //    }

        //    if (!GetByFullyQualifiedName<FixedTypeDb>(constraintSymbol, out var conDb))
        //        return false;

        //    var typeConstraints = GetDbSet<TypeConstraint>();

        //    var typeConstraintDb = typeConstraints
        //        .FirstOrDefault(c => c.TypeParameterID == tpDb.ID && c.ConstrainingTypeID == conDb!.ID);

        //    if (typeConstraintDb == null)
        //    {
        //        typeConstraintDb = new TypeConstraint
        //        {
        //            ConstrainingTypeID = conDb!.ID,
        //            TypeParameter = tpDb
        //        };

        //        typeConstraints.Add(typeConstraintDb);
        //    }

        //    typeConstraintDb.Synchronized = true;

        //    return true;
        //}
    }
}
