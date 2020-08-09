using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using J4JSoftware.Roslyn.entities.types;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeImplementationConfigurator))]
    public class TypeAncestor : ISynchronized
    {
        public int ID { get; set; }
        public int ChildTypeID { get; set; }
        public TypeDefinition ChildType { get; set; }
        public int ImplementingTypeID { get; set; }
        public TypeDefinition ImplementingType { get; set; }
        public bool Synchronized { get; set; }

        // only one of these next properties should ever be non-null
        // and they all can be null if the implementation relates 
        // solely to a ContainingType
        public TypeParameter? TypeParameter { get; set; }
        public Method? MethodReturnType { get; set; }
        public MethodParameter? MethodParameter { get; set; }
        public Property? Property { get; set; }
        public PropertyParameter? PropertyParameter { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeAncestor>
    {
        protected override void Configure(EntityTypeBuilder<TypeAncestor> builder)
        {
            builder.HasOne( x => x.ImplementingType )
                .WithMany( x => x.Implementations )
                .HasForeignKey( x => x.ImplementingTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }

}
