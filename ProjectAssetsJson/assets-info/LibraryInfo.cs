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
            ReferenceType refType,
            IJ4JLogger<LibraryInfo> logger
        )
            : base( logger )
        {
            Type = refType;
        }

        public string Assembly { get; private set; }
        public SemanticVersion Version { get; private set; }
        public ReferenceType Type { get; }
        public string Path { get; private set; }

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

        public virtual string GetAbsolutePath( IEnumerable<string> repositoryPaths, TargetFramework tgtFramework )
        {
            if( repositoryPaths == null )
            {
                Logger.Error( $"Undefined {nameof(repositoryPaths)}" );
                return null;
            }

            if( tgtFramework == null )
            {
                Logger.Error( $"Undefined {nameof(tgtFramework)}" );
                return null;
            }

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

                var filePath = System.IO.Path.Combine( match.path, $"{Assembly}.dll" );

                if( File.Exists( filePath ) )
                    return filePath;

                // check to see if the "it's already included from elsewhere" marker file is there
                // in which case return null but don't issue a warning
                filePath = System.IO.Path.Combine( match.path, "_._" );

                if( File.Exists( filePath ) )
                    return null;
            }

            // nuget appears to use directories starting with "runtime" to indicate runtime-only libraries,
            // typically for other operating systems...suppress warnings associated with such
            if( Path.IndexOf( "runtime", StringComparison.Ordinal ) != 0 )
                Logger.Warning( $"Couldn't find '{Path}' in provided repositories" );

            return null;
        }
    }
}