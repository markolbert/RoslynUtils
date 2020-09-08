using System;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class ProjectFrameworkDependency : ProjectAssetsBase
    {
        public ProjectFrameworkDependency(
            string text, 
            ExpandoObject dependencyInfo,
            Func<IJ4JLogger> loggerFactory 
            )
            : base(loggerFactory)
        {
            Assembly = text;
            Target = GetProperty<string>( dependencyInfo, "target" );
            Version = GetSemanticVersion( dependencyInfo, "version" );
        }

        public string Assembly { get; }
        public string Target { get; }
        public SemanticVersion Version { get; }
    }
}