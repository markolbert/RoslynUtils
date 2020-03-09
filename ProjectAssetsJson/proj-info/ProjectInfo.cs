using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        public bool IsNetCoreApplication =>
            Frameworks?.All( fw => fw.TargetFramework == CSharpFrameworks.NetCoreApp ) ?? false;

        public List<FrameworkReferences> Frameworks { get; set; }
        public List<WarningProperty> WarningProperties { get; set; }

        public bool Initialize( ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( container, context ) )
                return false;

            var okay = GetProperty<string>( container, "version", context, out var versionText );
            okay &= GetProperty<ExpandoObject>( container, "restore", context, out var restoreContainer );
            okay &= GetProperty<ExpandoObject>( container, "frameworks", context, out var frameContainer );
            okay &= GetProperty<ExpandoObject>( container, "warningProperties", context, out var warnContainer,
                optional : true );

            if( !okay ) return false;

            if( !VersionedText.TryParseSemanticVersion( versionText, out var version, Logger ) )
                return false;

            var restore = _restoreCreator();
            if( !restore.Initialize( restoreContainer, context ) )
                return false;

            if( !LoadFromContainer<FrameworkReferences, ExpandoObject>( frameContainer, _fwCreator, context,
                out var fwList ) )
                return false;

            LoadFromContainer<WarningProperty, List<string>>( warnContainer, _wpCreator, context, out var warnList,
                containerCanBeNull : true );

            Version = version;
            Restore = restore;
            Frameworks = fwList;
            WarningProperties = warnList;

            return true;
        }
    }
}
