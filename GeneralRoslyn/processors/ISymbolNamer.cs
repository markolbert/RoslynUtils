using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolNamer
    {
        //SymbolInfo Create( ISymbol symbol );
        SymbolDisplayFormat FullyQualifiedFormat { get; }
        SymbolDisplayFormat GenericTypeFormat { get; }
        SymbolDisplayFormat SimpleNameFormat { get; }

        string GetFullyQualifiedName( ISymbol symbol );
        string GetName( ISymbol symbol );
    }
}