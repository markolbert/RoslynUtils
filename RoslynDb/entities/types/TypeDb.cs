﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeDbConfigurator ) )]
    [Table("Types")]
    public class TypeDb : IDocObject, IFullyQualifiedName, ISynchronized
    {
        protected TypeDb()
        {
        }

        public int DocObjectID { get; set; }
        public DocObject DocObject { get; set; }
        public bool Synchronized { get; set; }
        public TypeKind Nature { get; set; }
        public string Name { get; set; }
        public string FullyQualifiedName { get; set; }
        public Accessibility Accessibility { get; set; }
        public bool InDocumentationScope { get; set; }

        // the namespace to which this entity belongs
        public int NamespaceId { get; set; }
        public NamespaceDb Namespace { get; set; }

        // the assembly defining this type
        public int AssemblyID { get; set; }
        public AssemblyDb Assembly { get; set; }
        public List<ParametricTypeDb> ParametricTypes { get; set; }

        // list of return types referencing this type definition
        public List<MethodDb> ReturnTypes { get; set; }

        // list of method arguments referencing this type definition
        public List<ArgumentDb> MethodArguments { get; set; }

        // list of properties having a return value equal to this type
        public List<PropertyDb> PropertyTypes { get; set; }

        // list of types implemented by this type (including the type it is descended from)
        public List<TypeAncestorDb> AncestorTypes { get; set; }
    }

    internal class TypeDbConfigurator : EntityConfigurator<TypeDb>
    {
        protected override void Configure( EntityTypeBuilder<TypeDb> builder )
        {
            builder.HasKey(x => x.DocObjectID);

            builder.HasOne( x => x.DocObject )
                .WithOne( x => x.Type )
                .HasPrincipalKey<DocObject>( x => x.ID )
                .HasForeignKey<TypeDb>( x => x.DocObjectID );

            builder.HasOne( x => x.Namespace )
                .WithMany( x => x.Types )
                .HasForeignKey( x => x.NamespaceId )
                .HasPrincipalKey( x => x.DocObjectID );

            builder.HasOne(x => x.Assembly)
                .WithMany(x => x.Types)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.DocObjectID);

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();

            builder.Property( x => x.Accessibility )
                .HasConversion( new EnumToStringConverter<Accessibility>() );

            builder.Property(x=>x.Nature  )
                .HasConversion( new EnumToStringConverter<TypeKind>());

            //builder.Property(x => x.Nature)
            //    .HasConversion(
            //        a => a.ToString(),
            //        b => Enum.Parse<TypeKind>(b, true)
            //    );
        }
    }
}
