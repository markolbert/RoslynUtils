using System;

namespace J4JSoftware.Roslyn
{
    public interface IRoslynProcessor
    {
        bool Process( ISyntaxWalker syntaxWalker, object inputData );
    }

    public interface IRoslynProcessor<in TInput> : IRoslynProcessor
    {
        bool Process( ISyntaxWalker syntaxWalker, TInput inputData );
    }
}