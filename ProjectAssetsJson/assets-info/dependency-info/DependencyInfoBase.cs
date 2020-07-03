using J4JSoftware.Logging;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    public class DependencyInfoBase : ConfigurationBase
    {
        protected DependencyInfoBase( IJ4JLogger logger )
            : base( logger )
        {
        }

        public string Assembly { get; set; }
    }
}