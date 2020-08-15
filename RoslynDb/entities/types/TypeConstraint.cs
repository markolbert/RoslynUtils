using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(GenericParameterTypeConstraintConfigurator))]
    public class TypeConstraint : ISynchronized
    {
        public int TypeParameterID { get; set; }
        public TypeParameter TypeParameter { get; set; }
        public int ConstrainingTypeID { get; set; }
        public TypeDefinition ConstrainingType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class GenericParameterTypeConstraintConfigurator : EntityConfigurator<TypeConstraint>
    {
        protected override void Configure(EntityTypeBuilder<TypeConstraint> builder)
        {
            builder.HasKey( x => new { x.ConstrainingTypeID, x.TypeParameterID} );

            builder.HasOne( x => x.ConstrainingType )
                .WithMany( x => x.TypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ConstrainingTypeID );

            builder.HasOne( x => x.TypeParameter )
                .WithMany( x => x.TypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeParameterID );

        }
    }
}