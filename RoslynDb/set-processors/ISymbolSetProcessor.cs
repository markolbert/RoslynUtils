using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolSetProcessor<in TSymbol>
    {
        bool Process( IEnumerable<TSymbol> symbols );
    }

    //public interface ITypeDefinitionProcessors
    //{
    //    bool Process( List<ITypeSymbol> typeSymbols );
    //}
}