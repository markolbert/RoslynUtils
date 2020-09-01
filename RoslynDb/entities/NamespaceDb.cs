using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( NamespaceConfigurator ) )]
    public class NamespaceDb : IFullyQualifiedName, ISynchronized
    {
        public int ID { get; set; }
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
            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.ID );

            builder.HasMany(x => x.Types)
                .WithOne(x => x.Namespace)
                .HasForeignKey(x => x.NamespaceId)
                .HasPrincipalKey(x => x.ID);

            builder.HasAlternateKey(x => x.FullyQualifiedName);

            builder.Property(x => x.FullyQualifiedName)
                .IsRequired();
        }
    }
}
