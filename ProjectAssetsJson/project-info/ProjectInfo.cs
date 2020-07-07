using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectInfo : ProjectAssetsBase
    {
        public ProjectInfo( 
            string text,
            ExpandoObject projInfo,
            Func<IJ4JLogger> loggerFactory
            )
            : base( loggerFactory )
        {
            Version = GetSemanticVersion(projInfo,"version" );
            Restore = new RestoreInfo( "restore", GetProperty<ExpandoObject>(projInfo,"restore" ), LoggerFactory );

            CreateFrameworks( GetProperty<ExpandoObject>(projInfo,"frameworks" ) );
        }

        private void CreateFrameworks( ExpandoObject fwCollection )
        {
            foreach( var kvp in fwCollection )
            {
                if( kvp.Value is ExpandoObject fwInfo )
                    ProjectFrameworks.Add(new ProjectFramework(kvp.Key, fwInfo, LoggerFactory)  );
            }
        }

        public bool IsNetCoreApplication =>
            ProjectFrameworks?.All(fw => fw.TargetFramework == CSharpFramework.NetCoreApp) ?? false;

        public SemanticVersion Version { get; }
        public RestoreInfo Restore { get; }
        public List<ProjectFramework> ProjectFrameworks { get; } = new List<ProjectFramework>();
        public List<WarningProperty> WarningProperties { get; } = new List<WarningProperty>();
    }
}
