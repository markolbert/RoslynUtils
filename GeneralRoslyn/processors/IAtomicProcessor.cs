using System;

namespace J4JSoftware.Roslyn
{
    public interface IAtomicProcessor
    {
        bool Process( object inputData );
    }

    public interface IAtomicProcessor<TInput> : IAtomicProcessor, ITopologicalSort<IAtomicProcessor<TInput>>
    {
        bool Process( TInput inputData );
    }
}