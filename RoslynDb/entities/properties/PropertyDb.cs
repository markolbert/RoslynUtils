using System.Collections.Generic;
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
    [Table("Properties")]
    [EntityConfiguration( typeof( PropertyConfigurator ) )]
    public class PropertyDb : ISharpObject
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }

        public Accessibility Accessibility
        {
            get
            {
                var getVal = GetAccessibility ?? Accessibility.NotApplicable;
                var setVal = SetAccessibility ?? Accessibility.NotApplicable;

                return getVal >= setVal ? getVal : setVal;
            }
        }

        public Accessibility? GetAccessibility { get; set; }
        public Accessibility? SetAccessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }
        
        public bool ReturnsByRef { get; set; }
        public bool ReturnsByRefReadOnly { get; set; }

        public bool IsAbstract { get; set; }
        public bool IsExtern { get; set; }
        public bool IsIndexer { get; set; }
        public bool IsOverride { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsSealed { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsWriteOnly { get; set; }

        public int DefiningTypeID { get; set; }
        public ImplementableTypeDb DefiningType { get; set; } = null!;

        public int PropertyTypeID { get; set; }
        public BaseTypeDb PropertyType { get; set; } = null!;

        public List<PropertyParameterDb> Parameters { get; set; }
    }

    internal class PropertyConfigurator : EntityConfigurator<PropertyDb>
    {
        protected override void Configure( EntityTypeBuilder<PropertyDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne(x => x.SharpObject)
                .WithOne(x => x.Property)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<PropertyDb>(x => x.SharpObjectID);

            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Properties )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasOne( x => x.PropertyType )
                .WithMany( x => x.PropertyTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.PropertyTypeID );

            builder.Ignore( x => x.Accessibility );

            builder.Property(x => x.GetAccessibility)
                .HasConversion<string>();

            builder.Property(x => x.SetAccessibility)
                .HasConversion<string>();

            builder.Property(x => x.DeclarationModifier)
                .HasConversion<string>();
        }
    }
}
