using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(ClosedMethodArgumentConfigurator))]
    public class ClosedMethodParameter : MethodParameter
    {
        public int ParameterTypeID { get; set; }
        public TypeDefinition ParameterType { get; set; }
    }

    internal class ClosedMethodArgumentConfigurator : EntityConfigurator<ClosedMethodParameter>
    {
        protected override void Configure(EntityTypeBuilder<ClosedMethodParameter> builder)
        {
            builder.HasOne(x => x.ParameterType)
                .WithMany(x => x.ClosedMethodArguments)
                .HasForeignKey(x => x.ParameterTypeID)
                .HasPrincipalKey(x => x.ID);
        }
    }
}