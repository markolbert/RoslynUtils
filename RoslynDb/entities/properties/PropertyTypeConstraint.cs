using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeConstraintConfigurator))]
    public class PropertyTypeConstraint
    {
        public int GenericPropertyParameterID { get; set; }
        public GenericPropertyParameter GenericPropertyParameter { get; set; }
        public int ConstrainingTypeID { get; set; }
        public TypeDefinition ConstrainingType { get; set; }
    }

    internal class PropertyTypeConstraintConfigurator : EntityConfigurator<PropertyTypeConstraint>
    {
        protected override void Configure(EntityTypeBuilder<PropertyTypeConstraint> builder)
        {
            builder.HasKey( x => new { x.ConstrainingTypeID, TypeParameterID = x.GenericPropertyParameterID} );

            builder.HasOne( x => x.ConstrainingType )
                .WithMany( x => x.PropertyTypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ConstrainingTypeID );

            builder.HasOne( x => x.GenericPropertyParameter )
                .WithMany( x => x.TypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.GenericPropertyParameterID );
        }
    }
}