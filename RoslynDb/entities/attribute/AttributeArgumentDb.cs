using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(AttributeArgumentConfigurator))]
    public class AttributeArgumentDb
    {
        public int ID { get; set; }
        public int AttributeID { get; set; }
        public AttributeDb Attribute { get; set; }

        public AttributeArgumentType ArgumentType { get; set; }
        public string? PropertyName { get; set; }
        public int? ConstructorArgumentOrdinal { get; set; }

        public string Value { get; set; }
    }

    internal class AttributeArgumentConfigurator : EntityConfigurator<AttributeArgumentDb>
    {
        protected override void Configure(EntityTypeBuilder<AttributeArgumentDb> builder)
        {
            builder.HasOne( x => x.Attribute )
                .WithMany( x => x.Arguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.AttributeID );
        }
    }
}
