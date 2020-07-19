using System.Collections.Generic;

namespace J4JSoftware.Roslyn
{
    public interface IInScopeAssemblyProcessor
    {
        bool Initialize();
        bool Synchronize( List<string> projFiles );
        bool Finalize();
    }
}