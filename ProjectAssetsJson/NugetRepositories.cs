using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class NugetRepositories : ProjectAssetsBase
    {
        public NugetRepositories( ExpandoObject reposContainer, Func<IJ4JLogger> loggerFactory )
            : base( loggerFactory )
        {
            RepositoryPaths = ( (IDictionary<string, object>) reposContainer! ).Keys.ToList();
        }

        public List<string> RepositoryPaths { get; }


        public bool ResolvePackagePath(string pkgReference, TargetFramework tgtFramework, out PackageAbsolutePath? result)
        {
            result = null;

            if( !VersionedText.Create( pkgReference, out var pkgVersion ) )
            {
                Logger.Error<string, string>( 
                    "Failed to create a {0} from '{1}'", 
                    nameof(VersionedText),
                    pkgReference );

                return false;
            }

            foreach (var repositoryPath in RepositoryPaths)
            {
                var pkgDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(repositoryPath, pkgReference));

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

                var filePath1 = System.IO.Path.Combine(match.path, $"{pkgVersion!.TextComponent}.dll");
                var filePath2 = System.IO.Path.Combine(match.path, "_._");

                if (File.Exists(filePath1) || File.Exists(filePath2))
                {
                    result = new PackageAbsolutePath( tgtFramework, filePath1 );

                    return true;
                }
            }

            // nuget appears to use directories starting with "runtime" to indicate runtime-only libraries,
            // typically for other operating systems...suppress warnings associated with such
            if (pkgReference.IndexOf("runtime", StringComparison.Ordinal) != 0)
                Logger.Information($"Couldn't find '{pkgReference}' in provided repositories");

            return false;
        }
    }
}
