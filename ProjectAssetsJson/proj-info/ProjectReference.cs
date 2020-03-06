using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectReference : ProjectAssetsBase, ILoadFromNamed<ExpandoObject>
    {
        public ProjectReference( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }

        public bool Load( string rawName, ExpandoObject container )
        {
            if( !ValidateLoadArguments( rawName, container ) )
                return false;

            if( !GetProperty<string>( container, "projectPath", out var path ) )
                return false;

            ProjectName = rawName;
            ProjectPath = path;

            return true;
        }
    }
}