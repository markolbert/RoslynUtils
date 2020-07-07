using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFramework : ProjectAssetsBase
    {
        private readonly TargetFramework _tgtFW;

        public ProjectFramework( 
                string text,
            ExpandoObject fwInfo,
            Func<IJ4JLogger> loggerFactory
            ) 
            : base( loggerFactory )
        {
            _tgtFW = GetTargetFramework(text, TargetFrameworkTextStyle.Simple);

            CreateDependencies( GetProperty<ExpandoObject>( fwInfo, "dependencies" ) );
            CreateFrameworkLibraryReferences(GetProperty<ExpandoObject>(fwInfo, "frameworkReferences"));

            Imports = GetProperty<List<string>>( fwInfo, "imports" )
                .Select( t => GetTargetFramework( t, TargetFrameworkTextStyle.Simple ) )
                .ToList();

            AssetTargetFallback = GetProperty<bool>( fwInfo, "assetTargetFallback" );
            Warn = GetProperty<bool>(fwInfo, "warn");
            RuntimeIdentifierGraphPath = GetProperty<string>(fwInfo, "runtimeIdentifierGraphPath");
        }

        private void CreateDependencies( ExpandoObject refInfo )
        {
            foreach( var kvp in refInfo )
            {
                if( kvp.Value is ExpandoObject detail )
                    Dependencies.Add( new ProjectFrameworkDependency( kvp.Key, detail, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Project reference item is not an ExpandoObject",
                        this.GetType(),
                        Logger );
            }
        }

        private void CreateFrameworkLibraryReferences( ExpandoObject fwlrInfo )
        {
            foreach (var kvp in fwlrInfo)
            {
                if (kvp.Value is ExpandoObject detail)
                    References.Add(new FrameworkLibraryReference(kvp.Key, detail, LoggerFactory));
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Framework library reference item is not an ExpandoObject",
                        this.GetType(),
                        Logger);
            }
        }

        public CSharpFramework TargetFramework => _tgtFW.Framework;
        public SemanticVersion TargetVersion => _tgtFW.Version;
        public List<ProjectFrameworkDependency> Dependencies { get; } = new List<ProjectFrameworkDependency>();
        public List<TargetFramework> Imports { get; }
        public bool AssetTargetFallback { get; }
        public bool Warn { get; }
        public string RuntimeIdentifierGraphPath { get; }
        public List<FrameworkLibraryReference> References { get; } = new List<FrameworkLibraryReference>();
    }
}