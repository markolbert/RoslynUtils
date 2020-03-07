using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectInfo : ProjectAssetsBase
    {
        private readonly Func<RestoreInfo> _restoreCreator;
        private readonly Func<FrameworkReferences> _fwCreator;
        private readonly Func<WarningProperty> _wpCreator;

        public ProjectInfo( 
            Func<RestoreInfo> restoreCreator,
            Func<FrameworkReferences> fwCreator,
            Func<WarningProperty> wpCreator,
            IJ4JLogger<ProjectInfo> logger 
            )
            : base( logger )
        {
            _restoreCreator = restoreCreator ?? throw new NullReferenceException( nameof(restoreCreator) );
            _fwCreator = fwCreator ?? throw new NullReferenceException( nameof(fwCreator) );
            _wpCreator = wpCreator ?? throw new NullReferenceException( nameof(wpCreator) );
        }

        public SemanticVersion Version { get; set; }
        public RestoreInfo Restore { get; set; }
        public List<FrameworkReferences> Frameworks { get; set; }
        public List<WarningProperty> WarningProperties { get; set; }

        public bool Initialize( ExpandoObject container )
        {
            if( !ValidateInitializationArguments( container ) )
                return false;

            if( !GetProperty<string>( container, "version", out var versionText )
                || !GetProperty<ExpandoObject>( container, "restore", out var restoreContainer )
                || !GetProperty<ExpandoObject>( container, "frameworks", out var frameContainer )
                || !GetProperty<ExpandoObject>( container, "warningProperties", out var warnContainer,
                    optional : true ) )
                return false;

            if( !VersionedText.TryParseSemanticVersion( versionText, out var version, Logger ) )
                return false;

            var restore = _restoreCreator();
            if( !restore.Initialize( restoreContainer ) )
                return false;

            if( !LoadFromContainer<FrameworkReferences, ExpandoObject>( frameContainer, _fwCreator, out var fwList ) )
                return false;
            
            LoadFromContainer<WarningProperty, List<string>>( warnContainer, _wpCreator, out var warnList, containerCanBeNull: true );

            Version = version;
            Restore = restore;
            Frameworks = fwList;
            WarningProperties = warnList;

            return true;
        }
    }
}
