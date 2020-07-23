using System;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolNamer
    {
        ReadOnlyCollection<Type> SupportedSymbolTypes { get; }

        string GetSymbolName<TSymbol>( TSymbol symbol )
            where TSymbol : ISymbol;
    }
}