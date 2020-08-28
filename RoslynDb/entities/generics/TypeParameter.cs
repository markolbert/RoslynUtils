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
    public class TypeParameter
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public TypeParameterConstraint Constraints { get; set; }

        // list of types this type parameter must implement
        public List<TypeConstraint> TypeConstraints { get; set; }

        // list of TypeParameterUsages refering to this TypeParameter
        public List<TypeParameterUsage> References { get; set; }

        // list of method arguments referencing this type definition
        public List<TypeParameterMethodArgument> MethodArguments { get; set; }

    }

    internal class TypeParameterConfigurator : EntityConfigurator<TypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameter> builder)
        {
            builder.HasMany(x => x.TypeConstraints)
                .WithOne(x => x.TypeParameter)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.TypeParameterID);

            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<TypeParameterConstraint, int>());

            builder.Property( x => x.Name )
                .IsRequired();
        }
    }

}
