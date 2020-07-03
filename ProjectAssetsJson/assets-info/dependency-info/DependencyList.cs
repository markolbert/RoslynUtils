using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class DependencyList : DependencyInfoBase, IInitializeFromNamed<ExpandoObject>
    {
        public DependencyList( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        public ReferenceType TargetType { get; set; }
        public List<SemanticVersion> Versions { get; } = new List<SemanticVersion>();

        public virtual bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !GetProperty<string>( container, "target", context, out var tgtTypeText ) )
                return false;

            if( !Enum.TryParse<ReferenceType>( tgtTypeText, true, out var tgtType ) )
            {
                Logger.Error<string, string>( "Couldn't parse '{0}' to a {1}", tgtTypeText, nameof(ReferenceType) );

                return false;
            }

            Assembly = rawName;
            TargetType = tgtType;

            // parse into individual version strings
            rawName = rawName.Replace( "[", "" )
                .Replace( ")", "" )
                .Replace( " ", "" );

            var versionTexts = rawName.Split( ',', StringSplitOptions.RemoveEmptyEntries );

            Versions.Clear();

            var retVal = true;

            foreach( var versionText in versionTexts )
            {
                if( VersionedText.TryParseSemanticVersion( versionText, out var version, Logger ) )
                    Versions.Add( version! );
                else retVal = false;
            }

            return retVal;
        }
    }
}