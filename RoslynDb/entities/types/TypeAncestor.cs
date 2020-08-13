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
    public class TypeAncestor : ISynchronized
    {
        public int ChildTypeID { get; set; }
        public TypeDefinition ChildType { get; set; }
        public int AncestorTypeID { get; set; }
        public TypeDefinition AncestorType { get; set; }
        public bool Synchronized { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeAncestor>
    {
        protected override void Configure(EntityTypeBuilder<TypeAncestor> builder)
        {
            builder.HasOne( x => x.AncestorType )
                .WithMany( x => x.AncestorTypes )
                .HasForeignKey( x => x.AncestorTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }

}
