using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class DependencyInfo : DependencyInfoBase
    {
        public DependencyInfo(
            string assembly, 
            SemanticVersion version,
            Func<IJ4JLogger> loggerFactory 
            )
            : base(assembly, loggerFactory)
        {
            Version = version;
        }

        public SemanticVersion Version { get; }
    }
}