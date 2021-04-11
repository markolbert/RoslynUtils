using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public static class DocCompilerExtensions
    {
        public static SyntaxToken GetChildToken( this SyntaxNode node, SyntaxKind kind )
            => node.ChildTokens().FirstOrDefault( x => x.IsKind( kind ) );

        public static bool HasChildNode( this SyntaxNode node, SyntaxKind kind )
            => node.ChildNodes().Any( x => x.IsKind( kind ) );

        public static bool GetChildNode( this SyntaxNode node, SyntaxKind kind, out SyntaxNode? result )
        {
            result = node.ChildNodes().FirstOrDefault( x => x.IsKind( kind ) );

            return result != null;
        }

        public static bool GetChildNode( this SyntaxNode node, out SyntaxNode? result, params SyntaxKind[] altKinds )
        {
            result = node.ChildNodes().FirstOrDefault( x => altKinds.Any( x.IsKind ) );

            return result != null;
        }

        public static bool GetDescendantNode( this SyntaxNode node, out SyntaxNode? result, params SyntaxKind[] kinds )
        {
            result = null;

            var curNode = node;

            foreach( var curKind in kinds )
            {
                if( curNode.GetChildNode( curKind, out var childNode ) )
                    return false;

                curNode = childNode;

                if( curNode == null )
                    return false;
            }

            result = curNode;

            return true;
        }
    }
}
