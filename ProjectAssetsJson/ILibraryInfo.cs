using System.Collections.Generic;
using System.Dynamic;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public interface ILibraryInfo : IInitializeFromNamed<ExpandoObject>
    {
        string Assembly { get; }
        SemanticVersion Version { get; }
        ReferenceType Type { get; }
        string Path { get; }
        string GetAbsolutePath( IEnumerable<string> repositoryPaths, TargetFramework tgtFramework );
    }
}