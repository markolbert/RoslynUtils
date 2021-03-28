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

using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.DocCompiler
{
    public class DocNodeWalker : CSharpSyntaxWalker, IScanResults
    {
        private readonly IJ4JLogger? _logger;

        public DocNodeWalker(
            IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public string SourceFilePath { get; internal set; } = string.Empty;
        public bool IsParsed { get; private set; }

        public List<UsingStatementSyntax> Usings { get; } = new();
        public List<NamespaceDeclarationSyntax> Namespaces { get; } = new();
        public List<ClassDeclarationSyntax> Classes { get; } = new();
        public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();
        public List<StructDeclarationSyntax> Structs { get; } = new();
        public List<RecordDeclarationSyntax> Records { get; } = new();

        public override void Visit( SyntaxNode? node )
        {
            IsParsed = false;

            if( node == null )
            {
                _logger?.Error("Undefined root SyntaxNode"  );
                return;
            }

            Usings.Clear();
            Namespaces.Clear();
            Classes.Clear();
            Interfaces.Clear();
            Structs.Clear();
            Records.Clear();

            base.Visit( node );

            IsParsed = true;
        }

        public override void VisitUsingStatement( UsingStatementSyntax node )
        {
            base.VisitUsingStatement( node );

            Usings.Add( node );
        }

        public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
        {
            base.VisitNamespaceDeclaration( node );

            Namespaces.Add( node );
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            base.VisitClassDeclaration( node );

            Classes.Add( node );
        }

        public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
        {
            base.VisitInterfaceDeclaration( node );

            Interfaces.Add( node );
        }

        public override void VisitStructDeclaration( StructDeclarationSyntax node )
        {
            base.VisitStructDeclaration( node );
            
            Structs.Add( node );
        }

        public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
        {
            base.VisitRecordDeclaration( node );

            Records.Add( node );
        }
    }
}