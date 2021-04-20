using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NodeIdentifierTokens : INodeIdentifierTokens
    {
        private readonly IJ4JLogger? _logger;

        public NodeIdentifierTokens(
            IJ4JLogger? logger
        )
        {
            _logger = logger;
            _logger?.SetLoggedType(GetType());
        }

        public IEnumerable<IIdentifier> GetTokens( SyntaxNode node )
        {
            return node.Kind() switch
            {
                SyntaxKind.ClassDeclaration => GetNamedTypeTokens( node ),
                SyntaxKind.InterfaceDeclaration => GetNamedTypeTokens( node ),
                SyntaxKind.GenericName => GetChildIdentifierTokens( node, SyntaxKind.GenericName ),
                SyntaxKind.IdentifierName => GetChildIdentifierTokens( node, SyntaxKind.IdentifierName ),
                SyntaxKind.MethodDeclaration => GetMethodTokens( node ),
                SyntaxKind.PredefinedType => GetPredefinedTypeTokens( node ),
                SyntaxKind.NamespaceDeclaration => GetNamespaceTokens( node ),
                SyntaxKind.Parameter => GetParameterTokens( node  ),
                SyntaxKind.ParameterList => GetParameterListTokens( node ),
                SyntaxKind.RecordDeclaration => GetNamedTypeTokens( node ),
                SyntaxKind.SimpleBaseType => GetSimpleBaseTypeTokens(node),
                SyntaxKind.StructDeclaration => GetNamedTypeTokens( node ),
                SyntaxKind.TypeParameterList => GetTypeParameterListTokens( node ),
                SyntaxKind.UsingDirective => GetUsingTokens( node ),
                _ => unsupported()
            };

            IEnumerable<IIdentifier> unsupported()
            {
                _logger?.Error( "Unsupported SyntaxKind '{0}'", node.Kind() );
                return Enumerable.Empty<IIdentifier>();
            }
        }

        private IEnumerable<IIdentifier> GetGenericNameTokens( SyntaxNode node )
            => GetChildIdentifierTokens( node, SyntaxKind.GenericName );

        private IEnumerable<IIdentifier> GetChildIdentifierTokens( SyntaxNode node, SyntaxKind requiredKind )
        {
            if( !ValidateNode( node, requiredKind ) )
                return Enumerable.Empty<IIdentifier>();

            var retVal = node.ChildTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No {0} IdentifierToken found", requiredKind);

            return retVal;
        }

        private IEnumerable<IIdentifier> GetPredefinedTypeTokens( SyntaxNode node ) =>
            !ValidateNode( node, SyntaxKind.PredefinedType )
                ? Enumerable.Empty<IIdentifier>()
                : new List<IIdentifier> { new TextIdentifier( node.ToString() ) };

        private IEnumerable<IIdentifier> GetMethodTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.MethodDeclaration ) )
                return Enumerable.Empty<IIdentifier>();

            var retVal = node.ChildTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No method IdentifierToken found");

            return retVal;
        }

        private IEnumerable<IIdentifier> GetNamedTypeTokens( SyntaxNode node )
        {
            if( !SyntaxCollections.DocumentedTypeKinds.Any( x => node.IsKind( x ) ) )
            {
                _logger?.Error( "SyntaxNode is not a supported kind of named type node" );
                return Enumerable.Empty<IIdentifier>();
            }

            var retVal = node.ChildTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No named type IdentifierToken found");

            return retVal;
        }

        private IEnumerable<IIdentifier> GetNamespaceTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.NamespaceDeclaration ) )
                return Enumerable.Empty<IIdentifier>();

            var containerNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.QualifiedName ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

            if( containerNode == null )
                return new List<IIdentifier>();

            var retVal = containerNode.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No namespace IdentifierToken found");

            return retVal;
        }

        private IEnumerable<IIdentifier> GetParameterListTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.ParameterList ) )
                return Enumerable.Empty<IIdentifier>();

            var retVal = node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.Parameter ) )
                .Select(x=>new ParameterIdentifier(x)  )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No parameter list IdentifierTokens found");

            return retVal;
        }

        private IEnumerable<IIdentifier> GetParameterTokens( SyntaxNode node ) =>
            ValidateNode( node, SyntaxKind.Parameter )
                ? new List<IIdentifier> { new ParameterIdentifier( node ) }
                : Enumerable.Empty<IIdentifier>();

        private IEnumerable<IIdentifier> GetSimpleBaseTypeTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.SimpleBaseType ) )
                return Enumerable.Empty<IIdentifier>();

            var childNode = node.ChildNodes().FirstOrDefault();

            if( childNode == null )
            {
                _logger?.Error("SimpleBaseType node does not have any child nodes");
                return Enumerable.Empty<IIdentifier>();
            }

            return GetTokens( childNode );
        }

        private IEnumerable<IIdentifier> GetTypeParameterListTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.TypeParameterList ) )
                return Enumerable.Empty<IIdentifier>();

            var retVal = node.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No type parameter list IdentifierTokens found");

            return retVal;
        }

        private IEnumerable<IIdentifier> GetUsingTokens( SyntaxNode node )
        {
            if( !ValidateNode( node, SyntaxKind.MethodDeclaration ) )
                return Enumerable.Empty<IIdentifier>();

            var containerNode = node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.NameEquals ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.QualifiedName ) );

            containerNode ??= node.ChildNodes()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

            if (containerNode == null)
                return new List<IIdentifier>();

            var retVal = containerNode.DescendantTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new SingleTokenIdentifier( x ) )
                .ToList();

            if( !retVal.Any() )
                _logger!.Error("No using declaration IdentifierToken found");

            return retVal;
        }

        private bool ValidateNode( SyntaxNode node, SyntaxKind kind )
        {
            if( node.IsKind( kind ) ) 
                return true;

            _logger?.Error( "SyntaxNode is not a {0}", kind );
            
            return false;
        }
    }
}
