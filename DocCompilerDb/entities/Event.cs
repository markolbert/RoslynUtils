using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(EventConfigurator))]
    public class Event : IDeprecation
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Deprecated { get; set; }
        public int EventTypeID { get; set; }
        public NamedType EventType { get; set; }

        public ICollection<DocumentedType> DeclaredIn { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class EventConfigurator : EntityConfigurator<Event>
    {
        protected override void Configure( EntityTypeBuilder<Event> builder )
        {
            builder.HasIndex( x => x.Name )
                .IsUnique();

            builder.HasMany( x => x.DeclaredIn )
                .WithMany( x => x.Events );

            builder.HasOne( x => x.EventType )
                .WithMany( x => x.UsedInEvents )
                .HasForeignKey( x => x.EventTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}
