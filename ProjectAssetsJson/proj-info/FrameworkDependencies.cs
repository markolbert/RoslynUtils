using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkDependencies : FrameworkBase
    {
        private readonly Func<DependencyList> _depListCreator;
        private readonly Func<FrameworkLibraryReference> _fwlCreator;

        public FrameworkDependencies(
            Func<DependencyList> depListCreator,
            Func<FrameworkLibraryReference> fwlCreator,
            IJ4JLogger<ProjectAssetsBase> logger
        )
            : base( logger )
        {
            _depListCreator = depListCreator ?? throw new NullReferenceException( nameof(depListCreator) );
            _fwlCreator = fwlCreator ?? throw new NullReferenceException( nameof(fwlCreator) );
        }

        public List<DependencyList> Dependencies { get; set; }
        public List<TargetFramework> Imports { get; set; }
        public bool AssetTargetFallback { get; set; }
        public bool Warn { get; set; }
        public List<FrameworkLibraryReference> FrameworkLibraryReferences { get; set; }
        public string RuntimeIdentifierGraphPath { get; set; }

        public override bool Load( string rawName, ExpandoObject container )
        {
            if( !ValidateLoadArguments( rawName, container ) )
                return false;

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework(rawName, out var tgtFramework, Logger ))
                return false;

            if( !GetProperty<ExpandoObject>( container, "dependencies", out var depContainer )
                || !GetProperty<List<string>>( container, "imports", out var importTexts )
                || !GetProperty<ExpandoObject>( container, "frameworkReferences", out var fwContainer )
                || !GetProperty<bool>( container, "assetTargetFallback", out var fallback )
                || !GetProperty<bool>( container, "warn", out var warn )
                || !GetProperty<string>( container, "runtimeIdentifierGraphPath", out var rtGraph )
            )
                return false;

            if( !LoadFromContainer<DependencyList, ExpandoObject>( depContainer, _depListCreator, out var depList )
                || !LoadFromContainer<FrameworkLibraryReference, ExpandoObject>( fwContainer, _fwlCreator,
                    out var fwList ) )
                return false;

            var importsValid = true;

            var imports = importTexts.Select( it =>
                {
                    if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework( it, out var retVal, Logger ) )
                    {
                        importsValid = false;

                        return null;
                    }

                    return retVal;
                } )
                .ToList();

            if( !importsValid )
                return false;

            TargetFramework = tgtFramework.Framework;
            TargetVersion = tgtFramework.Version;
            Dependencies = depList;
            Imports = imports;
            AssetTargetFallback = fallback;
            Warn = warn;
            FrameworkLibraryReferences = fwList;
            RuntimeIdentifierGraphPath = rtGraph;

            return true;
        }
    }
}