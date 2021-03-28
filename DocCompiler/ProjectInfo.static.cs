using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public partial class ProjectInfo
    {
        public static string[] ExcludedProjectDirectories = new[] { "bin", "obj" };

        public static bool ParseSolutionFile(
            string slnFilePath,
            StringComparison osFileComparison,
            IJ4JLogger? logger,
            out List<ProjectInfo> result )
        {
            result = new List<ProjectInfo>();

            if( !File.Exists( slnFilePath ) )
            {
                logger?.Error<string>( "File '{0}' doesn't exist", slnFilePath );
                return false;
            }

            if( !Path.GetExtension( slnFilePath ).Equals( ".sln", StringComparison.OrdinalIgnoreCase ) )
            {
                logger?.Error<string>( "'{0}' isn't a .sln file", slnFilePath );
                return false;
            }

            var lines = File.ReadAllLines( slnFilePath ).ToList();

            var nestedLine = lines.Select( ( x, i ) => new { x, i } )
                .Where( y => y.x.StartsWith( "GlobalSection(NestedProjects)" ) )
                .Select( y => y.i )
                .ToList();

            var nestedGuids = new List<Guid>();
            var folderGuids = new List<Guid>();

            switch( nestedLine.Count )
            {
                case 0:
                    // no op; no nested projects
                    break;

                case 1:
                    var curLine = nestedLine[ 0 ] + 1;

                    while( !lines[ curLine ].Equals( "EndGlobalSection" ) )
                    {
                        var match = Regex.Match( lines[ curLine ], "(.*?)=(.*?)" );
                        nestedGuids.Add( new Guid( match.Groups[ 0 ].Value ) );

                        var folderID = new Guid( match.Groups[ 2 ].Value );
                        if( !folderGuids.Contains( folderID ) )
                            folderGuids.Add( folderID );

                        curLine++;
                    }

                    break;

                default:
                    // error; too many nested project sections
                    logger?.Error<string>( "More than 1 nested project section in solution file '{0}'", slnFilePath );
                    return false;
            }

            var projects = lines.Where( x => x.StartsWith( "Project(", StringComparison.OrdinalIgnoreCase ) );

            var solutionDir = Path.GetDirectoryName( slnFilePath ) ?? string.Empty;

            var allOkay = true;

            foreach( var projText in projects )
            {
                var parts = projText.Split( '=' );

                if( parts.Length != 2 )
                {
                    logger?.Error<string, string>( "Couldn't parse project string '{0}' from solution file '{1}'",
                        projText, 
                        slnFilePath );

                    continue;
                }

                var args = parts[ 1 ].Split( ',' );

                if( args.Length != 3 )
                {
                    logger?.Error<string, string>( "Couldn't parse project string '{0}' from solution file '{1}'",
                        projText, 
                        slnFilePath );

                    continue;
                }

                var cleaned = args[ 2 ].Replace( "\"{", "" ).Replace( "}\"", "" );
                var projectID = new Guid( cleaned );

                if( nestedGuids.Contains( projectID ) || folderGuids.Contains( projectID ) ) 
                    continue;

                var projFilePath = Path.Combine( solutionDir, args[ 1 ].Replace( "\"", "" ).Trim() );

                if( ParseProjectFile( projFilePath, osFileComparison, logger, out var projInfo ) )
                    result.Add( projInfo! );
                else allOkay = false;
            }

            return allOkay;
        }

        //public static bool ParseProjectFile( 
        //    string projFilePath, 
        //    StringComparison osFileComparison, 
        //    IJ4JLogger? logger,
        //    out ProjectInfo? result )
        //{
        //    result = null;

        //    if( !File.Exists( projFilePath ) )
        //    {
        //        logger?.Error<string>( "Source file '{0}' does not exist", projFilePath );
        //        return false;
        //    }

        //    if( !Path.GetExtension( projFilePath ).Equals( ".csproj", osFileComparison ) )
        //    {
        //        logger?.Error<string>( "Source file '{0}' is not a .csproj file", projFilePath );
        //        return false;
        //    }

        //    var projDoc = CreateProjectDocument( projFilePath, logger );

        //    if( projDoc?.Root == null )
        //    {
        //        logger?.Error<string>( "Could not parse '{0}' as a C# project file", projFilePath );
        //        return false;
        //    }

        //    var projElem = projDoc.Root
        //        .DescendantsAndSelf()
        //        .FirstOrDefault( e => e.Name == "TargetFramework" || e.Name == "TargetFrameworks" )
        //        ?.Parent;

        //    if( projElem == null )
        //    {
        //        logger?.Error<string>( "Project file '{0}' has no project element", projFilePath );
        //        return false;
        //    }

        //    result = new ProjectInfo( Path.GetDirectoryName( projFilePath )!, projDoc, projElem, osFileComparison )
        //    {
        //    };

        //    return true;
        //}

        //private static XDocument? CreateProjectDocument( string projFilePath, IJ4JLogger? logger )
        //{
        //    XDocument? retVal = null;

        //    try
        //    {
        //        // this convoluted approach is needed because XDocument.Parse() does not 
        //        // properly handle the invisible UTF hint codes in files
        //        using var fs = File.OpenText( projFilePath );
        //        using var reader = new XmlTextReader( fs );

        //        retVal = XDocument.Load( reader );
        //    }
        //    catch( Exception e )
        //    {
        //        logger?.Error<string, string>( "Could not parse project file '{0}', exception was: {1}",
        //            projFilePath,
        //            e.Message );

        //        return null;
        //    }

        //    if( retVal.Root != null )
        //        return retVal;

        //    logger?.Error<string>( "Undefined root node in project file '{0}'", projFilePath );

        //    return null;
        //}

    }
}
