using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class LibraryInfo : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        public LibraryInfo(
            IJ4JLogger<LibraryInfo> logger
        )
            : base( logger )
        {
        }

        public string Assembly { get; set; }
        public SemanticVersion Version { get; set; }
        public string SHA512 { get; set; }
        public ReferenceType Type { get; set; }
        public string Path { get; set; }
        public List<string> Files { get; set; }

        public bool Initialize( string rawName, ExpandoObject container )
        {
            if( !ValidateInitializationArguments( rawName, container ) )
                return false;

            if( !VersionedText.CreateVersionedText( rawName, out var verText, Logger ) )
                return false;

            if( !GetProperty<string>(container, "type", out var libTypeText ))
                return false;

            if( !Enum.TryParse<ReferenceType>( libTypeText, true, out var libType ) )
            {
                Logger.Error(
                    $"Property 'type' ('{libTypeText}') isn't parseable to a {nameof(ReferenceType)}" );

                return false;
            }

            var isProject = libType == ReferenceType.Project;

            if( !GetProperty<string>( container, "SHA512", out var sha512, optional: isProject )
                || !GetProperty<string>( container, "path", out var path )
                || !GetProperty<List<string>>( container, "files", out var files, optional: isProject )
            )
                return false;

            Assembly = verText.TextComponent;
            Version = verText.Version;
            SHA512 = sha512;
            Type = libType;
            Path = path;
            Files = files;

            return true;
        }
    }
}