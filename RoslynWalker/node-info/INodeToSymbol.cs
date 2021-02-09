using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public interface INodeCollectorBase
    {
        NodeCollectors Container { get; }
        StoreSymbolResult StoreSymbol( SyntaxNode node, CompiledFile compiledFile, out ISymbol? mappedSymbol );
    }

    public interface INodeCollector : INodeCollectorBase, IEnumerable<ISymbol>
    {
        bool HandlesNode( SyntaxNode node );
        Type SymbolType { get; }
        void Clear();
        void RemoveDuplicates();
    }

    public interface INodeCollector<TSymbol> : INodeCollector
        where TSymbol : class, ISymbol
    {
        ReadOnlyCollection<TSymbol> Symbols { get; }
    }
}