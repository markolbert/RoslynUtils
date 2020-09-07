using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class RestoreInfo : ProjectAssetsBase
    {
        public RestoreInfo( 
            string text,
            ExpandoObject restoreInfo,
            Func<IJ4JLogger> loggerFactory )
            : base( loggerFactory )
        {
            ProjectUniqueName = GetProperty<string>(restoreInfo, "projectUniqueName" );
            ProjectName = GetProperty<string>(restoreInfo, "projectName");
            ProjectPath = GetProperty<string>(restoreInfo, "projectPath");
            PackagesPath = GetProperty<string>(restoreInfo, "packagesPath", optional : true );
            OutputPath = GetProperty<string>(restoreInfo, "outputPath");
            ProjectStyle = GetEnum<ProjectStyle>(restoreInfo, "projectStyle" );
            FallbackFolders = GetProperty<List<string>>( restoreInfo,"fallbackFolders", optional : true );
            ConfigurationFilePaths = GetProperty<List<string>>(restoreInfo,"configFilePaths", optional: true);

            var origFWText = GetProperty<List<string>>( restoreInfo,"originalTargetFrameworks" );
            OriginalTargetFrameworks = origFWText.Select( t =>
                {
                    if( !TargetFramework.Create( t, TargetFrameworkTextStyle.Simple, out var tgtFW ) )
                        throw new InvalidEnumArgumentException(
                            $"Couldn't parse '{t}' to a {typeof(TargetFramework)}" );

                    return tgtFW!;
                } )
                .ToList();

            var asDict = (IDictionary<string, object>) GetProperty<ExpandoObject>(
                    restoreInfo, 
                    "sources", 
                    optional: true );

            Sources = asDict.Keys.ToList();

            CreateRestoreFrameworks( GetProperty<ExpandoObject>( restoreInfo, "frameworks" ) );
        }

        private void CreateRestoreFrameworks( ExpandoObject rfContainer )
        {
            foreach( var kvp in rfContainer )
            {
                if( kvp.Value is ExpandoObject childContainer )
                    Frameworks.Add( new RestoreFramework( kvp.Key, childContainer, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Restore framework item is not an ExpandoObject",
                        this.GetType(),
                        Logger);
            }
        }

        private void CreateWarningProperties(ExpandoObject warnings )
        {
            foreach (var kvp in warnings)
            {
                if (kvp.Value is List<string> childContainer)
                    WarningProperties.Add(new WarningProperty(kvp.Key, childContainer, LoggerFactory));
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Warning property item is not a List<string>",
                        this.GetType(),
                        Logger);
            }
        }

        public string ProjectUniqueName { get; }
        public string ProjectName { get; }
        public string ProjectPath { get; }
        public string PackagesPath { get; }
        public string OutputPath { get; }
        public ProjectStyle ProjectStyle { get; }
        public List<string> FallbackFolders { get; }
        public List<string> ConfigurationFilePaths { get; }
        public List<TargetFramework> OriginalTargetFrameworks { get; }
        public List<string> Sources { get; }
        public List<RestoreFramework> Frameworks { get; } = new List<RestoreFramework>();
        public List<WarningProperty> WarningProperties { get; } = new List<WarningProperty>();
    }
}