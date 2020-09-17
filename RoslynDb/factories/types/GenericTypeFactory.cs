using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class GenericTypeFactory : ImplementableTypeEntityFactory<INamedTypeSymbol, GenericTypeDb>
    {
        public GenericTypeFactory( IJ4JLogger logger)
            : base( SharpObjectType.GenericType, logger)
        {
        }

        protected override bool GetEntitySymbol(ISymbol? symbol, out INamedTypeSymbol? result)
        {
            result = null;

            if( symbol == null )
                return false;

            if (symbol is INamedTypeSymbol ntSymbol && ntSymbol.IsGenericType)
                result = ntSymbol;

            return result != null;
        }

        protected override bool CreateNewEntity( INamedTypeSymbol symbol, out GenericTypeDb? result )
        {
            result = new GenericTypeDb();

            return true;
        }
    }
}