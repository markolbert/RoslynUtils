using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    //public interface IAtomicProcessor
    //{
    //    bool Process( object inputData, bool stopOnFirstError = false);
    //}

    public interface IAtomicProcessor<TSymbol> : ITopologicalAction<TSymbol>, IEquatable<IAtomicProcessor<TSymbol>>
        where TSymbol : ISymbol
    {
    }

    public interface IProcessorCollection<in T>
    {
        bool Process( IEnumerable<T> items, bool stopOnFirstError = false );
    }
}