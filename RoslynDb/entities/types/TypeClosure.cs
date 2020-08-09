using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn.entities.types
{
    [EntityConfiguration(typeof(TypeClosureConfigurator))]
    public class TypeClosure : ISynchronized
    {
        public int TypeBeingClosedID { get; set; }
        public TypeDefinition TypeBeingClosed { get; set; }
        public int Ordinal { get; set; }
        public int ClosingTypeID { get; set; }
        public TypeDefinition ClosingType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeClosureConfigurator : EntityConfigurator<TypeClosure>
    {
        protected override void Configure(EntityTypeBuilder<TypeClosure> builder)
        {
            builder.HasKey( x => new { x.TypeBeingClosedID, x.Ordinal, x.ClosingTypeID } );

            builder.HasOne( x => x.TypeBeingClosed )
                .WithMany( x => x.TypeClosures )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeBeingClosedID );

            builder.HasOne( x => x.ClosingType )
                .WithMany( x => x.TypeClosureReferences )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ClosingTypeID );
        }
    }
}
