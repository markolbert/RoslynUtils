#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace J4JSoftware.DocCompiler
{
    public class ScannedFileFactory : IScannedFileFactory
    {
        private readonly IJ4JLogger? _logger;

        public ScannedFileFactory(
            IJ4JLogger? logger 
            )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool Create( string sourceFilePath, out List<ScannedFile>? result )
        {
            result = null;

            if( !File.Exists( sourceFilePath ) )
            {
                _logger?.Error<string>("Source file '{0}' does not exist", sourceFilePath);
                return false;
            }

            return Path.GetExtension( sourceFilePath ).ToLower() switch
            {
                //".cs" => CreateStandaloneFile( sourceFilePath,
                //    ( p, r ) => new ScannedFile() { SourceFilePath = p, RootNode = r }, 
                //    out result ),

                ".csproj" => ScanProject( sourceFilePath, out result ),
                ".sln" => ScanSolution( sourceFilePath, out result ),
                _ => false
            };
        }

        private bool CreateScannedFile(string sourceFilePath, ProjectInfo projInfo, out ScannedFile? result)
        {
            result = null;

            try
            {
                using var fileStream = File.OpenRead(sourceFilePath);
                var srcText = SourceText.From(fileStream);

                var syntaxTree = CSharpSyntaxTree.ParseText(srcText);

                result = new ScannedFile
                {
                    BelongsTo = projInfo,
                    RootNode = syntaxTree.GetRoot(), 
                    SourceFilePath = sourceFilePath
                };

                var nodeWalker = new DocNodeWalker(result);
                nodeWalker.Visit();
            }
            catch (Exception e)
            {
                _logger?.Error<string>("Parsing failed, exception was '{0}'", e.Message);

                return false;
            }

            return true;
        }

        private bool ScanProject( string projFilePath, out List<ScannedFile>? result )
        {
            result = null;

            var projDoc = CreateProjectDocument( projFilePath );

            if( projDoc?.Root == null )
            {
                _logger?.Error<string>( "Could not parse '{0}' as a C# project file", projFilePath );
                return false;
            }

            var projElem = projDoc.Root
                .DescendantsAndSelf()
                .FirstOrDefault( e => e.Name == "TargetFramework" || e.Name == "TargetFrameworks" )
                ?.Parent;

            if( projElem == null )
            {
                _logger?.Error<string>( "Project file '{0}' has no project element", projFilePath );
                return false;
            }

            var projInfo = new ProjectInfo( Path.GetDirectoryName( projFilePath )!, projDoc, projElem );

            result = new List<ScannedFile>();

            var allOkay = true;

            foreach( var srcFile in projInfo!.SourceCodeFiles )
            {
                if( CreateScannedFile( srcFile, projInfo, out var temp) )
                    result.Add( temp! );
                else allOkay = false;
            }

            return allOkay;
        }

        private XDocument? CreateProjectDocument( string projFilePath )
        {
            XDocument? retVal = null;

            try
            {
                // this convoluted approach is needed because XDocument.Parse() does not 
                // properly handle the invisible UTF hint codes in files
                using var fs = File.OpenText( projFilePath );
                using var reader = new XmlTextReader( fs );

                retVal = XDocument.Load( reader );
            }
            catch( Exception e )
            {
                _logger?.Error<string, string>( "Could not parse project file '{0}', exception was: {1}",
                    projFilePath,
                    e.Message );

                return null;
            }

            if( retVal.Root != null )
                return retVal;

            _logger?.Error<string>( "Undefined root node in project file '{0}'", projFilePath );

            return null;
        }

        private bool ScanSolution( string slnFilePath, out List<ScannedFile>? result )
        {
            result = null;

            var projects = new List<ProjectInfo>();

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
                    _logger?.Error<string>( "More than 1 nested project section in solution file '{0}'", slnFilePath );
                    return false;
            }

            var projectEntries = lines.Where( x => x.StartsWith( "Project(", OsUtilities.FileSystemComparison ) );

            var solutionDir = Path.GetDirectoryName( slnFilePath ) ?? string.Empty;
            
            result = new List<ScannedFile>();
            var allOkay = true;

            foreach( var projText in projectEntries )
            {
                var parts = projText.Split( '=' );

                if( parts.Length != 2 )
                {
                    _logger?.Error<string, string>( "Couldn't parse project string '{0}' from solution file '{1}'",
                        projText, 
                        slnFilePath );

                    continue;
                }

                var args = parts[ 1 ].Split( ',' );

                if( args.Length != 3 )
                {
                    _logger?.Error<string, string>( "Couldn't parse project string '{0}' from solution file '{1}'",
                        projText, 
                        slnFilePath );

                    continue;
                }

                var cleaned = args[ 2 ].Replace( "\"{", "" ).Replace( "}\"", "" );
                var projectID = new Guid( cleaned );

                if( nestedGuids.Contains( projectID ) || folderGuids.Contains( projectID ) ) 
                    continue;

                var projFilePath = Path.Combine( solutionDir, args[ 1 ].Replace( "\"", "" ).Trim() );

                if( ScanProject( projFilePath, out var temp ) )
                    result.AddRange( temp! );
                else allOkay = false;
            }

            return allOkay;
        }
    }
}