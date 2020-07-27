using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolSink
    {
        bool SupportsSymbol( Type symbolType );
        bool InitializeSink( ISyntaxWalker syntaxWalker );
        bool FinalizeSink( ISyntaxWalker syntaxWalker );
        bool OutputSymbol(ISyntaxWalker syntaxWalker, ISymbol symbol );
    }

    public interface ISymbolSink<in TSymbol, TSink> : ISymbolSink
        where TSymbol : ISymbol
        where TSink : class
    {
        bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol );
        bool TryGetSunkValue( TSymbol symbol, out TSink? result );
    }

    public interface IDefaultSymbolSink : ISymbolSink
    {
    }
}