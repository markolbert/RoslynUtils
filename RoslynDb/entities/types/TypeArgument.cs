﻿using System.Collections.Generic;
using System.Xml.Serialization;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(TypeArgumentConfigurator))]
    public class TypeArgument : ISynchronized
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; }
        public int Ordinal { get; set; }

        public int TypeDefinitionID { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
    }

    internal class TypeArgumentConfigurator : EntityConfigurator<TypeArgument>
    {
        protected override void Configure(EntityTypeBuilder<TypeArgument> builder)
        {
            builder.HasOne( x => x.TypeDefinition )
                .WithMany( x => x.TypeArguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeDefinitionID );

            builder.Property( x => x.Name )
                .IsRequired();
        }
    }

}
