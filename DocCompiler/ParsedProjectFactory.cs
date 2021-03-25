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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class ParsedProjectFactory
    {
        public static string[] ExcludedProjectDirectories = new[] { "bin", "obj" };

        private readonly StringComparison _fsComparison;
        private readonly IJ4JLogger? _logger;

        public ParsedProjectFactory(
            StringComparison fileSystemTextComparison,
            IJ4JLogger? logger
        )
        {
            _fsComparison = fileSystemTextComparison;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public ParsedProject Create( string projFilePath )
        {
            var retVal = new ParsedProject();

            if( !File.Exists( projFilePath ) )
            {
                _logger?.Error<string>( "Source file '{0}' does not exist", projFilePath );
                return retVal;
            }

            if( !Path.GetExtension( projFilePath ).Equals( ".csproj", StringComparison.OrdinalIgnoreCase ) )
            {
                _logger?.Error<string>( "Source file '{0}' is not a .cs file", projFilePath );
                return retVal;
            }

            var projXDoc = CreateProjectDocument( projFilePath );

            if( projXDoc?.Root == null )
            {
                _logger?.Error<string>( "Could not parse '{0}' as a C# project file", projFilePath );
                return retVal;
            }

            retVal.ProjectDocument = projXDoc;

            var xmlProjElement = projXDoc.Root
                .DescendantsAndSelf()
                .FirstOrDefault( e => e.Name == "TargetFramework" || e.Name == "TargetFrameworks" )
                ?.Parent;

            if( xmlProjElement == null )
            {
                _logger?.Error<string>( "Project file '{0}' has no project element", projFilePath );
                return retVal;
            }

            retVal.ProjectElement = xmlProjElement;

            var nullableText = xmlProjElement?.Descendants( "Nullable" ).FirstOrDefault()?.Value;

            if( Enum.TryParse( typeof(NullableContextOptions),
                nullableText,
                true,
                out var ncTemp ) )
                retVal.NullableContextOptions = (NullableContextOptions) ncTemp!;

            retVal.ProjectDirectory = Path.GetDirectoryName( projFilePath ) ?? string.Empty;

            var excludedDirs = ExcludedProjectDirectories.Select( x => Path.Combine( retVal.ProjectDirectory, x ) )
                .ToList();

            retVal.AddExcludedFiles( projXDoc.Root
                .Descendants()
                .Where( e => e.Name == "Compile" && e.Attribute( "Remove" ) != null )
                .Select( e => e.Attribute( "Remove" )?.Value! ) );

            retVal.AddSourceCodeFiles( Directory
                .GetFiles( retVal.ProjectDirectory, "*.cs", SearchOption.AllDirectories )
                .Where( f =>
                    !excludedDirs.Any( x => x.Equals( Path.GetDirectoryName( f ), _fsComparison ) )
                    && !retVal.ExcludedFiles.Any( x => x.Equals( f, _fsComparison ) ) ) );

            return retVal;
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
    }
}