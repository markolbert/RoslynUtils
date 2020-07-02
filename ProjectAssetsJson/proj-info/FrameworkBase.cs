using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class FrameworkBase : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        protected FrameworkBase( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        public CSharpFramework TargetFramework { get; protected set; }
        public SemanticVersion TargetVersion { get; set; } = new SemanticVersion( 0, 0, 0 );

        public virtual bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            return true;
        }
    }
}