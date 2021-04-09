using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(DocumentedTypeUsingConfigurator))]
    public class DocumentedTypeUsing
    {
        public int DocumentedTypeID { get; set; }
        public DocumentedType DocumentedType { get;set; }
        public int Index { get; set; }
        public string UsingText { get; set; }
    }

    internal class DocumentedTypeUsingConfigurator : EntityConfigurator<DocumentedTypeUsing>
    {
        protected override void Configure( EntityTypeBuilder<DocumentedTypeUsing> builder )
        {
            builder.HasKey( x => new { x.DocumentedTypeID, x.Index } );

            builder.HasOne( x => x.DocumentedType )
                .WithMany( x => x.TypeUsings )
                .HasForeignKey( x => x.DocumentedTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}
