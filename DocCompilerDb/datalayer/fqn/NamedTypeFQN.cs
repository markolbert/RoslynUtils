using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NamedTypeFQN : FullyQualifiedName
    {
        private readonly NamespaceFQN _nsFQN;
        private readonly TypeParameterListFQN _tplFQN;

        public NamedTypeFQN(
            NamespaceFQN nsFQN,
            TypeParameterListFQN tplFQN,
            IJ4JLogger? logger
        )
            : base( logger, 
                SyntaxKind.ClassDeclaration, 
                SyntaxKind.InterfaceDeclaration, 
                SyntaxKind.RecordDeclaration,
                SyntaxKind.StructDeclaration )
        {
            _nsFQN = nsFQN;
            _tplFQN = tplFQN;
        }

        public bool GetFullyQualifiedNameWithoutTypeParameters( SyntaxNode node, out string? result )
            => GetFullyQualifiedNameInternal( node, false, out result );

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
            => GetFullyQualifiedNameInternal( node, true, out result );

        private bool GetFullyQualifiedNameInternal( SyntaxNode node, bool inclTypeParams, out string? result )
        {
            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if( !GetName( node, inclTypeParams, out var startName ) )
                return false;

            var sb = new StringBuilder( startName );

            // move up the parent container tree until we find a namespace
            // or have topped out
            var containerNode = node.Parent;

            while( true )
            {
                if( containerNode == null || containerNode.IsKind( SyntaxKind.NamespaceDeclaration ) )
                    break;

                containerNode = containerNode.Parent;
            }

            if( containerNode == null )
            {
                result = sb.ToString();
                return true;
            }

            // if we hit a NamespaceDeclaration follow it up
            if( !_nsFQN.GetFullyQualifiedName( containerNode, out var nsName ) )
            {
                Logger?.Error<string>( "Could not retrieve fully-qualified Namespace name for named type {0}",
                    sb.ToString() );
                return false;
            }

            sb.Insert( 0, $"{nsName}." );

            result = sb.ToString();

            return true;
        }

        public override bool GetName( SyntaxNode node, out string? result )
            => GetName( node, true, out result );

        public bool GetNameWithoutTypeParameters( SyntaxNode node, out string? result )
            => GetName( node, false, out result );

        private bool GetName( SyntaxNode node, bool inclTypeParams, out string? result )
        {
            if( !base.GetName( node, out result ) )
                return false;

            if (!GetIdentifierTokens(node, out var idTokens))
                return false;

            var sb = new StringBuilder( idTokens.First().ToString() );

            var curNode = node;

            while( ( curNode = curNode.Parent ) != null
                   && SupportedKinds.Any( x => x == curNode.Kind() ) )
            {
                if( !GetName( curNode, out var curName ) )
                    return false;

                sb.Insert( 0, $"{curName}." );
            }

            if( inclTypeParams )
            {
                // if we have a type parameter list append its textual representation
                var tplNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) );

                if( tplNode != null )
                {
                    if( !_tplFQN.GetName( tplNode, out var tplText ) )
                    {
                        Logger?.Error<string>( "Could not get TypeParameterList text for {0}", sb.ToString() );
                        return false;
                    }

                    sb.Append( tplText! );
                }
            }

            result = sb.ToString();

            return !string.IsNullOrEmpty(result);
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxToken> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            result = node.ChildTokens().Where( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            return true;
        }
    }
}