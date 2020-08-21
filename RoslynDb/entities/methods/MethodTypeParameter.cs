using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeParameterConfigurator))]
    public class MethodTypeParameter : TypeParameterBase
    {
        public int DeclaringMethodID { get; set; }
        public Method DeclaringMethod { get; set; }

        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class MethodTypeParameterConfigurator : EntityConfigurator<MethodTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<MethodTypeParameter> builder)
        {
            builder.HasOne( x => x.DeclaringMethod )
                .WithMany( x => x.TypeParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.DeclaringMethodID );
        }
    }

}
