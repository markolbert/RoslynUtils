using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(AttributeConfigurator))]
    public class AttributeDb
    {
        public int ID { get; set; }
        public int TargetObjectID { get; set; }
        public SharpObject TargetObject { get; set; }
        public int AttributeTypeID { get; set; }
        public ImplementableTypeDb AttributeType { get; set; }

        public List<AttributeArgumentDb> Arguments { get; set; }
    }

    internal class AttributeConfigurator : EntityConfigurator<AttributeDb>
    {
        protected override void Configure( EntityTypeBuilder<AttributeDb> builder )
        {
            builder.HasOne( x => x.TargetObject )
                .WithMany( x => x.Attributes )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TargetObjectID );

            builder.HasOne( x => x.AttributeType )
                .WithMany( x => x.AttributeReferences )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.AttributeTypeID );
        }
    }
}
