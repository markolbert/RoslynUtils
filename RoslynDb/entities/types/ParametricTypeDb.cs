using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeParametricTypeBaseDbConfigurator))]
    public class ParametricTypeDb : TypeDb
    {
        protected ParametricTypeDb()
        {
        }

        public ParametricTypeConstraint Constraints { get; set; }
    }

    internal class TypeParametricTypeBaseDbConfigurator : EntityConfigurator<ParametricTypeDb>
    {
        protected override void Configure(EntityTypeBuilder<ParametricTypeDb> builder)
        {
            builder.Property(x => x.Constraints)
                .HasConversion<string>();
        }
    }

}
