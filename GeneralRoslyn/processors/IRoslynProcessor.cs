using System;

namespace J4JSoftware.Roslyn
{
    public interface IRoslynProcessor
    {
        bool Process( object inputData );
    }

    public interface IRoslynProcessor<in TInput> : IRoslynProcessor
    {
        bool Process( TInput inputData );
    }
}