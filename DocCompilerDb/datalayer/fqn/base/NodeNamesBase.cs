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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace J4JSoftware.DocCompiler
{
    public abstract class NodeNamesBase
    {
        protected NodeNamesBase(
            INodeIdentifierTokens idTokens,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            IdentifierTokens = idTokens;
            DbContext = dbContext;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected INodeIdentifierTokens IdentifierTokens { get; }
        protected DocDbContext DbContext { get; }
        protected bool IncludeTypeParameters { get; set; }

        public bool ThrowOnUnsupported { get; set; }

        protected virtual bool GetNameInternal( SyntaxNode node, out string? result )
        {
            result = null;

            var nodeKind = node.Kind();

            // tuples are handled differently because they derive from/depend on other types
            return nodeKind switch
            {
                SyntaxKind.ClassDeclaration => GetNamedTypeName(node, out result ),
                SyntaxKind.InterfaceDeclaration => GetNamedTypeName(node, out result ),
                SyntaxKind.MethodDeclaration => GetMethodName( node, out result ),
                SyntaxKind.NamespaceDeclaration => GetNamespaceName(node, out result ),
                SyntaxKind.ParameterList => GetParameterListName( node, out result  ),
                SyntaxKind.Parameter => GetParameterName( node, out result  ),
                SyntaxKind.RecordDeclaration => GetNamedTypeName(node, out result ),
                SyntaxKind.StructDeclaration => GetNamedTypeName(node, out result ),
                SyntaxKind.TupleElement => GetTupleElementName( node, out result  ),
                SyntaxKind.TupleType => GetTupleTypeName( node, out result  ),
                SyntaxKind.TypeParameterList => GetTypeParameterListName(node, out result ),
                SyntaxKind.UsingDirective => GetUsingName( node, out result ),

                _ => unsupported()
            };

            bool unsupported()
            {
                Logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );

                if( ThrowOnUnsupported )
                    throw new SyntaxNodeException( "Unsupported SyntaxNode", null, nodeKind );

                return false;
            }
        }

        protected virtual bool GetMethodName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.MethodDeclaration ) )
                return false;

            var sb = new StringBuilder( IdentifierTokens.GetTokens( node ).First().Name );
            sb.Append( "(" );

            if( IncludeTypeParameters )
            {
                // if we have a parameter list append its textual representation
                var plNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.ParameterList ) );

                if( plNode != null )
                {
                    if( !GetParameterListName( plNode, out var plText ) )
                    {
                        Logger?.Error<string>( "Could not get ParameterList text for {0}", sb.ToString() );
                        return false;
                    }

                    if( !string.IsNullOrEmpty( plText! ) )
                        sb.Append( $" {plText[0]} " );
                }
            }

            sb.Append( ")" );

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual bool GetNamedTypeName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !SyntaxCollections.DocumentedTypeKinds.Any( node.IsKind ) )
            {
                Logger?.Error( "SyntaxNode is not a supported kind of named type node" );
                return false;
            }

            var idTokens = IdentifierTokens.GetTokens( node );

            var sb = new StringBuilder( idTokens.First().Name );

            if( IncludeTypeParameters )
            {
                // if we have a type parameter list append its textual representation
                var tplNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) );

                if( tplNode != null )
                {
                    if( !GetNameInternal( tplNode, out var tplText ) )
                    {
                        Logger?.Error<string>( "Could not get TypeParameterList name for {0}", sb.ToString() );
                        return false;
                    }

                    sb.Append( tplText! );
                }
            }

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual bool GetNamespaceName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.NamespaceDeclaration ) )
                return false;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( string.Join( ".", idTokens.Select( x => x.Name ) ), out result );
        }

        protected virtual bool GetParameterListName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.ParameterList ) )
                return false;

            return ValidateName( string.Join( ", ", 
                IdentifierTokens.GetTokens( node ).Select( x => x.Name ) ), out result );
        }

        protected virtual bool GetParameterName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.Parameter ) )
                return false;

            return ValidateName( IdentifierTokens.GetTokens( node )
                    .Select( x => x.Name )
                    .FirstOrDefault(), 
                out result );
        }

        protected bool GetTypeNodeFromTupleElement( SyntaxNode node, out SyntaxNode? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleElement ) )
            {
                Logger?.Error( "SyntaxNode is a {0}, not a SyntaxKind.TupleElement", node.Kind() );
                return false;
            }

            result = node.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            if( result == null )
                Logger?.Error( "Could not find type node within TupleElement node" );

            return result != null;
        }

        protected virtual bool GetTupleElementName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
                return false;

            if( typeNode == null )
            {
                Logger?.Error( "Could not find type node within TupleElement node" );
                return false;
            }

            if( !GetNameInternal( typeNode, out var typeName ) )
            {
                Logger?.Error( "Could not get name for type node" );
                return false;
            }

            return ValidateName($"{typeName} {IdentifierTokens.GetTokens( node ).First().Name}", out result);
        }

        protected virtual bool GetTupleTypeName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleType ) )
            {
                Logger?.Error("SyntaxNode is not a TupleType");
                return false;
            }

            var sb = new StringBuilder();

            foreach( var elementNode in node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
            {
                if( !GetTupleElementName( elementNode, out var elementName ) )
                    return false;

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( elementName );
            }

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual bool GetTypeParameterListName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TypeParameterList ) )
                return false;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( $"<{string.Join( ", ", idTokens.Select( x => x.Name ) )}>", out result );
        }

        protected virtual bool GetUsingName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.UsingDirective ) )
                return false;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( string.Join( ".", idTokens.Select( x => x.Name ) ), out result );
        }

        protected bool ValidateName( string? text, out string? result )
        {
            result = null;

            if( string.IsNullOrEmpty( text ) )
            {
                Logger?.Error("Empty or undefined name");
                return false;
            }

            result = text;

            return true;
        }
    }
}