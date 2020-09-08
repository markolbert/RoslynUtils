﻿using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618
#pragma warning disable 8602
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(AssemblyConfigurator))]
    public class AssemblyDb : ISharpObject //, IFullyQualifiedName, ISynchronized
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }

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
            builder.HasKey(x => x.SharpObjectID);

            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.Assembly )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<AssemblyDb>( x => x.SharpObjectID );

            builder.Ignore( x => x.DotNetVersion );

            builder.HasMany(x => x.AssemblyNamespaces)
                .WithOne(x => x.Assembly)
                .HasForeignKey(x => x.AssemblyID)
                .HasPrincipalKey(x => x.SharpObjectID);

            //builder.HasAlternateKey( x => x.FullyQualifiedName );

            //builder.Property( x => x.FullyQualifiedName )
            //    .IsRequired();

            builder.HasOne( x => x.InScopeInfo )
                .WithOne( x => x.Assembly );
        }
    }
}