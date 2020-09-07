using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FixedTypeFactory : ImplementableTypeEntityFactory<INamedTypeSymbol, FixedTypeDb>
    {
        public FixedTypeFactory( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol symbol, out INamedTypeSymbol? result )
        {
            result = null;

            if( symbol is INamedTypeSymbol ntSymbol && !ntSymbol.IsGenericType )
                result = ntSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( INamedTypeSymbol symbol, out FixedTypeDb? result )
        {
            result = new FixedTypeDb();

            return true;
        }
    }
}