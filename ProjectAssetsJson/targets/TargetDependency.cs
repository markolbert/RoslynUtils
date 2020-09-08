using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class TargetDependency : ProjectAssetsBase
    {
        public TargetDependency(
            string text, 
            SemanticVersion version,
            Func<IJ4JLogger> loggerFactory 
            )
            : base(loggerFactory)
        {
            Assembly = text;
            Version = version;
        }

        public string Assembly { get; }
        public SemanticVersion Version { get; }
    }
}