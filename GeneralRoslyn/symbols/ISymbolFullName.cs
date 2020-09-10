using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolFullName
    {
        string GetFullName( ISymbol symbol );
    }
}