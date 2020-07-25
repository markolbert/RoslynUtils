using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : IRoslynProcessor<List<CompiledProject>>, IEquatable<ISyntaxWalker>
    {
        Type SymbolType { get; }
        ReadOnlyCollection<IAssemblySymbol> ModelAssemblies { get; }
    }
}
