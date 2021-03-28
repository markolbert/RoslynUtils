using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(FieldConfigurator))]
    public class Field
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Deprecated { get; set; }
        public int DeclaredInID { get; set; }
        public NamedType DeclaredIn { get; set; }
        public int FieldTypeID { get; set; }
        public NamedType FieldType { get; set; }
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
