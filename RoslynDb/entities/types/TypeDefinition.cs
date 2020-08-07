﻿using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeDefinitionConfigurator ) )]
    public class TypeDefinition : IFullyQualifiedName, ISynchronized
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

        // list of types implemented by this type
        public List<TypeImplementation> ImplementedTypes { get; set; }

        // list of type implementations referencing this type definition
        public List<TypeImplementation> Implementations { get; set; }

        // list of generic closures referencing this type definition
        public List<ClosedTypeParameter> GenericClosures { get; set; }

        // list of methods defined for this type
        public List<Method> Methods { get; set; }

        // list of methods declaring this type as a return type
        public List<Method> ReturnTypes { get; set; }
        
        // list of method arguments referencing this type
        public List<ClosedMethodParameter> ClosedMethodArguments { get; set; }

        // list of generic method type constraints referencing this type
        public List<MethodTypeConstraint> MethodTypeConstraints { get; set; }

        // list of properties implemented by this type
        public List<Property> Properties { get; set; }

        // list of properties whose type matches this type
        public List<Property> PropertyTypes { get; set; }

        // list of PropertyParameters using this type
        public List<ClosedPropertyParameter> ClosedPropertyParamters { get; set; }

        // list of generic property type constraints referencing this type
        public List<PropertyTypeConstraint> PropertyTypeConstraints { get; set; }
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

            builder.HasMany( x => x.ImplementedTypes )
                .WithOne( x => x.TypeDefinition )
                .HasForeignKey( x => x.TypeDefinitionID )
                .HasPrincipalKey( x => x.ID );

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
