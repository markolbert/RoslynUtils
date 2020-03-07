using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class RestrictedDependencyInfo : DependencyInfoBase
    {
        public RestrictedDependencyInfo( IJ4JLogger<RestrictedDependencyInfo> logger )
            : base( logger )
        {
        }

        public SemanticVersion Version { get; set; }
        public VersionConstraint Constraint { get; set; }

        public virtual bool Initialize( string text )
        {
            var parts = text.Split( ' ', StringSplitOptions.RemoveEmptyEntries );

            if( parts.Length != 3 )
            {
                Logger.Error( $"Couldn't parse assembly constraint '{text}'" );

                return false;
            }

            if( !VersionedText.TryParseSemanticVersion( parts[ 2 ], out var version, Logger ) )
                return false;

            Assembly = parts[ 0 ];
            Version = version;
            Constraint = parts[ 1 ] switch
            {
                "==" => VersionConstraint.EqualTo,
                "<=" => VersionConstraint.Maximum,
                ">=" => VersionConstraint.Minimum,
                ">" => VersionConstraint.GreaterThan,
                "<" => VersionConstraint.LessThan,
                _ => VersionConstraint.Undefined
            };

            return true;
        }
    }
}