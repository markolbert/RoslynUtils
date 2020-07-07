using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkDependencies : FrameworkBase
    {
        public FrameworkDependencies(
            string text,
            ExpandoObject fwDepInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( text, loggerFactory )
        {
            AssetTargetFallback = GetProperty<bool>( fwDepInfo, "assetTargetFallback" );
            Warn = GetProperty<bool>( fwDepInfo, "warn" );
            RuntimeIdentifierGraphPath = GetProperty<string>( fwDepInfo, "runtimeIdentifierGraphPath" );

            var importTF = GetProperty<List<string>>( fwDepInfo, "imports" );
            Imports = importTF.Select( t =>
                {
                    if( !Roslyn.TargetFramework.Create( t, TargetFrameworkTextStyle.Simple, out var tgtFW ) )
                        throw new ArgumentException( $"Couldn't parse {t} to a {typeof(TargetFramework)}" );

                    return tgtFW!;
                } )
                .ToList();

            CreateDependencies( GetProperty<ExpandoObject>( fwDepInfo, "dependencies" ) );
            CreateFrameworkLibraryReferences( GetProperty<ExpandoObject>( fwDepInfo, "frameworkReferences" ) );
        }

        private void CreateDependencies( ExpandoObject depInfoColl )
        {
            foreach( var kvp in depInfoColl )
            {
                if( kvp.Value is ExpandoObject depInfo )
                    Dependencies.Add( new DependencyList( kvp.Key, depInfo, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        $"Couldn't create a {typeof( DependencyList )} from property '{kvp.Key}'",
                        this.GetType(),
                        Logger );
            }
        }

        private void CreateFrameworkLibraryReferences( ExpandoObject fwlrInfo )
        {
            foreach (var kvp in fwlrInfo)
            {
                if (kvp.Value is ExpandoObject fwlr)
                    FrameworkLibraryReferences.Add(new FrameworkLibraryReference(kvp.Key, fwlr, LoggerFactory));
                else
                    throw ProjectAssetsException.CreateAndLog(
                        $"Couldn't create a {typeof(FrameworkLibraryReference)} from property '{kvp.Key}'",
                        this.GetType(),
                        Logger);
            }
        }

        public List<DependencyList> Dependencies { get; } = new List<DependencyList>();
        public List<TargetFramework> Imports { get; }
        public bool AssetTargetFallback { get; }
        public bool Warn { get; }
        public List<FrameworkLibraryReference> FrameworkLibraryReferences { get; } = new List<FrameworkLibraryReference>();
        public string RuntimeIdentifierGraphPath { get; }
    }
}