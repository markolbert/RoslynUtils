using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : IAction<List<CompiledProject>>, IEquatable<ISyntaxWalker>
    {
        Type SymbolType { get; }
        string Name { get; }
    }

    public interface ISyntaxWalker<TSymbol> : ISyntaxWalker
        where TSymbol : ISymbol
    {
    }
}
