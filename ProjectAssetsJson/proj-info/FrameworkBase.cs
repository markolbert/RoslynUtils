using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class FrameworkBase : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        protected FrameworkBase( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public CSharpFrameworks TargetFramework { get; set; }
        public SemanticVersion TargetVersion { get; set; }

        public virtual bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            return true;
        }
    }
}