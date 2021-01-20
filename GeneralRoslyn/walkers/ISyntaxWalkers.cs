using System.Collections.Generic;
using J4JSoftware.Utilities;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalkers : ISortedCollection<ISyntaxWalker>
    {
        bool Process( List<CompiledProject> compResults, bool stopOnFirstError = false );
    }
}