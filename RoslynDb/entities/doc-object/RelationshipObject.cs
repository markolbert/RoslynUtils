using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(RelationshipObjectConfigurator))]
    public class RelationshipObject
    {
        public int SideOneID { get; set; }
        public DocObject SideOne { get; set; }
        public int SideTwoID { get; set; }
        public DocObject SideTwo { get; set; }
    }

    internal class RelationshipObjectConfigurator : EntityConfigurator<RelationshipObject>
    {
        protected override void Configure(EntityTypeBuilder<RelationshipObject> builder)
        {
            builder.HasKey( x => new { x.SideOneID, x.SideTwoID } );

            builder.HasOne(x => x.SideOne)
                .WithMany(x => x.SideOneRelationships)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.SideOneID);

            builder.HasOne(x => x.SideTwo)
                .WithMany(x => x.SideTwoRelationships)
                .HasPrincipalKey(x => x.ID)
                .HasForeignKey(x => x.SideTwoID);
        }
    }
}
