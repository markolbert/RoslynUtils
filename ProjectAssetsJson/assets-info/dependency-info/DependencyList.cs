using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class DependencyList : DependencyInfoBase, ILoadFromNamed<ExpandoObject>
    {
        public DependencyList( IJ4JLogger<DependencyList> logger ) 
            : base( logger )
        {
        }

        public ReferenceType TargetType { get; set; }
        public List<SemanticVersion> Versions { get; set; }

        public virtual bool Load( string rawName, ExpandoObject container )
        {
            if( !ValidateLoadArguments( rawName, container ) )
                return false;

            if( !GetProperty<string>( container, "target", out var tgtTypeText ) )
                return false;

            if( !Enum.TryParse<ReferenceType>( tgtTypeText, true, out var tgtType ) )
            {
                Logger.Error($"Couldn't parse '{tgtTypeText}' to a {nameof(ReferenceType)}");

                return false;
            }

            Assembly = rawName;
            TargetType = tgtType;

            // parse into individual version strings
            rawName = rawName.Replace( "[", "" )
                .Replace( ")", "" )
                .Replace( " ", "" );

            var versionTexts = rawName.Split( ',', StringSplitOptions.RemoveEmptyEntries );

            Versions = new List<SemanticVersion>();

            var retVal = true;

            foreach( var versionText in versionTexts )
            {
                if( VersionedText.TryParseSemanticVersion( versionText, out var version, Logger ) )
                    Versions.Add( version );
                else retVal = false;
            }

            return retVal;
        }
    }
}