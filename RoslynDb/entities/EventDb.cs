﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#pragma warning disable 8618
#pragma warning disable 8603
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( EventConfigurator ) )]
    public class EventDb : ISharpObject
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }

        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }
        
        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsWindowsRuntimeEvent { get; set; }

        public int DefiningTypeID { get; set; }
        public ImplementableTypeDb? DefiningType { get; set; } = null!;

        public int EventTypeID { get; set; }
        public BaseTypeDb EventType { get; set; } = null!;

        public int? AddMethodID { get; set; }
        public MethodDb? AddMethod { get; set; } = null!;

        public int? RemoveMethodID { get; set; }
        public MethodDb? RemoveMethod { get; set; } = null!;
    }

    internal class EventConfigurator : EntityConfigurator<EventDb>
    {
        protected override void Configure( EntityTypeBuilder<EventDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne(x => x.SharpObject)
                .WithOne(x => x.Event)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<EventDb>(x => x.SharpObjectID);

            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Events )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasOne( x => x.EventType )
                .WithMany( x => x.EventTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.EventTypeID );

            builder.HasOne( x => x.AddMethod )
                .WithMany( x => x.AddEvents )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.AddMethodID );

            builder.HasOne(x => x.RemoveMethod)
                .WithMany(x => x.RemoveEvents)
                .HasPrincipalKey(x => x.SharpObjectID)
                .HasForeignKey(x => x.RemoveMethodID);

            builder.Property(x => x.Accessibility)
                .HasConversion<string>();

            builder.Property(x => x.DeclarationModifier)
                .HasConversion<string>();
        }
    }
}