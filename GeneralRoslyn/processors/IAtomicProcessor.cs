using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IAtomicProcessor
    {
        bool Process( object inputData, bool stopOnFirstError = false);
    }

    public interface IAtomicProcessor<TSymbol> : IAtomicProcessor, ITopologicalSort<IAtomicProcessor<TSymbol>>
        where TSymbol : ISymbol
    {
        bool Process( IEnumerable<TSymbol> inputData, bool stopOnFirstError = false );
    }
}