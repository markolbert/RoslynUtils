using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class PackageLibrary : ProjectAssetsBase, ILibraryInfo
    {
        private readonly VersionedText _versionedText;

        public PackageLibrary(
            string text,
            ExpandoObject libInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            Type = ReferenceType.Package;

            if (!VersionedText.Create(text, out var verText))
                throw new ArgumentException($"Couldn't parse '{text}' into {typeof(VersionedText)}");

            _versionedText = verText!;

            SHA512 = GetProperty<string>( libInfo, "SHA512" );
            Path = GetProperty<string>( libInfo, "path" );
            Files = GetProperty<List<string>>( libInfo, "files" );
        }

        public string Assembly => _versionedText.TextComponent;
        public SemanticVersion Version => _versionedText.Version;
        public ReferenceType Type { get; }
        public string SHA512 { get; }
        public string Path { get; }
        public List<string> Files { get; }
    }
}