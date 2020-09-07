using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( FixedTypeDbConfigurator ) )]
    public class FixedTypeDb : ImplementableTypeDb
    {
    }

    internal class FixedTypeDbConfigurator : EntityConfigurator<FixedTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<FixedTypeDb> builder )
        {
        }
    }
}
