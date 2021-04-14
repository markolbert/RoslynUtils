using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public abstract class SyntaxNodeProcessor : EntityProcessor<NodeContext>
    {
        public static SyntaxKind[] AccessKinds = new[]
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword
        };

        protected SyntaxNodeProcessor( 
            IFullyQualifiedNames fqNamers, 
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
        }

        protected Accessibility GetAccessibility( SyntaxNode node )
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

        protected bool HasChildNode( NodeContext nodeContext, SyntaxKind kind )
            => nodeContext.Node.ChildNodes().Any( x => x.IsKind( kind ) );

        protected bool HasChildNode( SyntaxNode node, SyntaxKind kind )
            => node.ChildNodes().Any( x => x.IsKind( kind ) );

        //protected bool GetGeneralTypeConstraints( SyntaxNode node, out GeneralTypeConstraints result )
        //{
        //    result = GeneralTypeConstraints.None;

        //    if( !node.IsKind( SyntaxKind.TypeParameterConstraintClause ) )
        //        return false;

        //    if( HasChildNode( node, SyntaxKind.ConstructorConstraint ) )
        //        result |= GeneralTypeConstraints.New;

        //    if( HasChildNode( node, SyntaxKind.ClassConstraint ) )
        //        result |= GeneralTypeConstraints.Class;

        //    if( HasChildNode( node, SyntaxKind.StructConstraint ) )
        //        result |= GeneralTypeConstraints.Struct;

        //    return true;
        //}
    }
}