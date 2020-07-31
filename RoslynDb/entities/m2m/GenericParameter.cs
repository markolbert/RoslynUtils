using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(GenericParameterConfigurator))]
    public class GenericParameter
    {
        protected GenericParameter()
        {
        }

        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public int ParameterIndex { get; set; }
        public GenericConstraint Constraints { get; set; }

        public List<GenericTypeConstraint> TypeConstraints { get; set; }
    }

    internal class GenericParameterConfigurator : EntityConfigurator<GenericParameter>
    {
        protected override void Configure(EntityTypeBuilder<GenericParameter> builder)
        {
            builder.HasMany(x => x.TypeConstraints)
                .WithOne(x => x.GenericParameter)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.GenericParameterID);

            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<GenericConstraint, int>());
        }
    }

}
