﻿using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(FieldConfigurator))]
    public class Field : IDeprecation
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Deprecated { get; set; }
        public int DeclaredInID { get; set; }
        public DocumentedType DeclaredIn { get; set; }
        public int FieldTypeID { get; set; }
        public NamedType FieldType { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class FieldConfigurator : EntityConfigurator<Field>
    {
        protected override void Configure( EntityTypeBuilder<Field> builder )
        {
            builder.HasOne( x => x.DeclaredIn )
                .WithMany( x => x.Fields )
                .HasForeignKey( x => x.DeclaredInID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.FieldType )
                .WithMany( x => x.FieldTypes )
                .HasForeignKey( x => x.FieldTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}
