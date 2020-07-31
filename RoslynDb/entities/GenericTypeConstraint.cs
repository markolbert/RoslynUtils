using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(GenericParameterTypeConstraintConfigurator))]
    public class GenericTypeConstraint
    {
        public int NamedTypeID { get; set; }
        public NamedType NamedType { get; set; }
        public int GenericParameterID { get; set; }
        public GenericParameter GenericParameter { get; set; }
    }

    internal class GenericParameterTypeConstraintConfigurator : EntityConfigurator<GenericTypeConstraint>
    {
        protected override void Configure(EntityTypeBuilder<GenericTypeConstraint> builder)
        {
            builder.HasKey( x => new { x.NamedTypeID, x.GenericParameterID } );

            builder.HasOne( x => x.NamedType )
                .WithMany( x => x.GenericConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.NamedTypeID );

            builder.HasOne( x => x.GenericParameter )
                .WithMany( x => x.TypeConstraints )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.GenericParameterID );

        }
    }
}