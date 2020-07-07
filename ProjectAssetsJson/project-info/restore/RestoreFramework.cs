using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class RestoreFramework : ProjectAssetsBase
    {
        private readonly TargetFramework _tgtFW;

        public RestoreFramework(
            string text,
            ExpandoObject rfInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            _tgtFW = GetTargetFramework(text, TargetFrameworkTextStyle.Simple);

            CreateProjectReferences( GetProperty<ExpandoObject>( rfInfo, "projectReferences" ) );
        }

        private void CreateProjectReferences( ExpandoObject prefInfo )
        {
            foreach( var kvp in prefInfo )
            {
                if( kvp.Value is ExpandoObject projRefContainer )
                    ProjectReferences.Add( new ProjectReference( kvp.Key, projRefContainer, LoggerFactory ) );
                else
                    throw ProjectAssetsException.CreateAndLog(
                        "Project reference item is not an ExpandoObject",
                        this.GetType(),
                        Logger);
            }
        }

        public CSharpFramework TargetFramework => _tgtFW.Framework;
        public SemanticVersion TargetVersion => _tgtFW.Version;
        public List<ProjectReference> ProjectReferences { get; } = new List<ProjectReference>();
    }
}
