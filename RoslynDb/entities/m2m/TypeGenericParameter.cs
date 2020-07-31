using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeGenericParameterConfigurator))]
    public class TypeGenericParameter : GenericParameter
    {
        public int NamedTypeID { get; set; }
        public NamedType NamedType { get; set; }
    }

    internal class TypeGenericParameterConfigurator : EntityConfigurator<TypeGenericParameter>
    {
        protected override void Configure(EntityTypeBuilder<TypeGenericParameter> builder)
        {
            builder.HasOne( x => x.NamedType )
                .WithMany( x => x.TypeGenericParameters )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.NamedTypeID );
        }
    }

    //[EntityConfiguration(typeof(MethodGenericParameterConfigurator))]
    //public class MethodGenericParameter : GenericParameter
    //{
    //    public int MethodID { get; set; }
    //    public Method Method { get; set; }
    //}

}