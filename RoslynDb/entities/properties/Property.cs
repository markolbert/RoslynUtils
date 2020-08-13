using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( PropertyConfigurator ) )]
    public class Property : IFullyQualifiedName, ISynchronized
    {
        public int ID { get; set; }
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
        public bool IsWithEvents { get; set; }
        public bool IsWriteOnly { get; set; }
        public bool IsReadOnly { get; set; }

        public int DefiningTypeID { get; set; }
        public TypeDefinition DefiningType { get; set; } = null!;

        public int PropertyTypeID { get; set; }
        public TypeDefinition PropertyType { get; set; } = null!;

        public List<PropertyParameter> Parameters { get; set; }
    }

    internal class PropertyConfigurator : EntityConfigurator<Property>
    {
        protected override void Configure( EntityTypeBuilder<Property> builder )
        {
            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Properties )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.PropertyType )
                .WithMany( x => x.PropertyTypes )
                .HasPrincipalKey( x => x.ID )
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
