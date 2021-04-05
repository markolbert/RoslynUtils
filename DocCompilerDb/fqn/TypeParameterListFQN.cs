﻿using System.Collections.Generic;
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

            result = $"<{string.Join( ", ", idTokens )}>";

            return true;
        }

        public override bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxToken> result )
        {
            if( !base.GetIdentifierTokens( node, out result ) )
                return false;

            result = node.DescendantTokens().Where( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            return true;
        }
    }
}