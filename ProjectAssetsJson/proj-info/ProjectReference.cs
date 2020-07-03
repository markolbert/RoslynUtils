using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectReference : ConfigurationBase, IInitializeFromNamed<ExpandoObject>
    {
        public ProjectReference( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        public string ProjectName { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;

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