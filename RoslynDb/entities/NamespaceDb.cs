using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( NamespaceConfigurator ) )]
    public class NamespaceDb : ISharpObject //, IFullyQualifiedName, ISynchronized
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }
        //public bool Synchronized { get; set; }
        //public string Name { get; set; } = null!;
        //public string FullyQualifiedName { get; set; } = null!;

        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<TypeDb>? Types { get; set; }
    }

    internal class NamespaceConfigurator : EntityConfigurator<NamespaceDb>
    {
        protected override void Configure( EntityTypeBuilder<NamespaceDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne(x => x.SharpObject)
                .WithOne(x => x.Namespace)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<NamespaceDb>(x => x.SharpObjectID);

            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasMany(x => x.Types)
                .WithOne(x => x.Namespace)
                .HasForeignKey(x => x.NamespaceID)
                .HasPrincipalKey(x => x.SharpObjectID);

            //builder.HasAlternateKey(x => x.FullyQualifiedName);

            //builder.Property(x => x.FullyQualifiedName)
            //    .IsRequired();
        }
    }
}
