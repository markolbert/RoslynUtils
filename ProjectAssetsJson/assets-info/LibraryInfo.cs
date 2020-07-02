using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class PackageAbsolutePath
    {
        public TargetFramework? TargetFramework { get; set; }
        public string DllPath { get; set; } = string.Empty;
        public bool IsVirtual => DllPath != null && DllPath == "_._";
    }

    public class LibraryInfo : ProjectAssetsBase, ILibraryInfo
    {
        protected LibraryInfo(
            ReferenceType refType,
            IJ4JLogger logger
        )
            : base( logger )
        {
            Type = refType;
        }

        public string Assembly { get; private set; } = string.Empty;
        public SemanticVersion Version { get; private set; } = new SemanticVersion( 0, 0, 0 );
        public ReferenceType Type { get; }
        public string Path { get; private set; } = string.Empty;

        public virtual bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !VersionedText.CreateVersionedText( rawName, out var verText, Logger ) )
                return false;

            if( !GetProperty<string>( container, "type", context, out var libTypeText ) )
                return false;

            if( !Enum.TryParse<ReferenceType>( libTypeText, true, out var libType ) )
            {
                Logger.Error(
                    $"Property 'type' ('{libTypeText}') isn't parseable to a {nameof( ReferenceType )}" );

                return false;
            }

            if( libType != Type )
            {
                Logger.Error($"Expected a {Type} library, encountered a {libType} instead");

                return false;
            }

            if( !GetProperty<string>( container, "path", context, out var path ) )
                return false;

            Assembly = verText.TextComponent;
            Version = verText.Version;
            Path = path;

            return true;
        }

        public virtual bool GetAbsolutePath( IEnumerable<string> repositoryPaths, TargetFramework tgtFramework, out PackageAbsolutePath? result )
        {
            result = null;

            //if( repositoryPaths == null )
            //{
            //    Logger.Error( $"Undefined {nameof(repositoryPaths)}" );
            //    return false;
            //}

            //if( tgtFramework == null )
            //{
            //    Logger.Error( $"Undefined {nameof(tgtFramework)}" );
            //    return false;
            //}

            foreach( var repositoryPath in repositoryPaths )
            {
                var pkgDir = System.IO.Path.GetFullPath( System.IO.Path.Combine( repositoryPath, Path ) );

                if( !Directory.Exists( pkgDir ) )
                    continue;

                // look for best match re: version
                pkgDir = System.IO.Path.Combine( pkgDir, "lib" );

                if( !Directory.Exists( pkgDir ) )
                    continue;

                var fwDirectories = Directory.GetDirectories( pkgDir, tgtFramework.Framework + "*" )
                    .Where( dir =>
                        VersionedText.CreateVersionedText( System.IO.Path.GetFileName( dir ), out var result, Logger ) )
                    .Select( dir =>
                    {
                        VersionedText.CreateVersionedText( System.IO.Path.GetFileName( dir ), out var verText, Logger );

                        return new
                        {
                            path = dir,
                            version = verText.Version
                        };
                    } )
                    .OrderByDescending( x => x.version )
                    .ToList();

                var match = fwDirectories.FirstOrDefault( x => x.version == tgtFramework.Version )
                            ?? fwDirectories.FirstOrDefault();

                if( match == null )
                    continue;

                var filePath1 = System.IO.Path.Combine( match.path, $"{Assembly}.dll" );
                var filePath2 = System.IO.Path.Combine( match.path, $"_._" );

                if( File.Exists( filePath1 ) || File.Exists(filePath2) )
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
            if( Path.IndexOf( "runtime", StringComparison.Ordinal ) != 0 )
                Logger.Information( $"Couldn't find '{Path}' in provided repositories" );

            return false;
        }
    }
}