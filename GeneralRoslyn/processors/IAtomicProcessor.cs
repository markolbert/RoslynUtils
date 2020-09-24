using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    //public interface IAtomicProcessor
    //{
    //    bool Process( object inputData, bool stopOnFirstError = false);
    //}

    public interface ITopologicalActionConfiguration
    {
        bool StopOnFirstError { get; }
    }

    public interface IAtomicActionConfiguration : ITopologicalActionConfiguration
    {
        ISyntaxWalker SyntaxWalker { get; }
    }

    public class TopologicalActionConfiguration : ITopologicalActionConfiguration
    {
        public TopologicalActionConfiguration( bool stopOnFirstError = true )
        {
            StopOnFirstError = stopOnFirstError;
        }

        public bool StopOnFirstError { get; }
    }

    public class AtomicActionConfiguration : TopologicalActionConfiguration, IAtomicActionConfiguration
    {
        public AtomicActionConfiguration(ISyntaxWalker walker, bool stopOnFirstError = true)
            : base(stopOnFirstError)
        {
            SyntaxWalker = walker;
        }

        public ISyntaxWalker SyntaxWalker { get; }
    }

    public interface IAtomicProcessor<TSymbol> : ITopologicalAction<TSymbol>, IEquatable<IAtomicProcessor<TSymbol>>
        where TSymbol : ISymbol
    {
        bool Initialize( IAtomicActionConfiguration config );
    }

    public interface IProcessorCollection<in TItem>
    {
        bool Process( IEnumerable<TItem> items, bool stopOnFirstError = false );
    }


}