using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface ITopologicalAction<in TArg>
    {
        bool Process( IEnumerable<TArg> items, bool stopOnFirstError = false );
    }
}