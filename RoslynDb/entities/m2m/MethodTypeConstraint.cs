using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeConstraintConfigurator))]
    public class MethodTypeConstraint
    {
        public int GenericMethodArgumentID { get; set; }
        public GenericMethodArgument GenericMethodArgument { get; set; }
        public int ConstrainingTypeID { get; set; }
        public TypeDefinition ConstrainingType { get; set; }
    }

    internal class MethodTypeConstraintConfigurator : EntityConfigurator<MethodTypeConstraint>
    {
        protected override void Configure(EntityTypeBuilder<MethodTypeConstraint> builder)
        {
            builder.HasKey( x => new { x.ConstrainingTypeID, TypeParameterID = x.GenericMethodArgumentID} );

            builder.HasOne( x => x.ConstrainingType )
                .WithMany( x => x.MethodTypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ConstrainingTypeID );

            builder.HasOne( x => x.GenericMethodArgument )
                .WithMany( x => x.TypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.GenericMethodArgumentID );
        }
    }
}