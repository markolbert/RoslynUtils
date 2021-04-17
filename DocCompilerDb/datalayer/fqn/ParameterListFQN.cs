using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class ParameterListFQN : FullyQualifiedName
    {
        private readonly ParameterFQN _paramFQN;

        public ParameterListFQN(
            ParameterFQN paramFQN,
            IJ4JLogger? logger
        )
            : base( logger, SyntaxKind.ParameterList )
        {
            _paramFQN = paramFQN;
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
            => GetName( node, out result );

        public override bool GetName( SyntaxNode node, out string? result )
        {
            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if( !GetIdentifierTokens( node, out var idTokens ) )
                return false;

            result = string.Join( ", ", idTokens );

            return true;
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            throw new NotImplementedException();

            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            result = node.ChildNodes().Where( x => x.IsKind( SyntaxKind.Parameter ) )
                .SelectMany( x => x.ChildTokens()
                    .Where( y => y.IsKind( SyntaxKind.IdentifierToken ) )
                    .Select( y => new BasicIdentifier( y ) ) );

            return true;
        }

        public List<ParameterInfo> GetParametersInfo( SyntaxNode? paramListNode )
        {
            var retVal = new List<ParameterInfo>();

            if( paramListNode == null )
                return retVal;

            if( !base.GetName( paramListNode, out _ ) )
                return retVal;

            foreach( var pNode in paramListNode.ChildNodes()
                .Where( x => SyntaxCollections.TypeNodeKinds.Any(x.IsKind) ))
            {
                if( !_paramFQN.GetParameterInfo( pNode, out var pInfo ) )
                {
                    Logger?.Error("Could not get parameter info for  parameter SyntaxNode");
                    return new List<ParameterInfo>();
                }

                retVal.Add( pInfo! );
            }

            return retVal;
        }
    }
}