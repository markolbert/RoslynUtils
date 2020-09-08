using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeParametricTypeDbConfigurator ) )]
    public class TypeParametricTypeDb : ParametricTypeDb
    {
        public int? ContainingTypeID { get; set; }
        public TypeDb? ContainingType { get; set; }
    }

    internal class TypeParametricTypeDbConfigurator : EntityConfigurator<TypeParametricTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<TypeParametricTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingTypeID );
        }
    }
}
