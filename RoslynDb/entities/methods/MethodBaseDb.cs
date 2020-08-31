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
    public class MethodBaseDb : IFullyQualifiedName, ISynchronized
    {
        protected MethodBaseDb()
        {
        }

        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public bool Synchronized { get; set; }

        public List<MethodParametricTypeDb> ParametricTypes { get; set; }
    }

    internal class MethodBaseDbConfigurator : EntityConfigurator<MethodBaseDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodBaseDb> builder )
        {
            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();
        }
    }
}
