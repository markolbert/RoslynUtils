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

            var okay = container.GetProperty<string>( "projectStyle", out var styleText );
            okay &= container.GetProperty<string>( "projectUniqueName", out var uniqueName );
            okay &= container.GetProperty<string>( "projectName", out var projName );
            okay &= container.GetProperty<string>( "projectPath", out var path );
            okay &= container.GetProperty<string>( "packagesPath", out var pkgPath, optional : true );
            okay &= container.GetProperty<string>( "outputPath", out var outPath );
            okay &= container.GetProperty<List<string>>( "fallbackFolders", out var fallbackList,
                optional : true );
            okay &= container.GetProperty<List<string>>( "configFilePaths", out var configPaths,
                optional : true );
            okay &= container.GetProperty<List<string>>( "originalTargetFrameworks", out var origFWText );
            okay &= container.GetProperty<ExpandoObject>( "sources", out var srcContainer,true );

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
                    if(TargetFramework.Create( t, TargetFrameworkTextStyle.Simple, out var retVal) )
                        return retVal;

                    origFWValid = false;

                    return null;

                } )
                .ToList();

            if( !origFWValid )
                return false;

            List<string>? sources = null;
            if( srcContainer != null && !srcContainer.LoadNamesFromContainer( out sources ) )
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