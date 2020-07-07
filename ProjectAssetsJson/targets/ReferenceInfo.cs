using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public class ReferenceInfo : ProjectAssetsBase
    {
        public const string DependencyKey = "dependencies";

#pragma warning disable 8618
        public ReferenceInfo(
#pragma warning restore 8618
            string text,
            ExpandoObject tgtInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            if( VersionedText.Create( text, out var verText ) )
            {
                Assembly = verText!.TextComponent;
                Version = verText.Version;
            }
            else
                throw ProjectAssetsException.CreateAndLog(
                    $"Couldn't create a {nameof( VersionedText )}",
                    this.GetType(),
                    Logger );

            Type = GetEnum<ReferenceType>( tgtInfo, "type" );

            CreateDependencies(tgtInfo);
        }

        private void CreateDependencies( ExpandoObject tgtInfo )
        {
            Dependencies.Clear();

            // dependencies are optional
            var tgtDict = (IDictionary<string, object>) tgtInfo;
            if( !tgtDict.ContainsKey( DependencyKey ) )
                return;

            //...but they must be present in the form of an ExpandoObject
            if( tgtDict[ DependencyKey ] is ExpandoObject depContainer )
            {
                foreach( var kvp in depContainer )
                {
                    if( kvp.Value is string versionText )
                        Dependencies.Add(
                            new TargetDependency( kvp.Key, GetSemanticVersion( versionText ),
                                LoggerFactory ) );
                    else
                        throw ProjectAssetsException.CreateAndLog(
                            $"'{kvp.Value}' is not a version string",
                            this.GetType(),
                            Logger );
                }
            }
            else
                throw ProjectAssetsException.CreateAndLog(
                    $"'{DependencyKey}' clause for assembly {Assembly} is not an ExpandoObject",
                    this.GetType(),
                    Logger );
        }

        public string Assembly { get; }
        public SemanticVersion Version { get; }
        public ReferenceType Type { get; }
        public List<string> Compile { get; } = new List<string>();
        public List<string> Runtime { get; } = new List<string>(); 
        public List<TargetDependency> Dependencies { get; } = new List<TargetDependency>();
    }
}