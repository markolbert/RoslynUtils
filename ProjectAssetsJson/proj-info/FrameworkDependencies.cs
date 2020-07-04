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
            _depListCreator = depListCreator;
            _fwlCreator = fwlCreator;
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

            if( !Roslyn.TargetFramework.Create( rawName, TargetFrameworkTextStyle.Simple, out var tgtFramework ) )
                return false;

            var okay = container.GetProperty<ExpandoObject>( "dependencies", out var depContainer );
            okay &= container.GetProperty<List<string>>( "imports", out var importTexts );
            okay &= container.GetProperty<ExpandoObject>( "frameworkReferences", out var fwContainer );
            okay &= container.GetProperty<bool>( "assetTargetFallback", out var fallback );
            okay &= container.GetProperty<bool>( "warn", out var warn );
            okay &= container.GetProperty<string>( "runtimeIdentifierGraphPath", out var rtGraph );

            if( !okay ) return false;

            okay = depContainer.LoadFromContainer<DependencyList, ExpandoObject>( _depListCreator, context,
                out var depList );
            okay &= fwContainer.LoadFromContainer<FrameworkLibraryReference, ExpandoObject>( _fwlCreator, context,
                out var fwList );

            if( !okay ) return false;

            var importsValid = true;

            var imports = importTexts.Select( it =>
                {
                    if( !Roslyn.TargetFramework.Create( it, TargetFrameworkTextStyle.Simple, out var retVal) )
                    {
                        importsValid = false;

                        return null;
                    }

                    return retVal;
                } )
                .ToList();

            if( !importsValid )
                return false;

            TargetFramework = tgtFramework!.Framework;
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