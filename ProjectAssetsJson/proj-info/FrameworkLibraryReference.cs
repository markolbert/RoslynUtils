using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkLibraryReference : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        public FrameworkLibraryReference( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public string LibraryName { get; set; }
        public string PrivateAssets { get; set; }

        public bool Initialize( string rawName, ExpandoObject container )
        {
            if( !ValidateInitializationArguments( rawName, container ) )
                return false;

            if( !GetProperty<string>( container, "privateAssets", out var privateAssets ) )
                return false;

            LibraryName = rawName;
            PrivateAssets = privateAssets;

            return true;
        }
    }
}