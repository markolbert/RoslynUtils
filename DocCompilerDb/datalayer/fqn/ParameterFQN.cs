using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class ParameterFQN : FullyQualifiedName
    {
        public ParameterFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.Parameter )
        {
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
            => GetName( node, out result );

        public override bool GetName( SyntaxNode node, out string? result )
        {
            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if( !GetIdentifierTokens( node, out var idTokens ) )
                return false;

            result = $"<{string.Join( ", ", idTokens.Select(x=>x.Name) )}>";

            return true;
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            result = new List<BasicIdentifier> { new BasicIdentifier( node.ChildTokens().ToArray() ) };

            return true;
        }

        public bool GetParameterInfo( SyntaxNode node, out ParameterInfo? result )
        {
            result = null;

            if( !GetName( node, out var parameterName ) )
            {
                Logger?.Error( "Could not get name for parameter SyntaxNode" );
                return false;
            }

            var typeNode = node.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            if( typeNode == null )
            {
                Logger?.Error("Could not find type SyntaxNode for parameter SyntaxNode");
                return false;
            }

            var parameterIndex = node.Parent!
                .ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.Parameter ) )
                .ToList()
                .FindIndex( x => x == node );

            var modifiers = node.ChildTokens()
                .Where( x => SyntaxCollections.ArgumentModifiers.Any( y => x.IsKind( y ) ) )
                .ToList();

            result = new ParameterInfo( parameterName!, parameterIndex, modifiers, typeNode );

            return true;
        }
    }
}