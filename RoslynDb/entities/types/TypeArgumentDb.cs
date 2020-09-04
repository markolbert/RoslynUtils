using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeArgumentConfigurator))]
    public class TypeArgumentDb : ISynchronized
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }

        public int DeclaringTypeID { get; set; }
        public ImplementableTypeDb DeclaringType { get; set; }

        public int Ordinal { get; set; }
        public int ArgumentTypeID { get; set; }
        public ImplementableTypeDb ArgumentType { get; set; }
    }

    internal class TypeArgumentConfigurator : EntityConfigurator<TypeArgumentDb>
    {
        protected override void Configure(EntityTypeBuilder<TypeArgumentDb> builder)
        {
            builder.HasOne(x => x.ArgumentType)
                .WithMany(x => x.TypeArgumentReferences)
                .HasPrincipalKey(x => x.DocObjectID)
                .HasForeignKey(x => x.ArgumentTypeID);

            builder.HasOne( x => x.DeclaringType )
                .WithMany( x => x.TypeArguments )
                .HasPrincipalKey( x => x.DocObjectID )
                .HasForeignKey( x => x.DeclaringTypeID );

            builder.HasIndex(x => new { TypeDefinitionID = x.DeclaringTypeID, x.Ordinal })
                .IsUnique();
        }
    }

}
