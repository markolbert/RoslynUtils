using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeFactory : TypeEntityFactory<ITypeParameterSymbol, ParametricTypeDb>
    {
        public ParametricTypeFactory( IJ4JLogger logger ) 
            : base(  logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out ITypeParameterSymbol? result )
        {
            result = symbol as ITypeParameterSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( ITypeParameterSymbol symbol, out ParametricTypeDb? result )
        {
            result = null;

            if( symbol.DeclaringType != null )
                result = new TypeParametricTypeDb();

            if( result == null && symbol.DeclaringMethod != null )
                result = new MethodParametricTypeDb();

            if( result == null )
                Logger.Error<string>( "'{0}' is not contained by either an IMethodSymbol or an INamedTypeSymbol",
                    Factories!.GetFullyQualifiedName( symbol ) );
            else 
                result.Constraints = symbol.GetParametricTypeConstraint();

            return result != null;
        }
    }
}