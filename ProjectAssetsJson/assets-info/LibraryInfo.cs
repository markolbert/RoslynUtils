using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public interface ILoadFromNamed<in TContainer>
    {
        bool Load( string rawName, TContainer container );
    }

    public class LibraryInfo : ProjectAssetsBase, ILoadFromNamed<ExpandoObject>
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

        public bool Load( string rawName, ExpandoObject container )
        {
            if( !ValidateLoadArguments( rawName, container ) )
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

            if( !GetProperty<string>( container, "SHA512", out var sha512 )
                || !GetProperty<string>( container, "path", out var path )
                || !GetProperty<List<string>>( container, "files", out var files )
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