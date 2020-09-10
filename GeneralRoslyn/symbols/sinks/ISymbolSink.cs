using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISymbolSink
    {
        bool SupportsSymbol( Type symbolType );
        bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false );
        bool FinalizeSink( ISyntaxWalker syntaxWalker );
        bool OutputSymbol(ISyntaxWalker syntaxWalker, ISymbol symbol );
    }

    public interface ISymbolSink<in TSymbol> : ISymbolSink
        where TSymbol : ISymbol
    {
        bool OutputSymbol( ISyntaxWalker syntaxWalker, TSymbol symbol );
    }

    public interface IDefaultSymbolSink : ISymbolSink
    {
    }
}