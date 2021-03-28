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
    public class DocNodeWalker : CSharpSyntaxWalker
    {
        private StandaloneFile _scanFile;

        internal DocNodeWalker(
            StandaloneFile scanFile )
        {
            _scanFile = scanFile;
        }

        public void Visit()
        {
            base.Visit( _scanFile.RootNode );
        }

        public override void VisitUsingStatement( UsingStatementSyntax node )
        {
            base.VisitUsingStatement( node );

            _scanFile.Usings.Add( node );
        }

        public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
        {
            base.VisitNamespaceDeclaration( node );

            _scanFile.Namespaces.Add( node );
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            base.VisitClassDeclaration( node );

            _scanFile.Classes.Add( node );
        }

        public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
        {
            base.VisitInterfaceDeclaration( node );

            _scanFile.Interfaces.Add( node );
        }

        public override void VisitStructDeclaration( StructDeclarationSyntax node )
        {
            base.VisitStructDeclaration( node );
            
            _scanFile.Structs.Add( node );
        }

        public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
        {
            base.VisitRecordDeclaration( node );

            _scanFile.Records.Add( node );
        }
    }
}