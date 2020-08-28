using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeParameterConfigurator))]
    public class MethodTypeParameter : TypeParameterUsage
    {
        public int ReferencingMethodID { get; set; }

        public Method ReferencingMethod { get; set; }
    }

    internal class MethodTypeParameterConfigurator : EntityConfigurator<MethodTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<MethodTypeParameter> builder)
        {
            builder.HasOne(x => x.ReferencingMethod)
                .WithMany(x => x.TypeParameterReferences)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.ReferencingMethodID);

            builder.HasIndex( x => new { x.ReferencingMethodID, x.TypeParameterID } )
                .IsUnique();
        }
    }
}