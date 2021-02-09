using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface ISyntaxWalkerNG
    {
        NodeCollectors NodeCollectors { get; }
        void Process( List<CompiledProject> projects );
    }
}