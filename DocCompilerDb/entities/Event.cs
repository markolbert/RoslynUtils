﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(EventConfigurator))]
    public class Event
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<NamedType> DeclaredIn { get; set; }
        public int EventTypeID { get; set; }
        public NamedTypeReference EventType { get; set; }
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
