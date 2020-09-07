using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Deprecated
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