using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( AssemblyNamespaceConfigurator ) )]
    public class AssemblyNamespace
    {
        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; } = null!;

        public int NamespaceID { get; set; }
        public Namespace Namespace { get; set; } = null!;
    }

    internal class AssemblyNamespaceConfigurator : EntityConfigurator<AssemblyNamespace>
    {
        protected override void Configure( EntityTypeBuilder<AssemblyNamespace> builder )
        {
            builder.HasKey( x => new { x.AssemblyID, x.NamespaceID } );
        }
    }
}
