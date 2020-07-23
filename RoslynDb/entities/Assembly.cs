using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(AssemblyConfigurator))]
    public class Assembly
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
    }

    internal class AssemblyConfigurator : EntityConfigurator<Assembly>
    {
        protected override void Configure( EntityTypeBuilder<Assembly> builder )
        {
            builder.Ignore( x => x.DotNetVersion );

            builder.HasAlternateKey( x => x.FullyQualifiedName );

            builder.Property( x => x.FullyQualifiedName )
                .IsRequired();

            builder.HasOne( x => x.InScopeInfo )
                .WithOne( x => x.Assembly );
        }
    }
}