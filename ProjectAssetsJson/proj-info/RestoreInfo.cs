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

        public bool Initialize( ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( container, context ) )
                return false;

            var okay = GetProperty<string>( container, "projectStyle", context, out var styleText );
            okay &= GetProperty<string>( container, "projectUniqueName", context, out var uniqueName );
            okay &= GetProperty<string>( container, "projectName", context, out var projName );
            okay &= GetProperty<string>( container, "projectPath", context, out var path );
            okay &= GetProperty<string>( container, "packagesPath", context, out var pkgPath, optional : true );
            okay &= GetProperty<string>( container, "outputPath", context, out var outPath );
            okay &= GetProperty<List<string>>( container, "fallbackFolders", context, out var fallbackList,
                optional : true );
            okay &= GetProperty<List<string>>( container, "configFilePaths", context, out var configPaths,
                optional : true );
            okay &= GetProperty<List<string>>( container, "originalTargetFrameworks", context, out var origFWText );
            okay &= GetProperty<ExpandoObject>( container, "sources", context, out var srcContainer, optional : true );

            if( !okay ) return false;

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