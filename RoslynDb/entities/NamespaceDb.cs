using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( NamespaceConfigurator ) )]
    public class NamespaceDb : IDocObject, IFullyQualifiedName, ISynchronized
    {
        public int DocObjectID { get; set; }
        public DocObject DocObject { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; } = null!;
        public string FullyQualifiedName { get; set; } = null!;

        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<TypeDb>? Types { get; set; }
    }

    internal class NamespaceConfigurator : EntityConfigurator<NamespaceDb>
    {
        protected override void Configure( EntityTypeBuilder<NamespaceDb> builder )
        {
            builder.HasKey( x => x.DocObjectID );

            builder.HasOne(x => x.DocObject)
                .WithOne(x => x.Namespace)
                .HasPrincipalKey<DocObject>(x => x.ID)
                .HasForeignKey<NamespaceDb>(x => x.DocObjectID);

            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.DocObjectID );

            builder.HasMany(x => x.Types)
                .WithOne(x => x.Namespace)
                .HasForeignKey(x => x.NamespaceId)
                .HasPrincipalKey(x => x.DocObjectID);

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();
        }
    }
}
