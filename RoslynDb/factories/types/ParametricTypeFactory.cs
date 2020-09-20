using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeFactory : TypeEntityFactory<ITypeParameterSymbol, ParametricTypeDb>
    {
        public ParametricTypeFactory( IJ4JLogger logger ) 
            : base( SharpObjectType.ParametricType,  logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out ITypeParameterSymbol? result )
        {
            result = null;

            if( symbol is ITypeParameterSymbol tpSymbol && tpSymbol.DeclaringType != null )
                result = tpSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( ITypeParameterSymbol symbol, out ParametricTypeDb? result )
        {
            result = null;

            // this should never get tripped...
            if( symbol.DeclaringType != null )
                result = new ParametricTypeDb();

            if( result == null )
                Logger.Error<string>( "'{0}' is not contained by either an INamedTypeSymbol",
                    symbol.ToFullName() );
            else 
                result.Constraints = symbol.GetParametricTypeConstraint();

            return result != null;
        }
    }
}