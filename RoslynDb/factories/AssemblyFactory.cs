using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyFactory : EntityFactory<IAssemblySymbol, AssemblyDb>
    {
        public AssemblyFactory( IJ4JLogger logger )
            : base( SharpObjectType.Assembly, logger )
        {
        }

        protected override bool GetEntitySymbol( ISymbol? symbol, out IAssemblySymbol? result )
        {
            result = null;

            if( symbol == null )
                return false;

            if( symbol is IAssemblySymbol assemblySymbol )
                result = assemblySymbol;

            if( symbol is IArrayTypeSymbol arraySymbol )
                result = arraySymbol.ElementType.ContainingAssembly;

            result ??= symbol.ContainingAssembly;

            return result != null;
        }

        protected override bool CreateNewEntity(IAssemblySymbol symbol, out AssemblyDb? result)
        {
            result = new AssemblyDb();

            return true;
        }
    }
}
