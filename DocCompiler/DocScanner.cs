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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace J4JSoftware.DocCompiler
{
    public class DocScanner : CSharpSyntaxWalker, IScanResults
    {
        private readonly DocNodeWalker _nodeWalker;
        private readonly StringComparison _osFileComparison;
        private readonly List<StandaloneFile> _scannedFiles = new();
        private readonly IJ4JLogger? _logger;

        public DocScanner(
            DocNodeWalker nodeWalker,
            StringComparison osFileComparison,
            IJ4JLogger? logger
        )
        {
            _nodeWalker = nodeWalker;
            _osFileComparison = osFileComparison;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool ScanSolution( string solutionPath )
        {
            if( !ProjectInfo.ParseSolutionFile(solutionPath, _osFileComparison, _logger, out var projects))
                return false;

            var allOkay = true;

            foreach( var projInfo in projects )
            {
                foreach( var srcFile in projInfo.SourceCodeFiles )
                {
                    if( ProjectFile.ParseFileInProject( projInfo, srcFile, _nodeWalker, _logger, out var result ) )
                        _scannedFiles.Add( result! );
                    else allOkay = false;
                }
            }

            return allOkay;
        }

        public bool ScanProject( string projPath )
        {
            if( !ProjectInfo.ParseProjectFile( projPath, _osFileComparison, _logger, out var projInfo ) )
                return false;

            var allOkay = true;

            foreach( var srcFile in projInfo!.SourceCodeFiles )
            {
                if( ProjectFile.ParseFileInProject( projInfo, srcFile, _nodeWalker, _logger, out var result ) )
                    _scannedFiles.Add( result! );
                else allOkay = false;
            }

            return allOkay;
        }

        public bool ScanSourceFile( string filePath )
        {
            if( !StandaloneFile.ParseStandaloneFile( filePath, _nodeWalker, _logger, out var result ) )
                return false;

            _scannedFiles.Add( result! );

            return true;
        }

        public ReadOnlyCollection<StandaloneFile> ScannedFiles => _scannedFiles.AsReadOnly();

        public List<UsingStatementSyntax> Usings => _scannedFiles.SelectMany( x => x.Usings ).ToList();
        public List<NamespaceDeclarationSyntax> Namespaces => _scannedFiles.SelectMany( x => x.Namespaces ).ToList();
        public List<ClassDeclarationSyntax> Classes => _scannedFiles.SelectMany( x => x.Classes ).ToList();
        public List<InterfaceDeclarationSyntax> Interfaces => _scannedFiles.SelectMany( x => x.Interfaces ).ToList();
        public List<StructDeclarationSyntax> Structs => _scannedFiles.SelectMany( x => x.Structs ).ToList();
        public List<RecordDeclarationSyntax> Records => _scannedFiles.SelectMany( x => x.Records ).ToList();
    }
}