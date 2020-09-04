using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IAtomicProcessor
    {
        bool Process( object inputData );
    }

    public interface IAtomicProcessor<TSymbol> : IAtomicProcessor, ITopologicalSort<IAtomicProcessor<TSymbol>>
        where TSymbol : ISymbol
    {
        bool Process( IEnumerable<TSymbol> inputData );
    }
}