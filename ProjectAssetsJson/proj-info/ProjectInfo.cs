using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectInfo : ConfigurationBase
    {
        private readonly Func<RestoreInfo> _restoreCreator;
        private readonly Func<FrameworkReferences> _fwCreator;
        private readonly Func<WarningProperty> _wpCreator;

        public ProjectInfo( 
            Func<RestoreInfo> restoreCreator,
            Func<FrameworkReferences> fwCreator,
            Func<WarningProperty> wpCreator,
            IJ4JLogger logger
            )
            : base( logger )
        {
            _restoreCreator = restoreCreator;
            _fwCreator = fwCreator;
            _wpCreator = wpCreator;
        }

        public SemanticVersion Version { get; set; } = new SemanticVersion( 0, 0, 0 );
        public RestoreInfo? Restore { get; private set; }

        public bool IsNetCoreApplication =>
            Frameworks?.All( fw => fw.TargetFramework == CSharpFramework.NetCoreApp ) ?? false;

        public List<FrameworkReferences> Frameworks { get; } = new List<FrameworkReferences>();
        public List<WarningProperty> WarningProperties { get; } = new List<WarningProperty>();

        public bool Initialize( ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( container, context ) )
                return false;

            var okay = container.GetProperty<string>( "version", out var versionText );
            okay &= container.GetProperty<ExpandoObject>( "restore", out var restoreContainer );
            okay &= container.GetProperty<ExpandoObject>( "frameworks", out var frameContainer );
            okay &= container.GetProperty<ExpandoObject>( "warningProperties", out var warnContainer,
                optional : true );

            if( !okay ) return false;

            if( !Versioning.GetSemanticVersion( versionText, out var version) )
                return false;

            var restore = _restoreCreator();
            if( !restore.Initialize( restoreContainer, context ) )
                return false;

            if( !frameContainer.LoadFromContainer<FrameworkReferences, ExpandoObject>( _fwCreator, context,
                out var fwList ) )
                return false;

            warnContainer.LoadFromContainer<WarningProperty, List<string>>( _wpCreator, context, out var warnList,
                containerCanBeNull : true );

            Version = version!;
            Restore = restore;

            Frameworks.Clear();
            Frameworks.AddRange(fwList!);

            WarningProperties.Clear();

            if( warnList != null )
                WarningProperties.AddRange(warnList);

            return true;
        }
    }
}
