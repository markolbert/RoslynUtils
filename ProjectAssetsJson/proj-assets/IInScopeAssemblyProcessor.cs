using System.Collections.Generic;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public interface IInScopeAssemblyProcessor
    {
        bool Initialize();
        bool Synchronize( List<CompiledProject> projects );
        bool Cleanup();
    }
}