using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class FullyQualifiedNames : IFullyQualifiedNames
    {
        private readonly Dictionary<SyntaxKind, IFullyQualifiedName> _namers = new();
        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        public FullyQualifiedNames(
            IEnumerable<IFullyQualifiedName> namers,
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger?.SetLoggedType(GetType());

            var temp = namers.ToList();

            foreach( var syntaxNamer in temp )
            {
                foreach( var supportedKind in syntaxNamer.SupportedKinds )
                {
                    if( _namers.ContainsKey( supportedKind ) )
                        _logger?.Error( "Skipping duplicate IFullyQualifiedName for {0}", supportedKind );
                    else _namers.Add( supportedKind, syntaxNamer );
                }
            }
        }

        public bool Supports( SyntaxNode node ) => _namers.ContainsKey( node.Kind() );

        public bool GetName( SyntaxNode node, out string? result )
        {
            return GetNameInternal( node, false, out result );
            //result = null;

            //var nodeKind = node.Kind();

            //// tuples are handled differently because they derive from/depend on other types
            //return nodeKind switch
            //{
            //    SyntaxKind.TupleType => GetTupleTypeName( node, out result ),
            //    SyntaxKind.TupleElement => GetTupleElementName( node, out result ),
            //    SyntaxKind.MethodDeclaration => GetMethodName( node, out result ),
            //    _ => _namers.ContainsKey( nodeKind ) switch
            //    {
            //        true => _namers[ nodeKind ].GetName( node, out result ),
            //        _ => unsupported()
            //    }
            //};

            //bool unsupported()
            //{
            //    _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );
            //    return false;
            //}
        }

        public bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            return GetNameInternal( node, true, out result );
            //result = null;

            //var nodeKind = node.Kind();

            //// tuples are handled differently because they derive from/depend on other types
            //return nodeKind switch
            //{
            //    SyntaxKind.TupleType => GetTupleTypeFullyQualifiedName( node, out result ),
            //    SyntaxKind.TupleElement => GetTupleElementFullyQualifiedName( node, out result ),
            //    _ => _namers.ContainsKey( nodeKind ) switch
            //    {
            //        true => _namers[ nodeKind ].GetFullyQualifiedName( node, out result ),
            //        _ => unsupported()
            //    }
            //};

            //bool unsupported()
            //{
            //    _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );
            //    return false;
            //}
        }

        private bool GetNameInternal( SyntaxNode node, bool fullyQualified, bool inclParams, out string? result )
        {
            result = null;

            var nodeKind = node.Kind();

            // tuples are handled differently because they derive from/depend on other types
            return nodeKind switch
            {
                SyntaxKind.TupleType => GetTupleTypeName( node, fullyQualified, out result ),
                SyntaxKind.TupleElement => GetTupleElementName( node, fullyQualified, out result ),
                SyntaxKind.MethodDeclaration => GetMethodName( node, fullyQualified, inclParams, out result ),
                SyntaxKind.ParameterList => GetParameterListName( node, out result ),
                SyntaxKind.Parameter => GetParameterName( node, out result ),

                _ => unsupported()
            };

            bool unsupported()
            {
                _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );
                return false;
            }
        }

        public bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            var nodeKind = node.Kind();

            if( _namers.ContainsKey( nodeKind ) )
                return _namers[ nodeKind ].GetIdentifierTokens( node, out result );

            _logger?.Error("Unsupported SyntaxKind {0}", nodeKind);

            result = Enumerable.Empty<BasicIdentifier>();

            return false;
        }

        public IEnumerable<IIdentifier> GetIdentifierTokens( SyntaxNode node )
        {
            return node.Kind() switch
            {
                SyntaxKind.MethodDeclaration => child_single_token(),
                SyntaxKind.ParameterList => grandchild_single_token(),
                _ => unsupported()
            };

            IEnumerable<IIdentifier> child_single_token() => node.ChildTokens()
                .Where( x => x.IsKind( SyntaxKind.IdentifierToken ) )
                .Select( x => new BasicIdentifier( x ) );

            IEnumerable<IIdentifier> grandchild_single_token()=> node.ChildNodes().Where( x => x.IsKind( SyntaxKind.Parameter ) )
                .SelectMany( x => x.ChildTokens()
                    .Where( y => y.IsKind( SyntaxKind.IdentifierToken ) )
                    .Select( y => new BasicIdentifier( y ) ) );

            IEnumerable<IIdentifier> unsupported()
            {
                _logger?.Error( "Unsupported SyntaxKind '{0}'", node.Kind() );
                return Enumerable.Empty<IIdentifier>();
            }
        }

        private bool GetTupleTypeName( SyntaxNode node, bool fullyQualified, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleType ) )
            {
                _logger?.Error("SyntaxNode is not a TupleType");
                return false;
            }

            var sb = new StringBuilder();

            foreach( var elementNode in node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
            {
                if( !GetTupleElementName( elementNode, fullyQualified, out var elementName ) )
                    return false;

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( elementName );
            }

            result = $"({sb.ToString()})";

            return true;
        }

        //private bool GetTupleTypeFullyQualifiedName( SyntaxNode node, out string? result )
        //{
        //    result = null;

        //    if( !node.IsKind( SyntaxKind.TupleType ) )
        //    {
        //        _logger?.Error("SyntaxNode is not a TupleType");
        //        return false;
        //    }

        //    var sb = new StringBuilder();

        //    foreach( var elementNode in node.ChildNodes()
        //        .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
        //    {
        //        if( !GetTupleElementFullyQualifiedName( elementNode, out var elementName ) )
        //            return false;

        //        if( sb.Length > 0 )
        //            sb.Append( ", " );

        //        sb.Append( elementName );
        //    }

        //    result = $"({sb.ToString()})";

        //    return true;
        //}

        private bool GetTupleElementName( SyntaxNode node, bool fullyQualified, out string? result )
        {
            result = null;

            if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
                return false;

            if( typeNode == null )
            {
                _logger?.Error( "Could not find type node within TupleElement node" );
                return false;
            }

            if( !GetNameInternal( typeNode, fullyQualified, true, out var typeName ) )
            {
                _logger?.Error( "Could not get name for type node" );
                return false;
            }

            result = $"{typeName} {GetIdentifierTokens( node ).First().Name}";

            return true;
        }

        private bool GetTypeNodeFromTupleElement( SyntaxNode node, out SyntaxNode? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleElement ) )
            {
                _logger?.Error( "SyntaxNode is a {0}, not a SyntaxKind.TupleElement", node.Kind() );
                return false;
            }

            result = node.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            if( result == null )
                _logger?.Error( "Could not find type node within TupleElement node" );

            return result != null;
        }

        private bool GetMethodName( SyntaxNode node, bool fullyQualified, bool inclParams, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.MethodDeclaration ) )
                return false;

            var sb = new StringBuilder( GetIdentifierTokens( node ).First().Name );
            sb.Append( "(" );

            if( inclParams )
            {
                // if we have a parameter list append its textual representation
                var plNode = node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.ParameterList ) );

                if( plNode != null )
                {
                    if( !GetParameterListName( plNode, out var plText ) )
                    {
                        _logger?.Error<string>( "Could not get ParameterList text for {0}", sb.ToString() );
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

        private bool GetParameterListName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.ParameterList ) )
                return false;

            result = string.Join( ", ", GetIdentifierTokens( node )
                .Select( x => x.Name ) );

            return true;
        }

        private bool GetParameterName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.Parameter ) )
                return false;

            result = string.Join( ", ", GetIdentifierTokens( node )
                .Select( x => x.Name ) );

            return true;
        }

        //private bool GetTupleElementFullyQualifiedName( SyntaxNode node, out string? result )
        //{
        //    result = null;

        //    if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
        //        return false;

        //    if( typeNode == null )
        //    {
        //        _logger?.Error( "Could not find type node within TupleElement node" );
        //        return false;
        //    }

        //    if( !GetFullyQualifiedName( typeNode, out var typeName ) )
        //    {
        //        _logger?.Error("Could not get fully-qualified name for type node");
        //        return false;
        //    }

        //    var idToken = node.ChildTokens().First( x => x.IsKind( SyntaxKind.IdentifierToken ) );

        //    result = $"{typeName} {idToken.Text}";

        //    return true;
        //}
    }
}
