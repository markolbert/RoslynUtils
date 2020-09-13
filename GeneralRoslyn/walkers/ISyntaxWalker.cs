using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : ITopologicalAction<CompiledProject>, IEquatable<ISyntaxWalker> //ITopologicalSort<ISyntaxWalker>
    {
        Type SymbolType { get; }
        ReadOnlyCollection<IAssemblySymbol> DocumentationAssemblies { get; }
        bool InDocumentationScope( IAssemblySymbol assemblySymbol );
    }

    public interface ISyntaxWalker<TSymbol> : ISyntaxWalker
        where TSymbol : ISymbol
    {
    }
}
