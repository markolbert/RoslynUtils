using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( AssemblyNamespaceConfigurator ) )]
    public class AssemblyNamespaceDb
    {
        public int AssemblyID { get; set; }
        public AssemblyDb Assembly { get; set; } = null!;

        public int NamespaceID { get; set; }
        public NamespaceDb Namespace { get; set; } = null!;
    }

    internal class AssemblyNamespaceConfigurator : EntityConfigurator<AssemblyNamespaceDb>
    {
        protected override void Configure( EntityTypeBuilder<AssemblyNamespaceDb> builder )
        {
            builder.HasKey( x => new { x.AssemblyID, x.NamespaceID } );
        }
    }
}
