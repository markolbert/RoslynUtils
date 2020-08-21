using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeParameterBaseConfigurator))]
    public class TypeParameterBase : ISynchronized
    {
        protected TypeParameterBase()
        {
        }

        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }
        public TypeParameterConstraint Constraints { get; set; }

        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class TypeParameterBaseConfigurator : EntityConfigurator<TypeParameterBase>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameterBase> builder)
        {
            builder.HasMany( x => x.TypeConstraints )
                .WithOne( x => x.TypeParameterBase )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeParameterBaseID );

            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<TypeParameterConstraint, int>());

            builder.Property( x => x.Name )
                .IsRequired();
        }
    }

}
