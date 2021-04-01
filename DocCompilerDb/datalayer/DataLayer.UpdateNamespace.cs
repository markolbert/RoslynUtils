#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public partial class DataLayer
    {
        public bool UpdateNamespace( SyntaxNode node )
        {
            if( node.Kind() != SyntaxKind.NamespaceDeclaration )
            {
                _logger?.Error( "Supplied SyntaxNode is not a NamespaceDeclaration" );
                return false;
            }

            if( node.Parent == null )
            {
                _logger?.Error( "Supplied NamespaceDeclaration node has no parent" );
                return false;
            }

            SyntaxKind? parentKind = null;
            object? parentEntity = null;

            switch( node.Parent.Kind() )
            {
                case SyntaxKind.NamespaceDeclaration:
                    parentKind = SyntaxKind.NamespaceDeclaration;

                    parentEntity = _dbContext.Namespaces.FirstOrDefault(n=>n.Name == node.p)
                    break;

                case SyntaxKind.CompilationUnit:
                    parentKind = SyntaxKind.CompilationUnit;
                    break;

                default:
                    _logger?.Error("Unsupported parent node type '{0}' of NamespaceDeclaration node");
                    return false;
            }

            // grab all the IdentifierToken child tokens as they contain the dotted
            // elements of the namespace's name
            var qualifiedName = node.ChildNodes()
                .FirstOrDefault( x => x.Kind() == SyntaxKind.QualifiedName );

            if( qualifiedName == null )
            {
                _logger?.Error( "Supplied NamespaceDeclaration node has no QualifiedName" );
                return false;
            }

            var identifierNodes = qualifiedName.DescendantNodes()
                .Where( x => x.Kind() == SyntaxKind.IdentifierName );

            var name = string.Join( ".", identifierNodes );

            var nsDb = _dbContext.Namespaces
                .FirstOrDefault( x => x.Name == name );

            // we don't try to update the namespace's container, as that may not be defined yet
            // if it's a class
            if( nsDb == null )
            {
                nsDb = new Namespace { Name = name };
                _dbContext.Namespaces.Add( nsDb );
            }
            else nsDb.Deprecated = false;

            _dbContext.SaveChanges();

            return true;
        }
    }
}