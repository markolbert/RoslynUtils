using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ReferenceInfo : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        private readonly Func<DependencyInfo> _diCreator;

        public ReferenceInfo(
            Func<DependencyInfo> diCreator,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _diCreator = diCreator ?? throw new NullReferenceException( nameof(diCreator) );
        }

        public string Assembly { get; set; } = string.Empty;
        public SemanticVersion Version { get; set; } = new SemanticVersion( 0, 0, 0 );
        public ReferenceType Type { get; set; }
        public List<string> Compile { get; } = new List<string>();
        public List<string> Runtime { get; } = new List<string>();
        public List<DependencyInfo> Dependencies { get; } = new List<DependencyInfo>();

        public bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !VersionedText.CreateVersionedText(rawName, out var verText, Logger) )
                return false;

            Assembly = verText.TextComponent;
            Version = verText.Version;
            Dependencies.Clear();

            var asDict = (IDictionary<string, object>) container;

            // dependencies are optional
            if( !asDict.ContainsKey( "dependencies" ) )
                return true;

            var depDict = asDict[ "dependencies" ] as ExpandoObject;

            if( depDict == null )
            {
                Logger.Error(
                    $"{nameof( container )} does not have a 'dependencies' property which is an {nameof( ExpandoObject )}" );

                return false;
            }

            var retVal = true;

            foreach( var kvp in depDict )
            {
                if( kvp.Value is string versionText )
                {
                    if( VersionedText.TryParseSemanticVersion( versionText, out var version, Logger ) )
                    {
                        var newItem = _diCreator();

                        newItem.Assembly = kvp.Key;
                        newItem.Version = version!;

                        Dependencies.Add( newItem );
                    }
                    else retVal = false;
                }
            }

            return retVal;
        }
    }
}