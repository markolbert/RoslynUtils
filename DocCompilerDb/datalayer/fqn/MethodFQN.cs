using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    public class MethodFQN : FullyQualifiedName
    {
        private readonly NamedTypeFQN _ntFqn;
        private readonly ParameterListFQN _plFQN;

        public MethodFQN(
            NamedTypeFQN ntFQN,
            ParameterListFQN plFQN,
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.MethodDeclaration )
        {
            _ntFqn = ntFQN;
            _plFQN = plFQN;
        }

        public bool GetFullyQualifiedNameWithoutTypeParameters( SyntaxNode node, out string? result )
            => GetFullyQualifiedNameInternal( node, false, out result );

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
            => GetFullyQualifiedNameInternal( node, true, out result );

        private bool GetFullyQualifiedNameInternal( SyntaxNode node, bool inclParams, out string? result )
        {
            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if( !GetName( node, inclParams, out var startName ) )
                return false;

            var sb = new StringBuilder( startName );

            // find our declaring node's DocumentedType
            if( node.Parent == null
                || !SyntaxCollections.DocumentedTypeKinds.Any( x => node.Parent.IsKind( x ) ) )
            {
                Logger?.Error( "MethodDeclaration node is not contained withing a type node" );
                return false;
            }

            if( !_ntFqn.GetFullyQualifiedName( node.Parent, out var containerName ) )
            {
                Logger?.Error("Could not fine method's declaring type's name");
                return false;
            }

            sb.Insert( 0, $"{containerName}." );

            result = sb.ToString();

            return true;
        }

        public override bool GetName( SyntaxNode node, out string? result )
            => GetName( node, true, out result );

        public bool GetNameWithoutTypeParameters( SyntaxNode node, out string? result )
            => GetName( node, false, out result );

        private bool GetName( SyntaxNode node, bool inclParams, out string? result )
        {
            if( !base.GetName( node, out result ) )
                return false;

            if (!GetIdentifierTokens(node, out var idTokens))
                return false;

            var sb = new StringBuilder( idTokens.First().Name );
            sb.Append( "(" );

            if( inclParams )
            {
                // if we have a parameter list append its textual representation
                var plNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.ParameterList ) );

                if( plNode != null )
                {
                    if( !_plFQN.GetName( plNode, out var plText ) )
                    {
                        Logger?.Error<string>( "Could not get ParameterList text for {0}", sb.ToString() );
                        return false;
                    }

                    if( !string.IsNullOrEmpty( plText ) )
                        sb.Append( $" {plText!} " );
                }
            }

            sb.Append( ")" );

            result = sb.ToString();

            return !string.IsNullOrEmpty(result);
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            result = node.ChildTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new BasicIdentifier( x ) );

            return true;
        }
    }
}