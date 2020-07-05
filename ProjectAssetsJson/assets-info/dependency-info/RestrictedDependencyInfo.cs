using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class RestrictedDependencyInfo : DependencyInfo
    {
        public RestrictedDependencyInfo(
            string assembly, 
            SemanticVersion version, 
            VersionConstraint constraint,
            Func<IJ4JLogger> loggerFactory 
            )
            : base( assembly, version, loggerFactory )
        {
            Constraint = constraint;
        }

        public VersionConstraint Constraint { get; set; }

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