#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    [ EntityConfiguration( typeof(InScopeAssemblyInfoConfigurator) ) ]
    public class InScopeAssemblyInfo : ISynchronized
    {
        private readonly List<TargetFramework> _tgtFrameworks = new();
        private bool _parseTargetFrameworks;

        private string _tgtFrameworksText = string.Empty;

        public int AssemblyID { get; set; }

#pragma warning disable 8618
        public AssemblyDb Assembly { get; set; }
#pragma warning restore 8618
        public string RootNamespace { get; set; } = null!;
        public string Authors { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Copyright { get; set; } = null!;

        public string FileVersionText { get; set; } = "0.0.0.0";

        public Version FileVersion
        {
            get => Version.TryParse( FileVersionText, out var version )
                ? version
                : new Version( 0, 0, 0, 0 );

            set => FileVersionText = value.ToString();
        }

        public string PackageVersionText { get; set; } = "0.0.0";

        public SemanticVersion PackageVersion
        {
            get => SemanticVersion.TryParse( PackageVersionText, out var version )
                ? version
                : new SemanticVersion( 0, 0, 0 );

            set => PackageVersionText = value.ToString();
        }

        public string TargetFrameworksText
        {
            get => _tgtFrameworksText;

            set
            {
                _tgtFrameworksText = value;
                _parseTargetFrameworks = true;
            }
        }

        public ReadOnlyCollection<TargetFramework> TargetFrameworks
        {
            get
            {
                if( !_parseTargetFrameworks )
                    return _tgtFrameworks.AsReadOnly();

                _tgtFrameworks.Clear();

                foreach( var tgtFWText in _tgtFrameworksText.Split( ',', StringSplitOptions.RemoveEmptyEntries ) )
                    if( TargetFramework.Create( tgtFWText, TargetFrameworkTextStyle.Simple, out var tgtFW ) )
                        _tgtFrameworks.Add( tgtFW! );
                    else
                        throw new ArgumentException(
                            $"Couldn't parse '{tgtFWText}' to a {typeof(TargetFramework)}" );

                _parseTargetFrameworks = false;

                return _tgtFrameworks.AsReadOnly();
            }
        }

        public bool Synchronized { get; set; }
    }

    internal class InScopeAssemblyInfoConfigurator : EntityConfigurator<InScopeAssemblyInfo>
    {
        protected override void Configure( EntityTypeBuilder<InScopeAssemblyInfo> builder )
        {
            builder.HasKey( x => x.AssemblyID );

            builder.HasOne( x => x.Assembly )
                .WithOne( x => x.InScopeInfo )
                .HasPrincipalKey<AssemblyDb>( x => x.SharpObjectID )
                .HasForeignKey<InScopeAssemblyInfo>( x => x.AssemblyID );

            builder.Ignore( x => x.FileVersion );
            builder.Ignore( x => x.PackageVersion );
            builder.Ignore( x => x.TargetFrameworks );
        }
    }
}