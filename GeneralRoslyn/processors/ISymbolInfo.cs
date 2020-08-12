using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolInfo
    {
        SymbolInfo Create( ISymbol symbol );
        string GetFullyQualifiedName( ISymbol symbol );
        string GetName( ISymbol symbol );
    }
}