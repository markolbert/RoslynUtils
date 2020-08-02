using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeDefinitionConfigurator ) )]
    public class TypeDefinition
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public TypeKind Nature { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullyQualifiedName { get; set; } = string.Empty;
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }
        public bool InDocumentationScope { get; set; }

        // the namespace to which this TypeDefinition entity belongs
        public int NamespaceId { get; set; }
        public Namespace Namespace { get; set; }

        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; }

        // list of generic parameters used by this type definition, if any
        public List<TypeParameter> TypeParameters { get; set; }

        // list of generic type constraints referencing this type definition, if any
        public List<TypeConstraint> TypeConstraints { get; set; }
    }

    internal class TypeDefinitionConfigurator : EntityConfigurator<TypeDefinition>
    {
        protected override void Configure( EntityTypeBuilder<TypeDefinition> builder )
        {
            builder.HasOne( x => x.Namespace )
                .WithMany( x => x.Types )
                .HasForeignKey( x => x.NamespaceId )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne(x => x.Assembly)
                .WithMany(x => x.Types)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.ID);

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
