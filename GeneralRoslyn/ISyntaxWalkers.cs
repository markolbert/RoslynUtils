using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalkers : ITopologicallySorted<ISyntaxWalker>
    {
        bool Process( List<CompiledProject> compResults );
    }

    public interface ISymbolProcessors : ITopologicallySorted<ISymbolProcessor>
    {

    }
}