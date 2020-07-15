using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolSink
    {
        bool SupportsSymbol( Type symbolType );
        bool OutputSymbol(ISymbol symbol);
    }

    public interface ISymbolSink<in TSymbol> : ISymbolSink
        where TSymbol : class, ISymbol
    {
        bool OutputSymbol( TSymbol symbol );
    }

    public interface IDefaultSymbolSink : ISymbolSink
    {
    }
}