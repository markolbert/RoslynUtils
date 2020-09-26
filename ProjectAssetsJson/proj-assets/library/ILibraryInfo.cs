using NuGet.Versioning;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public interface ILibraryInfo
    {
        public string Assembly { get; }
        public SemanticVersion Version { get; }
        public ReferenceType Type { get; }
    }
}