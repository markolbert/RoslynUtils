using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn.Deprecated
{
    [EntityConfiguration(typeof(TypeParametricTypeBaseDbConfigurator))]
    public class BaseParametricTypeDb : BaseTypeDb
    {
        protected BaseParametricTypeDb()
        {
        }

        public ParametricTypeConstraint Constraints { get; set; }
    }

    internal class TypeParametricTypeBaseDbConfigurator : EntityConfigurator<BaseParametricTypeDb>
    {
        protected override void Configure(EntityTypeBuilder<BaseParametricTypeDb> builder)
        {
            builder.Property(x => x.Constraints)
                .HasConversion<string>();
        }
    }

}
