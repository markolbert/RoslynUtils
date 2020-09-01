using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeImplementationConfigurator))]
    public class TypeAncestorDb : ISynchronized
    {
        public int ChildTypeID { get; set; }
        public TypeDb ChildType { get; set; }
        public int AncestorTypeID { get; set; }
        public TypeDb AncestorType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeAncestorDb>
    {
        protected override void Configure(EntityTypeBuilder<TypeAncestorDb> builder)
        {
            builder.HasOne( x => x.AncestorType )
                .WithMany( x => x.AncestorTypes )
                .HasForeignKey( x => x.AncestorTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasKey( x => new { x.ChildTypeID, x.AncestorTypeID } );
        }
    }

}
