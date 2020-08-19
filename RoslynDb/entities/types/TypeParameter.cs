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
    public class TypeParameter : TypeParameterBase
    {
        public int ContainingTypeID { get; set; }
        public TypeDefinition ContainingType { get; set; }

        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class TypeParameterConfigurator : EntityConfigurator<TypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeParameter> builder)
        {
            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.TypeParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ContainingTypeID );
        }
    }

}
