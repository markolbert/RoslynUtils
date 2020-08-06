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
    public class TypeImplementation : ISynchronized
    {
        public int ID { get; set; }
        public int TypeDefinitionID { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
        public int ImplementedTypeID { get; set; }
        public TypeDefinition ImplementedType { get; set; }
        public bool Synchronized { get; set; }

        public List<ClosedTypeParameter> ClosedTypeParameters { get; set; }
    }

    internal class TypeImplementationConfigurator : EntityConfigurator<TypeImplementation>
    {
        protected override void Configure(EntityTypeBuilder<TypeImplementation> builder)
        {
            builder.HasMany( x => x.ClosedTypeParameters )
                .WithOne( x => x.TypeImplementation )
                .HasForeignKey( x => x.TypeImplementationID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ImplementedType )
                .WithMany( x => x.Implementations )
                .HasForeignKey( x => x.ImplementedTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }

}
