using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class RestoreInfo : ConfigurationBase
    {
        public RestoreInfo( IJ4JLogger logger )
            : base( logger )
        {
        }

        public string ProjectUniqueName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public string PackagesPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public ProjectStyle? ProjectStyle { get; private set; }
        public List<string> FallbackFolders { get; } = new List<string>();
        public List<string> ConfigurationFilePaths { get; } = new List<string>();
        public List<TargetFramework> OriginalTargetFrameworks { get; } = new List<TargetFramework>();
        public List<string> Sources { get; } = new List<string>();
        public List<object> Frameworks { get; } = new List<object>();

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
                Logger.Error<string, string>( "Couldn't parse projectStyle text '{0}' as a {1}", 
                    styleText,
                    nameof(ProjectStyle) );

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

            List<string>? sources = null;
            if( srcContainer != null && !LoadNamesFromContainer( srcContainer, out sources ) )
                return false;

            ProjectStyle = style;
            ProjectUniqueName = uniqueName;
            ProjectName = projName;
            ProjectPath = path;
            PackagesPath = pkgPath;

            OriginalTargetFrameworks.Clear();
            OriginalTargetFrameworks.AddRange( tgtFrameworks! );

            OutputPath = outPath;

            FallbackFolders.Clear();
            FallbackFolders.AddRange(fallbackList);

            ConfigurationFilePaths.Clear();
            ConfigurationFilePaths.AddRange(configPaths);

            Sources.Clear();
            Sources.AddRange(sources!);

            return true;
        }
    }
}