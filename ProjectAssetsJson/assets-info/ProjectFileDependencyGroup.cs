using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFileDependencyGroup : ProjectAssetsBase, IInitializeFromNamed<List<string>>
    {
        private readonly Func<RestrictedDependencyInfo> _depCreator;

        public ProjectFileDependencyGroup(
            Func<RestrictedDependencyInfo> depCreator,
            IJ4JLogger<ProjectFileDependencyGroup> logger
        )
            : base( logger )
        {
            _depCreator = depCreator ?? throw new NullReferenceException( nameof(depCreator) );
        }

        public CSharpFrameworks TargetFramework { get; set; }
        public SemanticVersion TargetVersion { get; set; }
        public List<RestrictedDependencyInfo> Dependencies { get; set; }

        public bool Initialize( string rawName, List<string> container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( String.IsNullOrEmpty( rawName ) )
            {
                Logger.Error( $"Undefined or empty {nameof( rawName )}" );

                return false;
            }

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework(rawName, out var tgtFramework, Logger ) )
                return false;

            TargetFramework = tgtFramework.Framework;
            TargetVersion = tgtFramework.Version;
            Dependencies = new List<RestrictedDependencyInfo>();

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
