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
    public enum OutputType
    {
        Library,
        Exe,
        Module,
        WinExe
    }

    public class ProjectLibrary : LibraryInfo
    {
        public ProjectLibrary(
            IJ4JLogger logger
        )
            : base( ReferenceType.Project, logger )
        {
        }

        public string ProjectFilePath { get; private set; } = string.Empty;
        public string ProjectDirectory => System.IO.Path.GetDirectoryName( ProjectFilePath ) ?? string.Empty;
        
        public List<TargetFramework> TargetFrameworks { get; } = new List<TargetFramework>();
        public OutputType OutputType { get; private set; } = OutputType.Library;

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

        public override bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !base.Initialize( rawName, container, context ) )
                return false;

            if( !GetProperty<string>( container, "msbuildProject", context, out var rawPath ) )
                return false;

            var projPath = System.IO.Path.GetFullPath( System.IO.Path.Combine( context.ProjectDirectory!, rawPath ) );

            if( !IsFileSupported( projPath ) )
                return false;

            ProjectFilePath = projPath;

            return ParseProjectFile();
        }

        public bool InitializeFromProjectFile( string projFilePath )
        {
            if( !IsFileSupported( projFilePath ) )
                return false;

            ProjectFilePath = projFilePath;

            return ParseProjectFile();
        }

        protected bool ParseProjectFile()
        {
            TargetFrameworks.Clear();

            XDocument? projDoc = CreateProjectDocument( ProjectFilePath );
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

        protected bool InitializeTargetFrameworks()
        {
            bool add_frameworks( IEnumerable<string> fwStrings )
            {
                if( fwStrings == null )
                    return false;

                foreach( var curFW in fwStrings )
                {
                    //TODO: need to figure out how to determine if it's an app
                    var newTF = new TargetFramework();

                    if( !newTF.Initialize( curFW, Logger ) )
                        return false;

                    TargetFrameworks.Add( newTF );
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

        protected XDocument? CreateProjectDocument( string projectFilePath )
        {
            XDocument? retVal = null;

            if( !IsFileSupported( projectFilePath ) )
                return null;

            var projDir = System.IO.Path.GetDirectoryName( projectFilePath );

            if( projDir == null )
            {
                Logger.Error<string>( "Could not find project directory for project '{projectFilePath}'",
                    projectFilePath );

                return null;
            }

            try
            {
                // this convoluted approach is needed because XDocument.Parse() does not 
                // properly handle the invisible UTF hint codes in files
                using var fs = File.OpenText( projectFilePath );
                using var reader = new XmlTextReader( fs );

                retVal = XDocument.Load( reader );
            }
            catch( Exception e )
            {
                Logger.Error<string, string>(
                    "Could not parse project file '{0}', exception was: {1}",
                    projectFilePath,
                    e.Message );

                return null;
            }

            if( retVal.Root != null )
                return retVal;

            Logger.Error<string>( "Undefined root node in project file '{projectFilePath}'", projectFilePath );

            return null;
        }

    }
}