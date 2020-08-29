using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeDefinitionTypeParameterConfigurator))]
    public class TypeDefinitionTypeParameter : TypeParameterUsage
    {
        public int ReferencingTypeID { get; set; }

        public FixedTypeDb ReferencingType { get; set; }
    }

    internal class TypeDefinitionTypeParameterConfigurator : EntityConfigurator<TypeDefinitionTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeDefinitionTypeParameter> builder)
        {
            builder.HasOne(x => x.ReferencingType)
                .WithMany(x => x.TypeParameterReferences)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.ReferencingTypeID);

            builder.HasIndex( x => new { x.ReferencingTypeID, x.TypeParameterID } )
                .IsUnique();
        }
    }
}