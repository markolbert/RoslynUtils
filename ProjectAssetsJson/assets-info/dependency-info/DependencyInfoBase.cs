using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class DependencyInfoBase : ProjectAssetsBase
    {
        protected DependencyInfoBase( IJ4JLogger<DependencyInfoBase> logger )
            : base( logger )
        {
        }

        public string Assembly { get; set; }
    }
}