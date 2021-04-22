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
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public abstract class NodeNamesBase
    {
        protected NodeNamesBase(
            INodeIdentifierTokens idTokens,
            IJ4JLogger? logger
        )
        {
            IdentifierTokens = idTokens;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        public bool ThrowOnUnsupported { get; set; }

        protected IJ4JLogger? Logger { get; }
        protected INodeIdentifierTokens IdentifierTokens { get; }

        protected SyntaxNode CurrentNode { get; set; }
        protected bool IncludeTypeParameters { get; set; }

        protected virtual ResolvedNameState GetNameInternal( SyntaxNode node, out string? result )
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

            ResolvedNameState unsupported()
            {
                Logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );

                if( ThrowOnUnsupported )
                    throw new SyntaxNodeException( "Unsupported SyntaxNode", null, nodeKind );

                return ResolvedNameState.Failed;
            }
        }

        protected virtual ResolvedNameState GetMethodName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.MethodDeclaration, out result ) )
                return ResolvedNameState.Failed;

            var sb = new StringBuilder( IdentifierTokens.GetTokens( node ).First().Name );
            sb.Append( "(" );

            if( IncludeTypeParameters )
            {
                // if we have a parameter list append its textual representation
                var plNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.ParameterList ) );

                if( plNode != null )
                {
                    if( GetParameterListName( plNode, out var plText ) == ResolvedNameState.Failed )
                    {
                        Logger?.Error<string>( "Could not get ParameterList text for {0}", sb.ToString() );
                        return ResolvedNameState.Failed;
                    }

                    if( !string.IsNullOrEmpty( plText! ) )
                        sb.Append( $" {plText[0]} " );
                }
            }

            sb.Append( ")" );

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual ResolvedNameState GetNamedTypeName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !SyntaxCollections.DocumentedTypeKinds.Any( node.IsKind ) )
            {
                Logger?.Error( "SyntaxNode is not a supported kind of named type node" );
                return ResolvedNameState.Failed;
            }

            var idTokens = IdentifierTokens.GetTokens( node );

            var sb = new StringBuilder( idTokens.First().Name );

            if( !IncludeTypeParameters ) 
                return ValidateName( sb.ToString(), out result );

            // if we have a type parameter list append its textual representation
            var tplNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) );

            if( tplNode != null )
            {
                if( GetNameInternal( tplNode, out var tplText ) == ResolvedNameState.Failed )
                {
                    Logger?.Error<string>( "Could not get TypeParameterList name for {0}", sb.ToString() );
                    return ResolvedNameState.Failed;
                }

                sb.Append( tplText! );
            }

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual ResolvedNameState GetNamespaceName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.NamespaceDeclaration, out result ) )
                return ResolvedNameState.Failed;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( string.Join( ".", idTokens.Select( x => x.Name ) ), out result );
        }

        protected virtual ResolvedNameState GetParameterListName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.ParameterList, out result ) )
                return ResolvedNameState.Failed;

            return ValidateName( string.Join( ", ", 
                IdentifierTokens.GetTokens( node ).Select( x => x.Name ) ), out result );
        }

        protected virtual ResolvedNameState GetParameterName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.Parameter, out result ) )
                return ResolvedNameState.Failed;

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

        protected virtual ResolvedNameState GetTupleElementName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
                return ResolvedNameState.Failed;

            if( typeNode == null )
            {
                Logger?.Error( "Could not find type node within TupleElement node" );
                return ResolvedNameState.Failed;
            }

            if( GetNameInternal( typeNode, out var typeName ) == ResolvedNameState.Failed )
            {
                Logger?.Error( "Could not get name for type node" );
                return ResolvedNameState.Failed;
            }

            return ValidateName($"{typeName} {IdentifierTokens.GetTokens( node ).First().Name}", out result);
        }

        protected virtual ResolvedNameState GetTupleTypeName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.TupleType, out result ) )
                return ResolvedNameState.Failed;

            var sb = new StringBuilder();

            foreach( var elementNode in node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
            {
                if( GetTupleElementName( elementNode, out var elementName ) == ResolvedNameState.Failed )
                    return ResolvedNameState.Failed;

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( elementName );
            }

            return ValidateName( sb.ToString(), out result );
        }

        protected virtual ResolvedNameState GetTypeParameterListName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.TypeParameterList, out result ) )
                return ResolvedNameState.Failed;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( $"<{string.Join( ", ", idTokens.Select( x => x.Name ) )}>", out result );
        }

        protected virtual ResolvedNameState GetUsingName( SyntaxNode node, out string? result )
        {
            if( !ValidateNode( node, SyntaxKind.UsingDirective, out result ) )
                return ResolvedNameState.Failed;

            var idTokens = IdentifierTokens.GetTokens( node );

            return ValidateName( string.Join( ".", idTokens.Select( x => x.Name ) ), out result );
        }

        private bool ValidateNode( SyntaxNode node, SyntaxKind kind, out string? result )
        {
            result = null;

            if( node.IsKind( kind ) )
                return true;

            Logger?.Error("SyntaxNode is not a {0}", kind);

            return false;
        }

        protected ResolvedNameState ValidateName( string? text, out string? result )
        {
            result = null;

            if( string.IsNullOrEmpty( text ) )
            {
                Logger?.Error("Empty or undefined name");
                return ResolvedNameState.Failed;
            }

            result = text;

            return ResolvedNameState.FullyResolved;
        }
    }
}