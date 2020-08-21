using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( BaseMethodConfigurator ) )]
    public class Method : IFullyQualifiedName, ISynchronized
    {
        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public MethodKind Kind { get; set; }
        public Accessibility Accessibility { get; set; }
        public DeclarationModifier DeclarationModifier { get; set; }

        public int DefiningTypeID { get; set; }
        public TypeDefinition DefiningType { get; set; }

        public int? ReturnTypeID { get; set; }
        public TypeDefinition ReturnType { get; set; }

        // list of method arguments
        public List<MethodArgument> Arguments { get; set; }

        // list of generic parameters used by this method, if any
        public List<MethodTypeParameter> TypeParameters { get; set; }

        // list of type arguments defined for generic parameters used by this method, if any
        public List<TypeArgument> TypeArguments { get; set; }

    }

    internal class BaseMethodConfigurator : EntityConfigurator<Method>
    {
        protected override void Configure( EntityTypeBuilder<Method> builder )
        {
            builder.HasOne( x => x.DefiningType )
                .WithMany( x => x.Methods )
                .HasForeignKey( x => x.DefiningTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.ReturnType )
                .WithMany( x => x.ReturnTypes )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ReturnTypeID );

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();

            builder.Property( x => x.Accessibility )
                .HasConversion( new EnumToNumberConverter<Accessibility, int>() );

            builder.Property( x => x.DeclarationModifier )
                .HasConversion( new EnumToNumberConverter<DeclarationModifier, int>() );

            builder.Property( x => x.Kind )
                .HasConversion( new EnumToNumberConverter<MethodKind, int>() );
        }
    }
}
