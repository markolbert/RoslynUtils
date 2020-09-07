using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( GenericTypeDbConfigurator ) )]
    public class GenericTypeDb : ImplementableTypeDb
    {
    }

    internal class GenericTypeDbConfigurator : EntityConfigurator<GenericTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<GenericTypeDb> builder )
        {
        }
    }
}
