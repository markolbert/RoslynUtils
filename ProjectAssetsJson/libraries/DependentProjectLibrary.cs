using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using NuGet.Versioning;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public class DependentProjectLibrary : ProjectAssetsBase, IProjectLibrary
    {
#pragma warning disable 8618
        public DependentProjectLibrary(
#pragma warning restore 8618
            string text,
            ExpandoObject libInfo,
            string projDir,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            if( !VersionedText.Create( text, out var verText ) )
                throw new ArgumentException( $"Couldn't parse '{text}' into {typeof(VersionedText)}" );

            Assembly = verText!.TextComponent;
            Version = verText.Version;
            Type = ReferenceType.Project;

            var path = Path.GetFullPath( Path.Combine( projDir, GetProperty<string>( libInfo, "msbuildProject" ) ) );
            ValidateProjectFile( path );

            ProjectFilePath = path;
            ParseProjectFile();
        }

        public string Assembly { get; }
        public SemanticVersion Version { get; private set; }
        public ReferenceType Type { get; }

        // this will always be a full path
        public string ProjectFilePath { get; }
        public string ProjectDirectory => Path.GetDirectoryName( ProjectFilePath ) ?? string.Empty;

        public List<TargetFramework> TargetFrameworks { get; } = new List<TargetFramework>();
        public OutputType OutputType { get; private set; }

        public OutputKind OutputKind => OutputType switch
        {
            OutputType.Library => OutputKind.DynamicallyLinkedLibrary,
            OutputType.Exe => OutputKind.ConsoleApplication,
            OutputType.Module => OutputKind.NetModule,
            OutputType.WinExe => OutputKind.WindowsRuntimeApplication,
            _ => throw new ArgumentOutOfRangeException( $"Unhandled {nameof(Roslyn.OutputType)} '{OutputType}'" )
        };

        public XDocument? Document { get; private set; }
        public XElement ProjectElement { get; private set; }

        public string? AssemblyName => ProjectElement?.Descendants( "AssemblyName" ).FirstOrDefault()?.Value;
        public string? RootNamespace => ProjectElement?.Descendants( "RootNamespace" ).FirstOrDefault()?.Value;
        public string? Authors => ProjectElement?.Descendants( "Authors" ).FirstOrDefault()?.Value;
        public string? Company => ProjectElement?.Descendants( "Company" ).FirstOrDefault()?.Value;
        public string? Description => ProjectElement?.Descendants( "Description" ).FirstOrDefault()?.Value;
        public string? Copyright => ProjectElement?.Descendants( "Copyright" ).FirstOrDefault()?.Value;

        public NullableContextOptions NullableContextOptions
        {
            get
            {
                var text = ProjectElement?.Descendants( "Nullable" ).FirstOrDefault()?.Value;

                if( Enum.TryParse( typeof(NullableContextOptions), text, true, out var retVal ) )
                    return (NullableContextOptions) retVal!;

                return NullableContextOptions.Disable;
            }
        }

        public List<string> ExcludedFiles => Document?.Root?.Descendants()
                                                 .Where( e => e.Name == "Compile" && e.Attribute( "Remove" ) != null )
                                                 .Select( e => e.Attribute( "Remove" )?.Value! )
                                                 .ToList()
                                             ?? new List<string>();

        public List<string> SourceFiles { get; } = new List<string>();

        public Version DotNetVersion
        {
            get
            {
                var text = ProjectElement?.Descendants( "AssemblyVersion" ).FirstOrDefault()?.Value ?? "";

                return !System.Version.TryParse( text, out var parsed ) ? new System.Version() : parsed;
            }
        }

        public Version FileVersion
        {
            get
            {
                var text = ProjectElement?.Descendants( "FileVersion" ).FirstOrDefault()?.Value ?? "";

                return !System.Version.TryParse(text, out var parsed) ? new System.Version() : parsed;
            }
        }

        private void ValidateProjectFile( string projectFilePath )
        {
            if( String.IsNullOrEmpty( projectFilePath ) )
                throw ProjectAssetsException.CreateAndLog( "Undefined project file path", this.GetType(), Logger );

            if( !File.Exists( projectFilePath ) )
                throw ProjectAssetsException.CreateAndLog( 
                    $"Project file '{projectFilePath}' doesn't exist",
                    this.GetType(), 
                    Logger );

            var ext = System.IO.Path.GetExtension( projectFilePath );

            if( !ext.Equals( ".csproj", StringComparison.OrdinalIgnoreCase ) )
                ProjectAssetsException.CreateAndLog( $"Unsupported project file type '{ext}'", this.GetType(), Logger );
        }

        private void ParseProjectFile()
        {
            TargetFrameworks.Clear();

            XDocument projDoc = CreateProjectDocument();

            var projElem = projDoc.Root!.DescendantsAndSelf()
                .FirstOrDefault( e => e.Name == "AssemblyName" )
                ?.Parent;

            ProjectElement = projElem
                             ?? throw ProjectAssetsException.CreateAndLog(
                                 $"Project '{ProjectFilePath}' has no primary project group",
                                 this.GetType(),
                                 Logger );

            Document = projDoc;

            // determine if project produces an executable
            var typeText = ProjectElement.Descendants( "OutputType" ).FirstOrDefault()?.Value ?? string.Empty;
            OutputType = GetEnum<OutputType>( typeText );

            InitializeTargetFrameworks();

            var objDir = Path.Combine( ProjectDirectory, "obj" );

            SourceFiles.Clear();
         
            SourceFiles.AddRange( Directory.GetFiles( ProjectDirectory, $"*.cs", SearchOption.AllDirectories ).ToList()
                .Where( f =>
                    f.IndexOf( objDir, StringComparison.OrdinalIgnoreCase ) == -1 &&
                    !ExcludedFiles.Any( x => f.Equals( x, StringComparison.OrdinalIgnoreCase ) ) ) );
        }

        private void InitializeTargetFrameworks()
        {
            TargetFrameworks.Clear();

            var singleFramework = ProjectElement.Descendants( "TargetFramework" ).FirstOrDefault()?.Value;

            if( string.IsNullOrEmpty( singleFramework ) )
                foreach (var tfwText in ProjectElement.Descendants("TargetFrameworks").FirstOrDefault()?.Value
                    .Split(';') ?? Enumerable.Empty<string>())
                {
                    TargetFrameworks.Add(GetTargetFramework(tfwText, TargetFrameworkTextStyle.Simple));
                }
            else
                TargetFrameworks.Add(GetTargetFramework(singleFramework, TargetFrameworkTextStyle.Simple));
        }

        private XDocument CreateProjectDocument()
        {
            XDocument retVal = default!;

            try
            {
                // this convoluted approach is needed because XDocument.Parse() does not 
                // properly handle the invisible UTF hint codes in files
                using var fs = File.OpenText( ProjectFilePath );
                using var reader = new XmlTextReader( fs );

                retVal = XDocument.Load( reader );
            }
            catch( Exception e )
            {
                throw ProjectAssetsException.CreateAndLog(
                    $"Could not parse project file '{ProjectFilePath}', exception was: {e.Message}", 
                    this.GetType(),
                    Logger );
            }

            if( retVal.Root == null )
                throw ProjectAssetsException.CreateAndLog(
                    $"Undefined root node in project file '{ProjectFilePath}'",
                    this.GetType(),
                    Logger);

            return retVal;
        }
    }
}