using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : IEquatable<ISyntaxWalker>
    {
        Type SymbolType { get; }
        ReadOnlyCollection<IAssemblySymbol> DocumentationAssemblies { get; }
        bool InDocumentationScope( IAssemblySymbol assemblySymbol );
        bool Process(List<CompiledProject> compiledProjects);
    }
}
