using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class TargetInfo : ProjectAssetsBase
    {
        private readonly TargetFramework _tgtFW;

#pragma warning disable 8618
        public TargetInfo( 
#pragma warning restore 8618
            string text, 
            ExpandoObject tgtInfoCollection, 
            Func<IJ4JLogger> loggerFactory )
            : base( loggerFactory )
        {
            if( TargetFramework.Create( text, TargetFrameworkTextStyle.ExplicitVersion, out var tgtFW ) )
                _tgtFW = tgtFW!;
            else
                throw ProjectAssetsException.CreateAndLog(
                    $"Couldn't create a {nameof( TargetFramework )} from property '{text}'",
                    this.GetType(),
                    Logger );

            CreatePackageList( tgtInfoCollection );
        }

        private void CreatePackageList( ExpandoObject tgtInfoCollection )
        {
            Packages.Clear();

            foreach (var kvp in tgtInfoCollection )
            {
                if( kvp.Value is ExpandoObject tgtContainer )
                    Packages.Add( new ReferenceInfo( kvp.Key, tgtContainer, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        $"Missing property '{kvp.Key}'",
                        this.GetType(),
                        Logger);
            }
        }

        public CSharpFramework Target => _tgtFW.Framework;
        public SemanticVersion Version => _tgtFW.Version;
        public List<ReferenceInfo> Packages { get; } = new List<ReferenceInfo>();
    }
}