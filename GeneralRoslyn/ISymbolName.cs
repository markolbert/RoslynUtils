using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolName
    {
        string ToAssemblyBasedName( ISymbol symbol );
        string ToTypeLocalName( ISymbol symbol );
        string ToShortName( ISymbol symbol );
    }
}