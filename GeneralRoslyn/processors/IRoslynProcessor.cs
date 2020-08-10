using System;

namespace J4JSoftware.Roslyn
{
    public interface IRoslynProcessor
    {
        Type SupportedType { get; }
        bool Process( object inputData );
    }

    public interface IRoslynProcessor<in TInput> : IRoslynProcessor
    {
        bool Process( TInput inputData );
    }
}