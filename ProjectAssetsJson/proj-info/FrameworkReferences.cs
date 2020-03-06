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
            IJ4JLogger<ProjectAssetsBase> logger
            ) 
            : base( logger )
        {
            _refCreator = refCreator ?? throw new NullReferenceException( nameof(refCreator) );
        }

        public List<ProjectReference> ProjectReferences { get; set; }

        public override bool Load( string rawName, ExpandoObject container )
        {
            if( !base.Load( rawName, container ) )
                return false;

            if( !J4JSoftware.Roslyn.TargetFramework.CreateTargetFramework(rawName, out var tgtFramework, Logger ) )
                return false;

            if( !GetProperty<ExpandoObject>( container, "projectReferences", out var refContainer, optional: true ) )
                return false;

            LoadFromContainer<ProjectReference, ExpandoObject>( refContainer, _refCreator, out var refList, containerCanBeNull: true );

            TargetFramework = tgtFramework.Framework;
            TargetVersion = tgtFramework.Version;
            ProjectReferences = refList;

            return true;
        }
    }
}