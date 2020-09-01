using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration(typeof(InScopeAssemblyInfoConfigurator))]
    public class InScopeAssemblyInfo
    {
        private readonly List<TargetFramework> _tgtFrameworks = new List<TargetFramework>();

        private string _tgtFrameworksText = string.Empty;
        private bool _parseTargetFrameworks;

        public int ID { get; set; }
        public bool Synchronized { get; set; }
        public string RootNamespace { get; set; } = null!;
        public string Authors { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Copyright { get; set; } = null!;

        public string FileVersionText { get; set; } = "0.0.0.0";
        public Version FileVersion
        {
            get => Version.TryParse(FileVersionText, out var version)
                ? version
                : new Version(0, 0, 0, 0);

            set => FileVersionText = value.ToString();
        }

        public string PackageVersionText { get; set; } = "0.0.0";
        public SemanticVersion PackageVersion
        {
            get => SemanticVersion.TryParse(PackageVersionText, out var version)
                ? version
                : new SemanticVersion(0, 0, 0);

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
                if (!_parseTargetFrameworks)
                    return _tgtFrameworks.AsReadOnly();

                _tgtFrameworks.Clear();

                foreach (var tgtFWText in _tgtFrameworksText.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (TargetFramework.Create(tgtFWText, TargetFrameworkTextStyle.Simple, out var tgtFW))
                        _tgtFrameworks.Add(tgtFW!);
                    else
                        throw new ArgumentException(
                            $"Couldn't parse '{tgtFWText}' to a {typeof(TargetFramework)}");
                }

                _parseTargetFrameworks = false;

                return _tgtFrameworks.AsReadOnly();
            }
        }

        public int AssemblyID { get; set; }

#pragma warning disable 8618
        public AssemblyDb Assembly { get; set; }
#pragma warning restore 8618
    }

    internal class InScopeAssemblyInfoConfigurator : EntityConfigurator<InScopeAssemblyInfo>
    {
        protected override void Configure(EntityTypeBuilder<InScopeAssemblyInfo> builder)
        {
            builder.Ignore(x => x.FileVersion);
            builder.Ignore(x => x.PackageVersion);
            builder.Ignore(x => x.TargetFrameworks);
        }
    }
}
