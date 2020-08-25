using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(MethodTypeArgumentConfigurator))]
    public class MethodTypeArgument : TypeArgument
    {
        public int MethodID { get; set; }
        public Method Method { get; set; }
    }

    internal class MethodTypeArgumentConfigurator : EntityConfigurator<MethodTypeArgument>
    {
        protected override void Configure(EntityTypeBuilder<MethodTypeArgument> builder)
        {
            builder.HasOne( x => x.Method )
                .WithMany( x => x.TypeArgumentReferences )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.MethodID );

            builder.HasKey( x => new { TypeDefinitionID = x.MethodID, x.Ordinal } );
        }
    }

}
