using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkReferences : FrameworkBase
    {
        private readonly Func<ProjectReference> _refCreator;

        public FrameworkReferences( 
            Func<ProjectReference> refCreator,
            IJ4JLogger logger
            ) 
            : base( logger )
        {
            _refCreator = refCreator;
        }

        public List<ProjectReference> ProjectReferences { get; } = new List<ProjectReference>();

        public override bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !base.Initialize( rawName, container, context ) )
                return false;

            if( !Roslyn.TargetFramework.Create(rawName, TargetFrameworkTextStyle.Simple, out var tgtFramework) )
                return false;

            if( !container.GetProperty<ExpandoObject>( "projectReferences", out var refContainer, optional: true ) )
                return false;

            refContainer.LoadFromContainer<ProjectReference, ExpandoObject>( _refCreator, context, out var refList, containerCanBeNull: true );

            TargetFramework = tgtFramework!.Framework;
            TargetVersion = tgtFramework.Version;

            ProjectReferences.Clear();

            if( refList != null )
                ProjectReferences.AddRange(refList);

            return true;
        }
    }
}