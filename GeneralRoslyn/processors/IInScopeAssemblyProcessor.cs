using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface IInScopeAssemblyProcessor
    {
        bool Initialize();
        bool Synchronize( List<CompiledProject> projects );
        bool Cleanup();
    }
}