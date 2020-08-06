using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
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
        public string ParameterName { get; set; }
        public int ParameterIndex { get; set; }
        public GenericConstraint Constraints { get; set; }

        public int TypeDefinitionID { get; set; }
        public TypeDefinition TypeDefinition { get; set; }

        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class TypeParameterConfigurator : EntityConfigurator<TypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameter> builder)
        {
            builder.HasMany(x => x.TypeConstraints)
                .WithOne(x => x.TypeParameter)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.TypeParameterID);

            builder.HasOne( x => x.TypeDefinition )
                .WithMany( x => x.TypeParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeDefinitionID );

            builder.Property(x => x.Constraints)
                .HasConversion(new EnumToNumberConverter<GenericConstraint, int>());

            builder.Property( x => x.ParameterName )
                .IsRequired();
        }
    }

}
