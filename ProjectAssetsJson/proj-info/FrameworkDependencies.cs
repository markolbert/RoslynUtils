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
            IJ4JLogger logger
        )
            : base( logger )
        {
            _depListCreator = depListCreator ?? throw new NullReferenceException( nameof(depListCreator) );
            _fwlCreator = fwlCreator ?? throw new NullReferenceException( nameof(fwlCreator) );
        }

        public List<DependencyList> Dependencies { get; } = new List<DependencyList>();
        public List<TargetFramework> Imports { get; } = new List<TargetFramework>();
        public bool AssetTargetFallback { get; set; }
        public bool Warn { get; set; }
        public List<FrameworkLibraryReference> FrameworkLibraryReferences { get; } = new List<FrameworkLibraryReference>();
        public string RuntimeIdentifierGraphPath { get; set; } = string.Empty;

        public override bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !base.Initialize( rawName, container, context ) )
                return false;

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework( rawName, out var tgtFramework, Logger ) )
                return false;

            var okay = GetProperty<ExpandoObject>( container, "dependencies", context, out var depContainer );
            okay &= GetProperty<List<string>>( container, "imports", context, out var importTexts );
            okay &= GetProperty<ExpandoObject>( container, "frameworkReferences", context, out var fwContainer );
            okay &= GetProperty<bool>( container, "assetTargetFallback", context, out var fallback );
            okay &= GetProperty<bool>( container, "warn", context, out var warn );
            okay &= GetProperty<string>( container, "runtimeIdentifierGraphPath", context, out var rtGraph );

            if( !okay ) return false;

            okay = LoadFromContainer<DependencyList, ExpandoObject>( depContainer, _depListCreator, context,
                out var depList );
            okay &= LoadFromContainer<FrameworkLibraryReference, ExpandoObject>( fwContainer, _fwlCreator, context,
                out var fwList );

            if( !okay ) return false;

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

            Dependencies.Clear();
            Dependencies.AddRange(depList!);

            Imports.Clear();
            Imports.AddRange(imports!);

            AssetTargetFallback = fallback;
            Warn = warn;

            FrameworkLibraryReferences.Clear();
            FrameworkLibraryReferences.AddRange(fwList!);

            RuntimeIdentifierGraphPath = rtGraph;

            return true;
        }
    }
}