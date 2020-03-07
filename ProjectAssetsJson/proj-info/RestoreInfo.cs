using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class RestoreInfo : ProjectAssetsBase
    {
        public RestoreInfo( IJ4JLogger<RestoreInfo> logger )
            : base( logger )
        {
        }

        public string ProjectUniqueName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string PackagesPath { get; set; }
        public string OutputPath { get; set; }
        public ProjectStyle ProjectStyle { get; set; }
        public List<string> FallbackFolders { get; set; }
        public List<string> ConfigurationFilePaths { get; set; }
        public List<TargetFramework> OriginalTargetFrameworks { get; set; }
        public List<string> Sources { get; set; }
        public List<object> Frameworks { get; set; }

        public bool Initialize( ExpandoObject container )
        {
            if( !ValidateInitializationArguments( container ) )
                return false;

            if( !GetProperty<string>( container, "projectStyle", out var styleText )
                || !GetProperty<string>( container, "projectUniqueName", out var uniqueName )
                || !GetProperty<string>( container, "projectName", out var projName )
                || !GetProperty<string>( container, "projectPath", out var path )
                || !GetProperty<string>( container, "packagesPath", out var pkgPath, optional : true )
                || !GetProperty<string>( container, "outputPath", out var outPath )
                || !GetProperty<List<string>>( container, "fallbackFolders", out var fallbackList, optional : true )
                || !GetProperty<List<string>>( container, "configFilePaths", out var configPaths, optional : true )
                || !GetProperty<List<string>>( container, "originalTargetFrameworks", out var origFWText )
                || !GetProperty<ExpandoObject>( container, "sources", out var srcContainer, optional : true )
            )
                return false;

            if( !Enum.TryParse<ProjectStyle>( styleText, true, out var style ) )
            {
                Logger.Error( $"Couldn't parse projectStyle text '{styleText}' as a {nameof(ProjectStyle)}" );

                return false;
            }

            var origFWValid = true;

            var tgtFrameworks = origFWText.Select( t =>
                {
                    if( TargetFramework.CreateTargetFramework( t, out var retVal, Logger ) )
                        return retVal;

                    origFWValid = false;

                    return null;

                } )
                .ToList();

            if( !origFWValid )
                return false;

            List<string> sources = null;
            if( srcContainer != null && !LoadNamesFromContainer( srcContainer, out sources ) )
                return false;

            ProjectStyle = style;
            ProjectUniqueName = uniqueName;
            ProjectName = projName;
            ProjectPath = path;
            PackagesPath = pkgPath;
            OriginalTargetFrameworks = tgtFrameworks;
            OutputPath = outPath;
            FallbackFolders = fallbackList;
            ConfigurationFilePaths = configPaths;
            Sources = sources;

            return true;
        }
    }
}