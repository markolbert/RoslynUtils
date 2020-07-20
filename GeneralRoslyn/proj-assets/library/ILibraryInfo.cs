using System.Collections.Generic;
using System.Dynamic;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public interface ILibraryInfo
    {
        public string Assembly { get; }
        public SemanticVersion Version { get; }
        public ReferenceType Type { get; }

        //bool GetAbsolutePath( string path, IEnumerable<string> repositoryPaths, TargetFramework tgtFramework,
        //    out PackageAbsolutePath? result );
    }
}