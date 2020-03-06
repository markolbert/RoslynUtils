using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFileDependencyGroup : ILoadFromNamed<List<string>>
    {
        private readonly Func<RestrictedDependencyInfo> _depCreator;
        private readonly IJ4JLogger<ProjectFileDependencyGroup> _logger;

        public ProjectFileDependencyGroup( 
            Func<RestrictedDependencyInfo> depCreator,
            IJ4JLogger<ProjectFileDependencyGroup> logger
            )
        {
            _depCreator = depCreator ?? throw new NullReferenceException( nameof(depCreator) );
            _logger = logger ?? throw new NullReferenceException( nameof(logger) );
        }

        public CSharpFrameworks TargetFramework { get; set; }
        public SemanticVersion TargetVersion { get; set; }
        public List<RestrictedDependencyInfo> Dependencies { get; set; }

        public bool Load( string rawName, List<string> container )
        {
            if( container == null )
            {
                _logger.Error( $"Undefined {nameof( container )}" );

                return false;
            }

            if( String.IsNullOrEmpty( rawName ) )
            {
                _logger.Error( $"Undefined or empty {nameof( rawName )}" );

                return false;
            }

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework(rawName, out var tgtFramework, _logger ) )
                return false;

            TargetFramework = tgtFramework.Framework;
            TargetVersion = tgtFramework.Version;
            Dependencies = new List<RestrictedDependencyInfo>();

            var retVal = true;

            foreach( var entry in container )
            {
                var newItem = _depCreator();

                if( newItem.Load( entry ) )
                    Dependencies.Add( newItem );
                else retVal = false;
            }

            return retVal;
        }
    }
}
