using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
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
