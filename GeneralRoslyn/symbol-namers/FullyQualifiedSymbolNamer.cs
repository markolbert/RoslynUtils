using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class FullyQualifiedSymbolNamer : SymbolNamer
    {
        public FullyQualifiedSymbolNamer( IJ4JLogger logger ) 
            : base( SymbolNamers.DefaultFormat, logger )
        {
            AddSupportedType<IAssemblySymbol>();
            AddSupportedType<INamespaceSymbol>();
            AddSupportedType<INamedTypeSymbol>();
        }
    }
}