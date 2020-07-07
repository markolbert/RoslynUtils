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

namespace J4JSoftware.Roslyn
{
    public class ProjectLibrary : LibraryInfo
    {
        public ProjectLibrary(
            string text,
            ExpandoObject libInfo,
            string projDir,
            Func<IJ4JLogger> loggerFactory
        )
            : base( text, loggerFactory, ReferenceType.Project )
        {
            // not sure if this is always correct, but I believe the only references to other
            // projects that show up in the project.assets.json file are libraries, not executables
            // or modules.
            OutputType = OutputType.Library;

            var path = Path.GetFullPath( Path.Combine( projDir, GetProperty<string>( libInfo, "msbuildProject" ) ) );

            if( !IsFileSupported( path ) )
                throw new ArgumentException($"File '{path}' is not supported");

            ProjectFilePath = path;
            ParseProjectFile();
        }

        public ProjectLibrary(
            string projFilePath,
            Func<IJ4JLogger> loggerFactory
        )
            : base( Path.GetFileNameWithoutExtension( projFilePath ), loggerFactory, ReferenceType.Project )
        {
            if (!IsFileSupported(projFilePath))
                throw new ArgumentException($"File '{projFilePath}' is not supported");

            ProjectFilePath = projFilePath;
            ParseProjectFile();
        }

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
        public XElement? ProjectElement { get; private set; }

        public string? AssemblyName => ProjectElement?.Descendants( "AssemblyName" ).FirstOrDefault()?.Value;
        public string? RootNamespace => ProjectElement?.Descendants( "RootNamespace" ).FirstOrDefault()?.Value;
        public string? Authors => ProjectElement?.Descendants( "Authors" ).FirstOrDefault()?.Value;
        public string? Company => ProjectElement?.Descendants( "Company" ).FirstOrDefault()?.Value;
        public string? Description => ProjectElement?.Descendants( "Description" ).FirstOrDefault()?.Value;
        public string? Copyright => ProjectElement?.Descendants( "Copyright" ).FirstOrDefault()?.Value;

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

                if( !System.Version.TryParse( text, out var parsed ) )
                    return new System.Version();

                return parsed;
            }
        }

        public Version FileVersion
        {
            get
            {
                var text = ProjectElement?.Descendants( "FileVersion" ).FirstOrDefault()?.Value ?? "";

                if( !System.Version.TryParse(text, out var parsed) )
                    return new System.Version();

                return parsed;
            }
        }

        public bool IsFileSupported( string projectFilePath )
        {
            if( String.IsNullOrEmpty( projectFilePath ) )
            {
                Logger.Error( "Undefined project file path" );
                return false;
            }

            if( !File.Exists( projectFilePath ) )
            {
                Logger.Error<string>( "Project file '{projectFilePath}' doesn't exist", projectFilePath );
                return false;
            }

            var ext = System.IO.Path.GetExtension( projectFilePath );

            if( !ext.Equals( ".csproj", StringComparison.OrdinalIgnoreCase ) )
            {
                Logger.Error<string>( "Unsupported project file type '{ext}'", ext );
                return false;
            }

            Logger.Verbose<string>( "Validated project file '{projectFilePath}'", projectFilePath );
            return true;
        }

        private bool ParseProjectFile()
        {
            TargetFrameworks.Clear();

            XDocument? projDoc = CreateProjectDocument();
            if( projDoc == null )
                return false;

            ProjectElement = projDoc.Root?.DescendantsAndSelf()
                .FirstOrDefault( e => e.Name == "AssemblyName" )
                ?.Parent;

            if( ProjectElement == null )
            {
                Logger.Error<string>( "'{0}' has no primary ProjectGroup ", ProjectFilePath );
                return false;
            }

            Document = projDoc;

            // determine if project produces an executable
            var typeText = ProjectElement.Descendants( "OutputType" ).FirstOrDefault()?.Value;

            if( !string.IsNullOrEmpty( typeText ) )
            {
                if( Enum.TryParse<OutputType>( typeText, true, out var projType ) )
                    OutputType = projType;
                else
                {
                    Logger.Error( "Couldn't parse OutputType from project file" );
                    return false;
                }
            }

            if( !InitializeTargetFrameworks() )
            {
                Logger.Error( "Failed to initialize target framework(s) from project file" );
                return false;
            }

            SourceFiles.Clear();
            SourceFiles.AddRange( Directory.GetFiles( ProjectDirectory, $"*.cs" ).ToList()
                .Where( f => !ExcludedFiles.Any( x => f.Equals( x, StringComparison.OrdinalIgnoreCase ) ) ) );

            return true;
        }

        private bool InitializeTargetFrameworks()
        {
            bool add_frameworks( IEnumerable<string> fwStrings )
            {
                if( fwStrings == null )
                    return false;

                foreach( var curFW in fwStrings )
                {
                    //TODO: need to figure out how to determine if it's an app
                    if( !TargetFramework.Create( curFW, TargetFrameworkTextStyle.Simple, out var newTF ) )
                        return false;

                    TargetFrameworks.Add( newTF! );
                }

                return true;
            }

            TargetFrameworks.Clear();

            var singleFramework = ProjectElement?.Descendants( "TargetFramework" ).FirstOrDefault()?.Value;

            if( singleFramework != null )
                return add_frameworks( new[] { singleFramework } );

            if( !add_frameworks( ProjectElement?
                .Descendants( "TargetFrameworks" ).FirstOrDefault()?.Value
                .Split( ';' )! ) )
                return false;

            return true;
        }

        private XDocument? CreateProjectDocument()
        {
            XDocument? retVal = null;

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
                Logger.Error<string, string>(
                    "Could not parse project file '{0}', exception was: {1}",
                    ProjectFilePath,
                    e.Message );

                return null;
            }

            if( retVal.Root != null )
                return retVal;

            Logger.Error<string>( "Undefined root node in project file '{projectFilePath}'", ProjectFilePath );

            return null;
        }

    }
}