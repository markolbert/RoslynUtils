using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(ClosedPropertyParameterConfigurator))]
    public class ClosedPropertyParameter : PropertyParameter
    {
        public int ParameterTypeID { get; set; }
        public TypeDefinition ParameterType { get; set; }
    }

    internal class ClosedPropertyParameterConfigurator : EntityConfigurator<ClosedPropertyParameter>
    {
        protected override void Configure(EntityTypeBuilder<ClosedPropertyParameter> builder)
        {
            builder.HasOne(x => x.ParameterType)
                .WithMany(x => x.ClosedPropertyParamters)
                .HasForeignKey(x => x.ParameterTypeID)
                .HasPrincipalKey(x => x.ID);
        }
    }
}