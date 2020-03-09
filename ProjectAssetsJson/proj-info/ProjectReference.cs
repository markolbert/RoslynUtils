using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectReference : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        public ProjectReference( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }

        public bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !GetProperty<string>( container, "projectPath", context, out var path ) )
                return false;

            ProjectName = rawName;
            ProjectPath = path;

            return true;
        }
    }
}