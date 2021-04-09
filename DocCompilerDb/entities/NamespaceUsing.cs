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
    [ EntityConfiguration( typeof(NamespaceUsingConfigurator) ) ]
    public class NamespaceUsing
    {
        public int NamespaceContextID { get; set; }
        public Namespace NamespaceContext { get; set; }

        public int CodeFileID { get; set; }
        public CodeFile CodeFile { get; set; }

        public ICollection<Namespace>? InScopeNamespaces { get; set; }
    }

    public class NamespaceUsingConfigurator : EntityConfigurator<NamespaceUsing>
    {
        protected override void Configure( EntityTypeBuilder<NamespaceUsing> builder )
        {
            builder.HasKey( x => new { NamespaceID = x.NamespaceContextID, x.CodeFileID } );

            builder.HasOne( x => x.NamespaceContext )
                .WithMany( x => x.NamespaceContexts )
                .HasForeignKey( x => x.NamespaceContextID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.CodeFile )
                .WithMany( x => x.NamespaceUsingReferences )
                .HasForeignKey( x => x.CodeFileID )
                .HasPrincipalKey( x => x.ID );

            builder.HasMany( x => x.InScopeNamespaces )
                .WithMany( x => x.NamespaceReferences );
        }
    }
}
