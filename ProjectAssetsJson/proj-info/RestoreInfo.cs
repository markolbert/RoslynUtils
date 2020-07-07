﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.VisualBasic;

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

            Sources = GetProperty<List<string>>(restoreInfo,"sources", optional: true);
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
    }
}