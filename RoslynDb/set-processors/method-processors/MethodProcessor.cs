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
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        public MethodProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolInfo, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            yield return methodSymbol;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol )
        {
            if( !GetByFullyQualifiedName<FixedTypeDb>( symbol.ContainingType, out var typeDb ) )
                return false;

            if( !GetByFullyQualifiedName<FixedTypeDb>( symbol.ReturnType, out var retValDb ) )
                return false;

            if( !GetByFullyQualifiedName<Method>( symbol, out var methodDb ) )
            {
                methodDb = new Method
                {
                    FullyQualifiedName = SymbolInfo.GetFullyQualifiedName( symbol )
                };

                var methods = GetDbSet<Method>();
                methods.Add( methodDb );
            }

            methodDb!.Name = symbol.Name;
            methodDb.Accessibility = symbol.DeclaredAccessibility;
            methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            methodDb.Kind = symbol.MethodKind;
            methodDb.DefiningTypeID = typeDb!.ID;
            methodDb.ReturnTypeID = retValDb!.ID;

            var allOkay = true;

            foreach( var tpSymbol in symbol.TypeParameters )
            {
                allOkay &= ProcessTypeParameter( methodDb, tpSymbol );
            }

            for( var idx = 0; idx < symbol.TypeArguments.Length; idx++ )
            {
                allOkay &= ProcessTypeArgument( methodDb, symbol.TypeArguments[ idx ], idx );
            }

            return allOkay;
        }

        private bool ProcessTypeParameter( Method methodDb, ITypeParameterSymbol tpSymbol )
        {
            return true;
        }

        private bool ProcessTypeArgument( Method methodDb, ITypeSymbol typeSymbol, int ordinal )
        {
            return true;
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
