using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeParameterConfigurator))]
    public class MethodTypeParameter : TypeParameterUsage
    {
        public int MethodID { get; set; }
        public Method Method { get; set; }
    }

    internal class MethodTypeParameterConfigurator : EntityConfigurator<MethodTypeParameter>
    {
        protected override void Configure(EntityTypeBuilder<MethodTypeParameter> builder)
        {
            builder.HasOne(x => x.Method)
                .WithMany(x => x.TypeParameterReferences)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.MethodID);
        }
    }
}