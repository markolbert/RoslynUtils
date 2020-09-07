using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeImplementationConfigurator))]
    public class TypeAncestorDb : ISynchronized
    {
        public int ChildTypeID { get; set; }
        public TypeDb ChildType { get; set; }
        public int AncestorTypeID { get; set; }
        public TypeDb AncestorType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeAncestorDb>
    {
        protected override void Configure(EntityTypeBuilder<TypeAncestorDb> builder)
        {
            builder.HasOne( x => x.AncestorType )
                .WithMany( x => x.AncestorTypes )
                .HasForeignKey( x => x.AncestorTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasKey( x => new { x.ChildTypeID, x.AncestorTypeID } );
        }
    }

}
