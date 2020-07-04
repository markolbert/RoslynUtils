using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkLibraryReference : ConfigurationBase, IInitializeFromNamed<ExpandoObject>
    {
        public FrameworkLibraryReference( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        public string LibraryName { get; set; } = string.Empty;
        public string PrivateAssets { get; set; } = string.Empty;

        public bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !container.GetProperty<string>( "privateAssets", out var privateAssets ) )
                return false;

            LibraryName = rawName;
            PrivateAssets = privateAssets;

            return true;
        }
    }
}