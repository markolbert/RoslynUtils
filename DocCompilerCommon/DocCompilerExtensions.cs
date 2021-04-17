using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public static class DocCompilerExtensions
    {
        public static SyntaxKind[] AccessKinds = new[]
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword
        };

        public static SyntaxToken GetChildToken( this SyntaxNode node, SyntaxKind kind )
            => node.ChildTokens().FirstOrDefault( x => x.IsKind( kind ) );

        public static bool HasChildNode( this SyntaxNode node, SyntaxKind kind )
            => node.ChildNodes().Any( x => x.IsKind( kind ) );

        public static List<SyntaxNode> GetChildNodes( this SyntaxNode node, SyntaxKind kind )
            => node.ChildNodes().Where( x => x.IsKind( kind ) ).ToList();

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

        public static bool GetGeneralTypeConstraints( 
            this SyntaxNode typeConstraintNode,
            out GeneralTypeConstraints result )
        {
            result = GeneralTypeConstraints.None;

            if( !typeConstraintNode.IsKind( SyntaxKind.TypeParameterConstraintClause ) )
                return false;

            if( typeConstraintNode.HasChildNode( SyntaxKind.ConstructorConstraint ) )
                result |= GeneralTypeConstraints.New;

            if( typeConstraintNode.HasChildNode( SyntaxKind.ClassConstraint ) )
                result |= GeneralTypeConstraints.Class;

            if( typeConstraintNode.HasChildNode( SyntaxKind.StructConstraint ) )
                result |= GeneralTypeConstraints.Struct;

            return true;
        }

        public static Accessibility GetAccessibility( this SyntaxNode node )
        {
            var accessTokens = node.ChildTokens()
                .Where( x => AccessKinds.Any( y => x.RawKind == (int) y ) )
                .ToList();

            return accessTokens.Count switch
            {
                0 => Accessibility.Private,
                1 => accessTokens[ 0 ].Kind() switch
                {
                    SyntaxKind.PublicKeyword => Accessibility.Public,
                    SyntaxKind.PrivateKeyword => Accessibility.Private,
                    SyntaxKind.InternalKeyword => Accessibility.Internal,
                    SyntaxKind.ProtectedKeyword => Accessibility.Protected,
                    _ => throw new InvalidEnumArgumentException(
                        $"Unsupported access token type {accessTokens[ 0 ].Kind()}" )
                },
                2 => protected_internal(),
                _ => throw new ArgumentException( "More than two access tokens encountered" )
            };

            Accessibility protected_internal()
            {
                var numProtected = accessTokens.Count( x => x.IsKind( SyntaxKind.ProtectedKeyword ) );
                var numInternal = accessTokens.Count( x => x.IsKind( SyntaxKind.InternalKeyword ) );

                if( numProtected == 1 && numInternal == 1 )
                    return Accessibility.ProtectedInternal;

                throw new ArgumentException(
                    "Unsupported combination of protected and internal access tokens encountered" );
            }
        }
    }
}
