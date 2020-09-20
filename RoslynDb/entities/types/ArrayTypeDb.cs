using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#pragma warning disable 8618
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( ArrayTypeDbConfigurator ) )]
    public class ArrayTypeDb : BaseTypeDb
    {
        public int ElementTypeID { get; set; }
        public BaseTypeDb ElementType { get; set; }

        public int Rank { get; set; }
    }

    internal class ArrayTypeDbConfigurator : EntityConfigurator<ArrayTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ArrayTypeDb> builder )
        {
            builder.HasOne( x => x.ElementType )
                .WithMany( x => x.ArrayTypes )
                .HasForeignKey( x => x.SharpObjectID )
                .HasPrincipalKey( x => x.SharpObjectID );
        }
    }
}
