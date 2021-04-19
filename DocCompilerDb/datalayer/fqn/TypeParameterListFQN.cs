using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class TypeParameterListFQN : FullyQualifiedName
    {
        public TypeParameterListFQN(
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.TypeParameterList )
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

            result = node.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new BasicIdentifier( x ) );

            return true;
        }

        public List<TypeParameterInfo> GetTypeParameterInfo( SyntaxNode? typeParamListNode )
        {
            var retVal = new List<TypeParameterInfo>();

            if( typeParamListNode == null )
                return retVal;

            if( !base.GetName( typeParamListNode, out _ ) )
                return retVal;

            var index = 0;

            var constraintNodes = typeParamListNode.Parent!
                .GetChildNodes( SyntaxKind.TypeParameterConstraintClause )
                .ToDictionary( x =>
                    x.ChildNodes().First( y => y.IsKind( SyntaxKind.IdentifierName ) ).ToString() );

            foreach( var tpNode in typeParamListNode.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TypeParameter ) ) )
            {
                var name = tpNode.ChildTokens().First( x => x.IsKind( SyntaxKind.IdentifierToken ) ).Text;

                retVal.Add( new TypeParameterInfo(
                    name,
                    index,
                    constraintNodes.ContainsKey( name ) ? constraintNodes[ name ] : null ) );

                index++;
            }

            return retVal;
        }
    }
}