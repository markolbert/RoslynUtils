using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolName
    {
        string GetSymbolName( ISymbol symbol );
    }
}