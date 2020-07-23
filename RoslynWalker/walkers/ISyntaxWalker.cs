using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : IEquatable<ISyntaxWalker>
    {
        Type SymbolType { get; }
        ReadOnlyCollection<IAssemblySymbol> ModelAssemblies { get; }

        bool Traverse( List<CompiledProject> compResults );
    }

    public interface ISyntaxWalker<TTarget> : ISyntaxWalker
        where TTarget : class, ISymbol
    {
    }
}
