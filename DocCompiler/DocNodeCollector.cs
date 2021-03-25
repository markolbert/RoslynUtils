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
using System.Xml;
using System.Xml.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace J4JSoftware.DocCompiler
{
    public class DocNodeCollector : CSharpSyntaxWalker
    {
        private readonly ParsedProjectFactory _parsedFactory;
        private readonly IJ4JLogger? _logger;

        private ParsedSourceFile? _curFile;

        public DocNodeCollector(
            ParsedProjectFactory parsedFactory,
            IJ4JLogger? logger
        )
        {
            _parsedFactory = parsedFactory;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public ParsedProject? ParseProject( string projPath )
        {
            var retVal = _parsedFactory.Create( projPath );

            retVal.AddParsedFiles( retVal.SourceCodeFiles.Select( ParseSourceFile ) );

            return retVal;
        }

        public ParsedSourceFile ParseSourceFile( string filePath )
        {
            _curFile = new ParsedSourceFile();

            if( !File.Exists( filePath ) )
            {
                _logger?.Error<string>("Source file '{0}' does not exist", filePath);
                return _curFile;
            }

            _curFile.SourceFilePath = filePath;

            try
            {
                using var fileStream = File.OpenRead( filePath );
                var srcText = SourceText.From( fileStream );

                var syntaxTree  = CSharpSyntaxTree.ParseText( srcText );
                _curFile.RootNode = syntaxTree.GetRoot();

                Visit( _curFile.RootNode );
            }
            catch( Exception e )
            {
                _logger?.Error<string>("Parsing failed, exception was '{0}'", e.Message  );
                return _curFile;
            }

            _curFile.IsParsed = true;

            return _curFile;
        }

        public override void VisitUsingStatement( UsingStatementSyntax node )
        {
            base.VisitUsingStatement( node );

            _curFile!.Usings.Add( node );
        }

        public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
        {
            base.VisitNamespaceDeclaration( node );

            _curFile!.Namespaces.Add( node );
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            base.VisitClassDeclaration( node );

            _curFile!.Classes.Add( node );
        }

        public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
        {
            base.VisitInterfaceDeclaration( node );

            _curFile!.Interfaces.Add( node );
        }

        public override void VisitStructDeclaration( StructDeclarationSyntax node )
        {
            base.VisitStructDeclaration( node );
            
            _curFile!.Structs.Add( node );
        }

        public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
        {
            base.VisitRecordDeclaration( node );

            _curFile!.Records.Add( node );
        }
    }
}