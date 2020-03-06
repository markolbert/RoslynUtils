using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class FrameworkBase : ProjectAssetsBase, ILoadFromNamed<ExpandoObject>
    {
        protected FrameworkBase( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public CSharpFrameworks TargetFramework { get; set; }
        public SemanticVersion TargetVersion { get; set; }

        public virtual bool Load( string rawName, ExpandoObject container )
        {
            if( !ValidateLoadArguments( rawName, container ) )
                return false;

            return true;
        }
    }
}