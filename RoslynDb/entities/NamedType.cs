using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( NamedTypeConfigurator ) )]
    public class NamedType
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public TypeKind Nature { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullyQualifiedName { get; set; } = string.Empty;
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }

        // the namespace to which this NamedType entity belongs
        public int NamespaceId { get; set; }
        public Namespace Namespace { get; set; }

        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; }

        // the interface NamedType entities that this NamedType entity implements
        //public List<ImplementedInterface> ImplementedInterfaces { get; set; } = null!;

        public int? ParentTypeID { get; set; }
        public NamedType? ParentNamedType { get; set; }

        // the list of NamedType entities derived from this NamedType entity
        public List<NamedType> ChildTypes { get; set; }

        // the various components/children of this NamedType entity
        //public List<Method> Methods { get; set; } = null!;
        //public List<Property> Properties { get; set; } = null!;
        //public List<Event> Events { get; set; } = null!;
        //public List<Field> Fields { get; set; } = null!;

        //public List<Property> PropertyTypes { get; set; } = null!;
        //public List<Field> FieldTypes { get; set; } = null!;
        //public List<Event> EventTypes { get; set; } = null!;
        //public List<GenericTypeParameter> GenericTypeParameters { get; set; } = null!;
        //public List<NamedGenericTypeParameter> GenericConstrainingTypes { get; set; } = null!;
        //public List<Parameter> MethodParameters { get; set; } = null!;
    }

    internal class NamedTypeConfigurator : EntityConfigurator<NamedType>
    {
        protected override void Configure( EntityTypeBuilder<NamedType> builder )
        {
            builder.HasOne( x => x.Namespace )
                .WithMany( x => x.Types )
                .HasForeignKey( x => x.NamespaceId )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ParentNamedType )
                .WithMany( x => x.ChildTypes )
                .HasForeignKey( x => x.ParentTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne(x => x.Assembly)
                .WithMany(x => x.Types)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.ID);

            //builder.HasMany( x => x.ImplementedInterfaces )
            //    .WithOne( x => x.DeclaringType )
            //    .HasForeignKey( x => x.DeclaringTypeID )
            //    .HasPrincipalKey( x => x.ID );

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();

            builder.Property( x => x.Accessibility )
                .HasConversion( new EnumToNumberConverter<Accessibility, int>() );

            builder.Property(x => x.DeclarationModifier)
                .HasConversion(new EnumToNumberConverter<DeclarationModifier, int>());

            builder.Property(x => x.Nature)
                .HasConversion(
                    a => a.ToString(),
                    b => Enum.Parse<TypeKind>(b, true)
                );
        }
    }
}
