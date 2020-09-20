using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricMethodTypeFactory : TypeEntityFactory<ITypeParameterSymbol, ParametricMethodTypeDb>
    {
        public ParametricMethodTypeFactory(IJ4JLogger logger)
            : base(SharpObjectType.ParametricMethodType, logger)
        {
        }

        protected override bool GetEntitySymbol(ISymbol? symbol, out ITypeParameterSymbol? result)
        {
            result = null;

            if (symbol is ITypeParameterSymbol tpSymbol && tpSymbol.DeclaringMethod != null)
                result = tpSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity(ITypeParameterSymbol symbol, out ParametricMethodTypeDb? result)
        {
            result = null;

            // this should never get tripped...
            if (symbol.DeclaringType != null)
                result = new ParametricMethodTypeDb();

            if (result == null)
                Logger.Error<string>("'{0}' is not contained by an IMethodSymbol",
                    symbol.ToFullName());
            else
                result.Constraints = symbol.GetParametricTypeConstraint();

            return result != null;
        }
    }
}