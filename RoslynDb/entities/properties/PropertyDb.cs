using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [Table("Properties")]
    [EntityConfiguration( typeof( PropertyConfigurator ) )]
    public class PropertyDb : IDocObject, IFullyQualifiedName, ISynchronized
    {
        public int DocObjectID { get; set; }
        public DocObject DocObject { get; set; }

        public string FullyQualifiedName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool Synchronized { get; set; }

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
        public FixedTypeDb DefiningType { get; set; } = null!;

        public int PropertyTypeID { get; set; }
        public FixedTypeDb PropertyType { get; set; } = null!;

        public List<PropertyParameterDb> Parameters { get; set; }
    }

    internal class PropertyConfigurator : EntityConfigurator<PropertyDb>
    {
        protected override void Configure( EntityTypeBuilder<PropertyDb> builder )
        {
            builder.HasKey( x => x.DocObjectID );

            builder.HasOne(x => x.DocObject)
                .WithOne(x => x.Property)
                .HasPrincipalKey<DocObject>(x => x.ID)
                .HasForeignKey<PropertyDb>(x => x.DocObjectID);

            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Properties )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.DocObjectID );

            builder.HasOne( x => x.PropertyType )
                .WithMany( x => x.PropertyTypes )
                .HasPrincipalKey( x => x.DocObjectID )
                .HasForeignKey( x => x.PropertyTypeID );

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Ignore( x => x.Accessibility );

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();

            builder.Property(x => x.GetAccessibility)
                .HasConversion(new EnumToNumberConverter<Accessibility, int>());

            builder.Property(x => x.SetAccessibility)
                .HasConversion(new EnumToNumberConverter<Accessibility, int>());

            builder.Property(x => x.DeclarationModifier)
                .HasConversion(new EnumToNumberConverter<DeclarationModifier, int>());
        }
    }
}
