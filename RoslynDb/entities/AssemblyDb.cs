using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(AssemblyConfigurator))]
    public class AssemblyDb : IFullyQualifiedName, ISynchronized
    {
        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullyQualifiedName { get; set; } = string.Empty;

        public string DotNetVersionText { get; set; } = "0.0.0.0";
        public Version DotNetVersion
        {
            get => Version.TryParse(DotNetVersionText, out var version) 
                    ? version 
                    : new Version(0, 0, 0, 0);

            set => DotNetVersionText = value.ToString();
        }

        public InScopeAssemblyInfo? InScopeInfo { get; set; }

        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<TypeDb>? Types { get; set; }
    }

    internal class AssemblyConfigurator : EntityConfigurator<AssemblyDb>
    {
        protected override void Configure( EntityTypeBuilder<AssemblyDb> builder )
        {
            builder.Ignore( x => x.DotNetVersion );

            builder.HasMany(x => x.AssemblyNamespaces)
                .WithOne(x => x.Assembly)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.ID);

            builder.HasAlternateKey( x => x.FullyQualifiedName );

            builder.Property( x => x.FullyQualifiedName )
                .IsRequired();

            builder.HasOne( x => x.InScopeInfo )
                .WithOne( x => x.Assembly );
        }
    }
}