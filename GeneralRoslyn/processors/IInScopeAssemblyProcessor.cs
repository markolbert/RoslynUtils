using System.Collections.Generic;

namespace J4JSoftware.Roslyn.Deprecated
{
    public interface IInScopeAssemblyProcessor
    {
        bool Initialize();
        bool Synchronize( IEnumerable<CompiledProject> projects );
    }
}