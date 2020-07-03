using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class TargetInfo : ConfigurationBase, IInitializeFromNamed<ExpandoObject>
    {
        private readonly Func<ReferenceInfo> _refCreator;

        public TargetInfo(
            Func<ReferenceInfo> refCreator,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _refCreator = refCreator;
        }

        public CSharpFramework Target { get; set; }
        public SemanticVersion Version { get; set; } = new SemanticVersion( 0, 0, 0 );
        public List<ReferenceInfo> Packages { get; } = new List<ReferenceInfo>();

        public bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !TargetFramework.CreateTargetFramework( rawName, out var tgtFramework, Logger ) )
                return false;

            Target = tgtFramework.Framework;
            Version = tgtFramework.Version;

            Packages.Clear();

            var retVal = true;

            foreach( var kvp in container )
            {
                var newItem = _refCreator();

                if( kvp.Value is ExpandoObject childContainer )
                {
                    if( newItem.Initialize( kvp.Key, childContainer, context ) )
                        Packages.Add( newItem );
                    else
                        retVal = false;
                }
                else
                {
                    Logger.Error<string, string>( "{0} property is not a {1}", kvp.Key, nameof(ExpandoObject) );

                    retVal = false;
                }
            }

            return retVal;
        }
    }
}