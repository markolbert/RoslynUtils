using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(SharpObjectConfigurator))]
    public class SharpObject : ISynchronized
    {
        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public SharpObjectType SharpObjectType { get; set; } = SharpObjectType.Unknown;

        public AssemblyDb? Assembly { get; set; }
        public NamespaceDb? Namespace { get; set; }
        public FixedTypeDb? FixedType { get; set; }
        public GenericTypeDb? GenericType { get; set; }
        public ParametricTypeDb? ParametricType { get; set; }
        public ParametricMethodTypeDb? ParametricMethodType { get; set; }
        public ArrayTypeDb? ArrayType { get; set; }
        public MethodDb? Method { get; set; }
        public ArgumentDb? MethodArgument { get; set; }
        public PropertyDb? Property { get; set; }
        public PropertyParameterDb? PropertyParameter { get; set; }
        public FieldDb? Field { get; set; }
   }

    internal class SharpObjectConfigurator : EntityConfigurator<SharpObject>
    {
        protected override void Configure( EntityTypeBuilder<SharpObject> builder )
        {
            builder.HasOne( x => x.Assembly )
                .WithOne( x => x.SharpObject )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<AssemblyDb>( x => x.SharpObjectID );

            builder.HasOne(x => x.Namespace)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<NamespaceDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.FixedType)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<FixedTypeDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.GenericType)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<GenericTypeDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.ParametricType)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<ParametricTypeDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.ParametricMethodType)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<ParametricMethodTypeDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.ArrayType)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<ArrayTypeDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.Method)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<MethodDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.MethodArgument)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<ArgumentDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.Property)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<PropertyDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.PropertyParameter)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<PropertyParameterDb>(x => x.SharpObjectID);

            builder.HasOne(x => x.Field)
                .WithOne(x => x.SharpObject)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<FieldDb>(x => x.SharpObjectID);

            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.Property( x => x.FullyQualifiedName )
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property(x => x.SharpObjectType)
                .HasConversion<string>();
        }
    }
}
