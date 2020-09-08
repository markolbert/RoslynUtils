using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(DocObjectConfigurator))]
    public class SharpObject : ISynchronized
    {
        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public SharpObjectType SharpObjectType { get; set; } = SharpObjectType.Unknown;

        public object? Entity => SharpObjectType switch
        {
            SharpObjectType.Assembly => Assembly,
            SharpObjectType.Namespace => Namespace,
            SharpObjectType.FixedType => Type,
            SharpObjectType.GenericType => Type,
            SharpObjectType.ParametricType => Type,
            SharpObjectType.Method => Method,
            SharpObjectType.Property => Property,
            _ => null
        };

        public AssemblyDb? Assembly { get; set; }
        public NamespaceDb? Namespace { get; set; }
        public TypeDb? Type { get; set; }
        public MethodDb? Method { get; set; }
        //public MethodPlaceholderDb? PlaceholderMethod { get; set; }
        public PropertyDb? Property { get; set; }
    }

    internal class DocObjectConfigurator : EntityConfigurator<SharpObject>
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

            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.Property( x => x.FullyQualifiedName )
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property( x => x.SharpObjectType )
                .HasConversion<string>();
        }
    }
}
