using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.Build.Logging.StructuredLogger;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(DocObjectConfigurator))]
    public class DocObject
    {
        public int ID { get; set; }
        public string FullyQualifiedName { get; set; }
        public string Name { get; set; }
        public bool Synchronized { get; set; }
        public DocObjectType DocObjectType { get; set; } = DocObjectType.Unknown;

        public object? Entity => DocObjectType switch
        {
            DocObjectType.Assembly => Assembly,
            DocObjectType.Namespace => Namespace,
            DocObjectType.Type => Type,
            DocObjectType.Method => Method,
            DocObjectType.Property => Property,
            _ => null
        };

        public AssemblyDb? Assembly { get; set; }
        public NamespaceDb? Namespace { get; set; }
        public TypeDb? Type { get; set; }
        public MethodBaseDb? Method { get; set; }
        public PropertyDb? Property { get; set; }
    }

    internal class DocObjectConfigurator : EntityConfigurator<DocObject>
    {
        protected override void Configure( EntityTypeBuilder<DocObject> builder )
        {
            builder.HasOne( x => x.Assembly )
                .WithOne( x => x.DocObject )
                .HasPrincipalKey<DocObject>( x => x.ID )
                .HasForeignKey<AssemblyDb>( x => x.DocObjectID );

            builder.HasOne(x => x.Namespace)
                .WithOne(x => x.DocObject)
                .HasPrincipalKey<DocObject>(x => x.ID)
                .HasForeignKey<NamespaceDb>(x => x.DocObjectID);

            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.Property( x => x.FullyQualifiedName )
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired();

            builder.Property( x => x.DocObjectType )
                .HasConversion<string>();
        }
    }
}
