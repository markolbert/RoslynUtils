using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeParameterConfigurator))]
    public class TypeParameter : ISynchronized
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public TypeParameterConstraint Constraints { get; set; }

        public int ContainingTypeID { get; set; }
        public TypeDefinition ContainingType { get; set; }

        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class TypeParameterConfigurator : EntityConfigurator<TypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameter> builder)
        {
            builder.HasMany( x => x.TypeConstraints )
                .WithOne( x => x.TypeParameter )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeParameterID );

            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.TypeParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ContainingTypeID );

            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<TypeParameterConstraint, int>());

            builder.Property( x => x.Name )
                .IsRequired();
        }
    }

}
