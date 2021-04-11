using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.DocCompiler
{
    public class TypeInfo
    {
        private readonly List<TypeInfo> _children = new();

        public TypeInfo? Parent { get; private set; }

        public string Name { get; private set; } = string.Empty;
        public bool IsPredefined { get; private set; }
        public ReadOnlyCollection<TypeInfo> Arguments => _children.AsReadOnly();

        public NamedType? DbEntity {get; set; }

        public TypeInfo AddChild( string name, bool isPredefined = false )
        {
            var retVal = new TypeInfo
            {
                Name = name,
                IsPredefined = isPredefined,
                Parent = this
            };

            _children.Add( retVal );

            return retVal;
        }
    }

    //public class TypeFinder : ITypeFinder
    //{
    //    private readonly DocDbContext _dbContext;
    //    private readonly IJ4JLogger? _logger;

    //    public TypeFinder(
    //        DocDbContext dbContext,
    //        IJ4JLogger? logger
    //    )
    //    {
    //        _dbContext = dbContext;

    //        _logger = logger;
    //        _logger?.SetLoggedType( GetType() );
    //    }

    //    public bool GetNamedType(
    //        SyntaxNode typeNode, 
    //        DocumentedType dtContextDb, 
    //        IScannedFile scannedFile,
    //        out NamedType? result, 
    //        bool createIfMissing = true)
    //    {
    //        result = null;

    //        var rootTypeInfo = TypeInfo.CreateRootTypeInfo();

    //        if( !GetTypeInfo( typeNode, rootTypeInfo ) )
    //            return false;

    //        if( rootTypeInfo.Arguments.Count != 1 )
    //        {
    //            _logger?.Error("Invalid TypeInfo derived from scanning SyntaxNode");
    //            return false;
    //        }

    //        var typeInfo = rootTypeInfo.Arguments.First();

    //        var codeFileDb = _dbContext.CodeFiles.FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

    //        if( codeFileDb == null )
    //        {
    //            _logger?.Error<string>( "Could not find CodeFile entity in database for DocumentedType '{0}'",
    //                dtContextDb.FullyQualifiedName );

    //            return false;
    //        }

    //        // build the list of namespaces which define the context within which we'll be
    //        // searching for a NamedType
    //        var nsContexts = dtContextDb.GetNamespaceContext();
    //        nsContexts = codeFileDb.GetNamespaceContext( nsContexts );

    //        if (FindDocumentedType(typeInfo!, nsContexts, out var temp))
    //        {
    //            result = temp;
    //            return true;
    //        }

    //        result = FindExternalType( typeInfo!, nsContexts, createIfMissing );

    //        return result != null;
    //    }

    //    private bool ResolveTypeInfo( TypeInfo typeInfo )
    //    {
    //        // start by seeing if there's a DocumentedType which fits the bill

    //    }

    //    private bool FindDocumentedType(
    //        TypeInfo typeInfo,
    //        List<NamespaceContext> nsContexts,
    //        out DocumentedType? result)
    //    {
    //        result = null;

    //        foreach( var fqInfo in nsContexts)
    //        {
    //            foreach( var fqnMatch in _dbContext.DocumentedTypes
    //                .Where( x => x.FullyQualifiedName.StartsWith(fqInfo.NamespaceName) )
    //            )
    //            {
    //                // can't test for this match in EF Core LINQ because it can't be translated
    //                if( !fqnMatch.FullyQualifiedName.Equals( $"{fqInfo.NamespaceName}.{typeInfo.Name}" ) )
    //                    continue;

    //                if( fqnMatch.TypeParameters == null )
    //                {
    //                    if( typeInfo.Arguments.Count != 0 )
    //                        continue;

    //                    result = fqnMatch;
    //                    return true;
    //                }

    //                if( typeInfo.Arguments.Count != fqnMatch.TypeParameters.Count )
    //                    continue;

    //                result = fqnMatch;
    //                return true;
    //            }
    //        }

    //        return false;
    //    }

    //    private ExternalType? FindExternalType( 
    //        TypeInfo typeInfo, 
    //        List<NamespaceContext> nsContexts,
    //        bool createIfMissing )
    //    {
    //        var sameNameTypes = _dbContext.ExternalTypes
    //            .Include( x => x.PossibleNamespaces )
    //            .Include( x => x.TypeArguments )
    //            .Where( x => x.Name == typeInfo.Name
    //                         && ( x.TypeArguments == null && typeInfo.Arguments.Count == 0
    //                              || x.TypeArguments != null && x.TypeArguments.Count == typeInfo.Arguments.Count )
    //            );

    //        if( !sameNameTypes.Any() )
    //            return createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;

    //        foreach( var extTypeDb in sameNameTypes )
    //        {
    //            if( extTypeDb.PossibleNamespaces?
    //                .Any( x => nsContexts.Any( y => y.NamespaceName == x.Name ) ) ?? false )
    //            {
    //                // there's a match on the existing possible namespaces, so return the external type
    //                return extTypeDb;
    //            }

    //            // there's a match on the type name but not the possible namespaces so add the current
    //            // namespace to the list of possibles and return the type
    //            var possibleNS = extTypeDb.PossibleNamespaces?.ToList() ?? new List<Namespace>();
    //            possibleNS.AddRange( nsContexts.Select( x => x.Namespace ) );

    //            extTypeDb.PossibleNamespaces = possibleNS.Distinct( Namespace.FullyQualifiedNameComparer ).ToList();

    //            return extTypeDb;
    //        }

    //        // if we get here there were no matches among the ExternalTypes in the database
    //        return createIfMissing ? CreateExternalType( typeInfo, nsContexts ) : null;
    //    }

    //    private ExternalType CreateExternalType( TypeInfo typeInfo, List<NamespaceContext> nsContexts )
    //    {
    //        var retVal = new ExternalType
    //        {
    //            Name = typeInfo.Name,
    //            PossibleNamespaces = nsContexts.Select( x => x.Namespace ).ToList()
    //        };

    //        _dbContext.ExternalTypes.Add( retVal );

    //        return retVal;
    //    }

    //    private bool GetTypeInfo( SyntaxNode node, TypeInfo typeInfo )
    //    {
    //        if( !node.GetChildNode( out var typeNode, 
    //            SyntaxKind.GenericName, SyntaxKind.IdentifierName, SyntaxKind.PredefinedType ) )
    //        {
    //            _logger?.Error( "Type container node contains neither a GenericName, an IdentifierName node nor a PredefinedType node" );
    //            return false;
    //        }

    //        var childTypeInfo = typeInfo.AddChild( typeNode!.ChildTokens().First().Text );

    //        if( !typeNode!.GetChildNode( SyntaxKind.TypeParameterList, out var tplNode ) )
    //            return true;

    //        return tplNode!.ChildNodes().All( childNode => GetTypeInfo( childNode, childTypeInfo ) );
    //    }
    //}
}
