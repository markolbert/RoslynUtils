using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalker : IEquatable<ISyntaxWalker>
    {
        Type SymbolType { get; }
        ReadOnlyCollection<IAssemblySymbol> ModelAssemblies { get; }

        bool Traverse( List<CompilationResults> compResults );
    }

    public interface ISyntaxWalker<TTarget> : ISyntaxWalker
        where TTarget : class, ISymbol
    {
    }
}
