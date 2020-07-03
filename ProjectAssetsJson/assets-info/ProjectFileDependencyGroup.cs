﻿using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFileDependencyGroup : ConfigurationBase, IInitializeFromNamed<List<string>>
    {
        private readonly Func<RestrictedDependencyInfo> _depCreator;

        public ProjectFileDependencyGroup(
            Func<RestrictedDependencyInfo> depCreator,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _depCreator = depCreator;
        }

        public CSharpFramework TargetFramework { get; set; }
        public SemanticVersion TargetVersion { get; set; } = new SemanticVersion( 0, 0, 0 );
        public List<RestrictedDependencyInfo> Dependencies { get; } = new List<RestrictedDependencyInfo>();

        public bool Initialize( string rawName, List<string> container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( string.IsNullOrEmpty( rawName ) )
            {
                Logger.Error<string>( "Undefined or empty {0}", nameof(rawName) );

                return false;
            }

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework(rawName, out var tgtFramework, Logger ) )
                return false;

            TargetFramework = tgtFramework.Framework;
            TargetVersion = tgtFramework.Version;
            Dependencies.Clear();

            var retVal = true;

            foreach( var entry in container )
            {
                var newItem = _depCreator();

                if( newItem.Initialize( entry ) )
                    Dependencies.Add( newItem );
                else retVal = false;
            }

            return retVal;
        }
    }
}
