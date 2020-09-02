﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(MethodDbConfigurator) ) ]
    public class MethodDb : MethodBaseDb
    {
        public string Name { get; set; }
        public MethodKind Kind { get; set; }
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }

        public int DefiningTypeID { get; set; }
        public FixedTypeDb DefiningType { get; set; }

        public int? ReturnTypeID { get; set; }
        public FixedTypeDb ReturnType { get; set; }

        public bool ReturnsByRef { get; set; }
        public bool ReturnsByRefReadOnly { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsOverride { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }


        // list of method arguments
        public List<ArgumentDb> Arguments { get; set; }
    }

    internal class MethodDbConfigurator : EntityConfigurator<MethodDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodDb> builder )
        {
            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Methods )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ReturnType )
                .WithMany( x => x.ReturnTypes )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ReturnTypeID );

            builder.Property( x => x.Accessibility )
                .HasConversion( new EnumToNumberConverter<Accessibility, int>() );

            builder.Property( x => x.DeclarationModifier )
                .HasConversion( new EnumToNumberConverter<DeclarationModifier, int>() );

            builder.Property( x => x.Kind )
                .HasConversion( new EnumToNumberConverter<MethodKind, int>() );
        }
    }
}