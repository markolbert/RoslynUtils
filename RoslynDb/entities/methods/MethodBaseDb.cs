using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [Table("Methods")]
    [EntityConfiguration( typeof( MethodBaseDbConfigurator ) )]
    public class MethodBaseDb : IDocObject, IFullyQualifiedName, ISynchronized
    {
        protected MethodBaseDb()
        {
        }

        public int DocObjectID { get; set; }
        public DocObject DocObject { get; set; }
        public string FullyQualifiedName { get; set; }
        public bool Synchronized { get; set; }

        public List<MethodParametricTypeDb> ParametricTypes { get; set; }
    }

    internal class MethodBaseDbConfigurator : EntityConfigurator<MethodBaseDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodBaseDb> builder )
        {
            builder.HasKey(x => x.DocObjectID);

            builder.HasOne(x => x.DocObject)
                .WithOne(x => x.Method)
                .HasPrincipalKey<DocObject>(x => x.ID)
                .HasForeignKey<MethodBaseDb>(x => x.DocObjectID);

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();
        }
    }
}
