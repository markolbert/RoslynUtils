using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeDefinitionTypeArgumentConfigurator))]
    public class TypeDefinitionTypeArgument : TypeArgument
    {
        public int TypeDefinitionID { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
    }

    internal class TypeDefinitionTypeArgumentConfigurator : EntityConfigurator<TypeDefinitionTypeArgument>
    {
        protected override void Configure(EntityTypeBuilder<TypeDefinitionTypeArgument> builder)
        {
            builder.HasOne( x => x.TypeDefinition )
                .WithMany( x => x.TypeArgumentReferences )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeDefinitionID );

            builder.HasIndex( x => new { x.TypeDefinitionID, x.Ordinal } )
                .IsUnique();
        }
    }

}
