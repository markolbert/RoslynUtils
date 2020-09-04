using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolProcessors<in TSymbol> 
        where TSymbol : ISymbol
    {
        bool Process( IEnumerable<TSymbol> context );
    }
}