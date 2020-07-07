using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class LibraryInfo : ProjectAssetsBase, ILibraryInfo
    {
        protected LibraryInfo(
            string text,
            Func<IJ4JLogger> loggerFactory,
            ReferenceType refType
        )
            : base( loggerFactory )
        {
            if( !VersionedText.Create( text, out var verText ) )
                throw new ArgumentException( $"Couldn't parse '{text}' into {typeof(VersionedText)}" );

            Assembly = verText!.TextComponent;
            Version = verText.Version;

            Type = refType;
        }

        public string Assembly { get; }
        public SemanticVersion Version { get; }
        public ReferenceType Type { get; }

        public bool GetAbsolutePath(string path, IEnumerable<string> repositoryPaths, TargetFramework tgtFramework, out PackageAbsolutePath? result)
        {
            result = null;

            foreach (var repositoryPath in repositoryPaths)
            {
                var pkgDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(repositoryPath, path));

                if (!Directory.Exists(pkgDir))
                    continue;

                // look for best match re: version
                pkgDir = System.IO.Path.Combine(pkgDir, "lib");

                if (!Directory.Exists(pkgDir))
                    continue;

                var fwDirectories = Directory.GetDirectories(pkgDir, tgtFramework.Framework + "*")
                    .Where(dir =>
                       TargetFramework.Create(System.IO.Path.GetFileName(dir), TargetFrameworkTextStyle.Simple, out var _))
                    .Select(dir =>
                    {
                        TargetFramework.Create(System.IO.Path.GetFileName(dir), TargetFrameworkTextStyle.Simple, out var tFramework);

                        return new
                        {
                            path = dir,
                            version = tFramework!.Version
                        };
                    })
                    .OrderByDescending(x => x.version)
                    .ToList();

                var match = fwDirectories.FirstOrDefault(x => x.version == tgtFramework.Version)
                            ?? fwDirectories.FirstOrDefault();

                if (match == null)
                    continue;

                var filePath1 = System.IO.Path.Combine(match.path, $"{Assembly}.dll");
                var filePath2 = System.IO.Path.Combine(match.path, $"_._");

                if (File.Exists(filePath1) || File.Exists(filePath2))
                {
                    result = new PackageAbsolutePath()
                    {
                        DllPath = filePath1,
                        TargetFramework = tgtFramework
                    };

                    return true;
                }
            }

            // nuget appears to use directories starting with "runtime" to indicate runtime-only libraries,
            // typically for other operating systems...suppress warnings associated with such
            if (path.IndexOf("runtime", StringComparison.Ordinal) != 0)
                Logger.Information($"Couldn't find '{path}' in provided repositories");

            return false;
        }
    }
}