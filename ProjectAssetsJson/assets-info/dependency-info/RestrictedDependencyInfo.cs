using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class RestrictedDependencyInfo : DependencyInfoBase
    {
        public RestrictedDependencyInfo( IJ4JLogger logger )
            : base( logger )
        {
        }

        public SemanticVersion? Version { get; set; }
        public VersionConstraint Constraint { get; set; }

        public virtual bool Initialize( string text )
        {
            var parts = text.Split( ' ', StringSplitOptions.RemoveEmptyEntries );

            if( parts.Length != 3 )
            {
                Logger.Error<string>( "Couldn't parse assembly constraint '{text}'", text );

                return false;
            }

            if( !Versioning.GetSemanticVersion( parts[ 2 ], out var version ) )
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

        public bool MeetsConstraint( SemanticVersion toCheck )
        {
            if( toCheck == null ) return false;

            return Constraint switch
            {
                VersionConstraint.GreaterThan => toCheck > Version,
                VersionConstraint.Minimum => toCheck >= Version,
                VersionConstraint.LessThan => toCheck < Version,
                VersionConstraint.EqualTo => toCheck == Version,
                VersionConstraint.Maximum => toCheck < Version,
                VersionConstraint.Undefined => true,
                // should never get here...
                _ => false
            };
        }
    }
}