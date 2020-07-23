using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyQualifiedSymbolNamer : SymbolNamer
    {
        public AssemblyQualifiedSymbolNamer( IJ4JLogger logger )
            : base( SymbolDisplayFormat.MinimallyQualifiedFormat, logger )
        {
        }
    }
}