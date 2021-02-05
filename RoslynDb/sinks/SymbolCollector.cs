using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SymbolCollector : ISyntaxNodeSink
    {
        private readonly List<string> _visitedSymbols = new List<string>();

        private class SymbolComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals(ISymbol? x, ISymbol? y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                return string.Equals(x.ToUniqueName(),
                    y.ToUniqueName(),
                    StringComparison.Ordinal);
            }

            public int GetHashCode(ISymbol obj)
            {
                return obj.GetHashCode();
            }
        }

        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();

        private readonly Nodes<ISymbol> _symbols =
            new Nodes<ISymbol>( new SymbolComparer() );

        private readonly IRoslynDataLayer _dataLayer;
        private readonly WalkerContext _context;
        private readonly IJ4JLogger _logger;

        private SemanticModel _curModel;

        public SymbolCollector(
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            IJ4JLogger logger
        )
        {
            _dataLayer = dataLayer;
            _context = context;

            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool AlreadyProcessed( SyntaxNode node ) =>
            _visitedNodes.Any(vn => vn.Equals(node));

        public bool ProcessesNode(SyntaxNode node) =>
            node.Kind() switch
            {
                SyntaxKind.ArrayType => true,
                SyntaxKind.ClassDeclaration => true,
                SyntaxKind.ConstructorDeclaration => true,
                SyntaxKind.DelegateDeclaration => true,
                SyntaxKind.EventDeclaration => true,
                SyntaxKind.IndexerDeclaration => true,
                SyntaxKind.InterfaceDeclaration => true,
                SyntaxKind.MethodDeclaration => true,
                SyntaxKind.NamespaceDeclaration => true,
                SyntaxKind.PredefinedType => true,
                SyntaxKind.PropertyDeclaration => true,
                SyntaxKind.StructDeclaration => true,
                SyntaxKind.TupleType => true,
                SyntaxKind.TypeParameter => true,
                SyntaxKind.VariableDeclarator => true,
                _ => false
            };

        public bool DrillIntoNode( SyntaxNode node )
        {
            return node.Kind() switch
            {
                SyntaxKind.AccessorList => false,
                SyntaxKind.BaseList => false,
                SyntaxKind.IdentifierName => false,
                SyntaxKind.IdentifierToken => false,
                SyntaxKind.NameEquals => false,
                SyntaxKind.QualifiedName => false,
                SyntaxKind.SimpleBaseType => false,
                SyntaxKind.StringLiteralExpression => false,
                SyntaxKind.UsingDirective => false,
                //SyntaxKind.MethodDeclaration => false,
                //SyntaxKind.PropertyDeclaration => false,
                //SyntaxKind.EventDeclaration => false,
                //SyntaxKind.FieldDeclaration => false,
                _ => true
            };

        }

        public bool InitializeSink( SemanticModel model )
        {
            _visitedNodes.Clear();
            _visitedSymbols.Clear();

            _curModel = model;

            return true;
        }

        public void OutputSyntaxNode( Stack<SyntaxNode> nodeStack )
        {
            var node = nodeStack.Peek();

            // don't re-process nodes
            if( AlreadyProcessed(node ) ) 
            {
                _logger.Verbose<SyntaxKind>("Already processed SyntaxNode {0}", node.Kind());
                return;
            }

            _visitedNodes.Add( node );

            // certain kinds of nodes we don't attempt to process
            if( !ProcessesNode( node ) )
            {
                _logger.Verbose<SyntaxKind>("Node type {0} doesn't need to be processed directly", node.Kind());
                return;
            }

            var symbol = GetSymbol( node );

            if( symbol == null )
            {
                _logger.Error<SyntaxKind>( "Couldn't find ISymbol for node {0}", node.Kind() );
                return;
            }

            if( IsDuplicateSymbol( symbol ) )
            {
                _logger.Verbose<string>( "Already processed ISymbol '{0}'", symbol.ToUniqueName() );
                return;
            }

            // always collect the IAssemblySymbol and INamespaceSymbol...which can be
            // null for arrays, in which case we want the IAssemblySymbol and INamespaceSymbol
            // for the array's ElementType
            if( symbol is IArrayTypeSymbol arraySymbol )
            {
                ProcessAssembly( arraySymbol.ElementType.ContainingAssembly );
                ProcessNamespace( arraySymbol.ElementType.ContainingNamespace );
            }
            else
            {
                ProcessAssembly( symbol.ContainingAssembly );
                ProcessNamespace( symbol.ContainingNamespace );
            }

            switch( symbol )
            {
                case IAssemblySymbol assemblySymbol:
                case INamespaceSymbol nsSymbol:
                    // no op because we handled this in the preceding code (also,
                    // we should never see IAssemblySymbols from the walker anyway)
                    break;

                case IFieldSymbol fieldSymbol:
                case IEventSymbol eventSymbol:
                    _symbols.AddDependentNode(symbol.ContainingType, symbol);

                    ProcessType( symbol.ContainingType/*, symbol.ContainingNamespace*/ );
                    ProcessAttributes(symbol);
                    
                    break;

                case ITypeSymbol typeSymbol:
                    ProcessType( typeSymbol/*, typeSymbol.ContainingNamespace*/ );
                    ProcessAttributes(symbol);
                    
                    break;

                case IMethodSymbol methodSymbol:
                    ProcessMethod( methodSymbol );
                    break;

                case IPropertySymbol propSymbol:
                    ProcessProperty( propSymbol );
                    break;

                default:
                    DefaultNodeHandler( node, symbol );
                    
                    break;
            };
        }

        public bool FinalizeSink(ISyntaxWalker syntaxWalker)
        {
            if (!_symbols.Sort(out var sortedSymbols, out var remainingEdges))
            {
                _logger.Error<int>( "Failed to sort ISymbols topologically, {0} edges remain", remainingEdges!.Count() );
                return false;
            }

            // not sure why the topological sorts come out backwards but they do...
            sortedSymbols!.Reverse();

            var allOkay = true;

            foreach (var symbol in sortedSymbols!)
            {
                var updateSucceeded = symbol switch
                {
                    IAssemblySymbol assemblySymbol => UpdateAssemblyInDatabase(assemblySymbol),
                    INamespaceSymbol nsSymbol => UpdateNamespaceInDatabase(nsSymbol),
                    INamedTypeSymbol ntSymbol => UpdateNamedTypeInDatabase(ntSymbol),
                    ITypeParameterSymbol tpSymbol => UpdateParametricTypeInDatabase(tpSymbol),
                    IArrayTypeSymbol arraySymbol => UpdateArrayTypeInDatabase(arraySymbol),
                    IMethodSymbol methodSymbol => UpdateMethodInDatabase(methodSymbol),
                    IPropertySymbol propSymbol => UpdatePropertyInDatabase(propSymbol),
                    _ => false
                };

                if (updateSucceeded)
                    _dataLayer.SaveChanges();

                allOkay &= updateSucceeded;

                if (!allOkay && _context.StopOnFirstError)
                    break;
            }

            return allOkay;
        }

        private ISymbol? GetSymbol( SyntaxNode node )
        {
            var symbolInfo = _curModel.GetSymbolInfo( node );

            return symbolInfo.Symbol ?? _curModel.GetDeclaredSymbol( node );
        }

        private void DefaultNodeHandler( SyntaxNode node, ISymbol? symbol ) =>
            _logger.Error<SyntaxKind>(
                symbol != null ? "Unsupported SyntaxNode {0}" : "Couldn't get ISymbol for node type {0}", node.Kind() );

        private bool IsDuplicateSymbol(ISymbol symbol)
        {
            // don't allow duplicate additions so we can avoid infinite loops
            var fullName = symbol.ToUniqueName();

            if (_visitedSymbols.Any(x => x.Equals(fullName)))
                return true;

            _visitedSymbols.Add(fullName);

            return false;
        }

        #region symbol extraction methods

        private void ProcessAssembly( IAssemblySymbol symbol )
        {
            _symbols.AddIndependentNode( symbol );
            ProcessAttributes( symbol );
        }

        private void ProcessNamespace( INamespaceSymbol symbol )
        {
            _symbols.AddDependentNode( symbol.ContainingAssembly, symbol );
        }

        #region type symbol extraction methods

        private NodeSinkResult ProcessType( ITypeSymbol symbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            return symbol switch
            {
                INamedTypeSymbol ntSymbol => ProcessNamedType( ntSymbol/*, parentSymbol*/ ),
                ITypeParameterSymbol tpSymbol => ProcessTypeParameter( tpSymbol/*, parentSymbol*/ ),
                IArrayTypeSymbol arraySymbol => ProcessArrayType( arraySymbol/*, parentSymbol*/ ),
                _ => unhandled()
            };

            NodeSinkResult unhandled()
            {
                _logger.Error<string>(
                    "ITypeSymbol '{0}' is neither an INamedTypeSymbol, an ITypeParameterSymbol nor an IArrayTypeSymbol",
                    symbol.ToUniqueName() );

                return NodeSinkResult.InvalidNode;
            }
        }

        private NodeSinkResult ProcessNamedType( INamedTypeSymbol ntSymbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            if( ntSymbol.TypeKind == TypeKind.Interface )
                return ProcessInterface( ntSymbol/*, parentSymbol*/ );

            return ProcessImplementableType( ntSymbol/*, parentSymbol*/ );
        }

        private NodeSinkResult ProcessInterface( INamedTypeSymbol interfaceSymbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            if (IsDuplicateSymbol(interfaceSymbol))
                return NodeSinkResult.AlreadyProcessed;

            if ( interfaceSymbol.TypeKind != TypeKind.Interface )
            {
                _logger.Error<string>( "Non-interface '{0}' submitted to ProcessInterface()",
                    interfaceSymbol.ToFullName() );
                return NodeSinkResult.InvalidNode;
            }

            if( interfaceSymbol.BaseType != null )
            {
                _logger.Error<string>( "Interface '{0}' has a base type", interfaceSymbol.ToFullName() );
                return NodeSinkResult.InvalidNode;
            }

            // if the containingSymbol is null we're dealing with an interface >>definition<<,
            // which is considered "derived from" its namespace symbol
            //if( parentSymbol == null )
            //    _symbols.Add( interfaceSymbol.ContainingNamespace, interfaceSymbol );
            //else 
            if( interfaceSymbol.ContainingType == null )
                _symbols.AddDependentNode( interfaceSymbol.ContainingNamespace, interfaceSymbol );
            else
                _symbols.AddDependentNode(interfaceSymbol.ContainingType, interfaceSymbol);

            // add any type parameters and type arguments
            foreach ( var tpSymbol in interfaceSymbol.TypeParameters )
            {
                var addResult = ProcessType( tpSymbol/*, interfaceSymbol*/ );

                if( addResult != NodeSinkResult.Okay )
                    return addResult;
            }

            foreach( var taSymbol in interfaceSymbol.TypeArguments.Where( x => !( x is ITypeParameterSymbol ) ) )
            {
                var addResult = ProcessType( taSymbol/*, interfaceSymbol*/ );

                if( addResult != NodeSinkResult.Okay )
                    return addResult;
            }

            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessImplementableType( INamedTypeSymbol symbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            if (IsDuplicateSymbol(symbol))
                return NodeSinkResult.AlreadyProcessed;

            // if the symbol isn't contained by a type it must be contained by a
            // namespace
            //if ( parentSymbol == null )
            //    _symbols.Add( symbol.ContainingNamespace, symbol );
            //else 
            if( symbol.ContainingType == null )
                _symbols.AddDependentNode( symbol.ContainingNamespace, symbol );
            else
                _symbols.AddDependentNode( symbol.ContainingType, symbol );

            if ( symbol.BaseType != null )
                ProcessImplementableType( symbol.BaseType/*, symbol*/ );

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                var addResult = ProcessInterface( interfaceSymbol/*, symbol*/ );

                if( addResult != NodeSinkResult.Okay )
                    return addResult;
            }

            // add any type parameters and type arguments
            foreach( var tpSymbol in symbol.TypeParameters )
            {
                var addResult = ProcessType( tpSymbol/*, symbol*/ );
                if( addResult != NodeSinkResult.Okay )
                    return addResult;

                // add any interfaces
                foreach( var interfaceSymbol in tpSymbol.AllInterfaces )
                {
                    addResult = ProcessInterface( interfaceSymbol/*, tpSymbol*/ );

                    if( addResult != NodeSinkResult.Okay )
                        return addResult;
                }
            }

            foreach( var taSymbol in symbol.TypeArguments.Where( x => !( x is ITypeParameterSymbol ) ) )
            {
                var addResult = ProcessType( taSymbol/*, symbol*/ );
                if( addResult != NodeSinkResult.Okay )
                    return addResult;

                // add any interfaces
                foreach( var interfaceSymbol in taSymbol.AllInterfaces )
                {
                    addResult = ProcessInterface( interfaceSymbol/*, taSymbol*/ );
                    if( addResult != NodeSinkResult.Okay )
                        return addResult;
                }
            }

            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessTypeParameter( ITypeParameterSymbol symbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            if( IsDuplicateSymbol( symbol ) )
                return NodeSinkResult.AlreadyProcessed;

            // if the symbol isn't contained by a type it must be contained by a
            // namespace
            //if ( parentSymbol == null )
            //    _symbols.Add( symbol.ContainingNamespace, symbol );
            //else 
            if( symbol.ContainingType == null )
                _symbols.AddDependentNode( symbol.ContainingNamespace, symbol );
            else
                _symbols.AddDependentNode(symbol.ContainingType, symbol);

            if ( symbol.BaseType != null )
                ProcessImplementableType( symbol.BaseType/*, symbol*/ );

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                var addResult = ProcessInterface( interfaceSymbol/*, symbol*/ );
                if( addResult != NodeSinkResult.Okay )
                    return addResult;
            }

            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessArrayType( IArrayTypeSymbol symbol/*, INamespaceOrTypeSymbol parentSymbol*/ )
        {
            if (IsDuplicateSymbol(symbol))
                return NodeSinkResult.AlreadyProcessed;

            //// if the symbol isn't contained by a type it must be contained by a
            //// namespace
            //if ( parentSymbol == null )
            //    _symbols.Add( symbol.ContainingNamespace, symbol );
            //else 
            if( symbol.ContainingType == null )
                _symbols.AddDependentNode( symbol.ContainingNamespace, symbol );
            else
                _symbols.AddDependentNode(symbol.ContainingType, symbol);

            if ( symbol.BaseType != null )
                ProcessImplementableType( symbol.BaseType/*, symbol*/ );

            var addResult = ProcessType( symbol.ElementType/*, symbol*/ );
            if( addResult != NodeSinkResult.Okay )
                return addResult;

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                addResult = ProcessInterface( interfaceSymbol/*, symbol*/ );
                if( addResult != NodeSinkResult.Okay )
                    return addResult;
            }

            return NodeSinkResult.Okay;
        }

        #endregion

        private void ProcessMethod( IMethodSymbol symbol )
        {
            _symbols.AddDependentNode( symbol.ContainingType, symbol );

            ProcessType( symbol.ContainingType );

            foreach( var tpSymbol in symbol.TypeArguments )
            {
                ProcessType( tpSymbol );
            }

            foreach( var paramSymbol in symbol.Parameters )
            {
                ProcessType( paramSymbol.Type );
            }

            ProcessType( symbol.ReturnType );

            ProcessAttributes( symbol );
        }

        private void ProcessProperty(IPropertySymbol symbol)
        {
            _symbols.AddDependentNode(symbol.ContainingType, symbol);

            ProcessType(symbol.ContainingType);

            foreach (var paramSymbol in symbol.Parameters)
            {
                ProcessType(paramSymbol.Type);
            }

            ProcessType(symbol.Type);

            ProcessAttributes(symbol);
        }

        private void ProcessAttributes( ISymbol symbol )
        {
            foreach( var attrData in symbol.GetAttributes() )
            {
                if( attrData.AttributeClass != null )
                {
                    _symbols.AddDependentNode( attrData.AttributeClass, symbol );

                    ProcessNamespace(attrData.AttributeClass.ContainingNamespace);
                    ProcessType( attrData.AttributeClass );
                }

                if( attrData.AttributeConstructor != null )
                {
                    _symbols.AddDependentNode( attrData.AttributeConstructor.ContainingType, attrData.AttributeConstructor );

                    ProcessNamespace( attrData.AttributeConstructor.ContainingNamespace );
                    ProcessType( attrData.AttributeConstructor.ContainingType );
                }
            }
        }

        #endregion

        #region database update methods

        private bool UpdateAssemblyInDatabase( IAssemblySymbol symbol )
        {
            if( _dataLayer.GetAssembly( symbol, true, true ) == null )
                return false;

            // have to save newly-added entity so it can be found
            _dataLayer.SaveChanges();

            if ( _context.InDocumentationScope( symbol ) )
                return _dataLayer.GetInScopeAssemblyInfo( _context[ symbol ]!, true, true ) != null;

            return true;
        }

        private bool UpdateNamespaceInDatabase( INamespaceSymbol symbol )
        {
            var assemblyDb = _dataLayer.GetAssembly(symbol.ContainingAssembly);

            if (assemblyDb == null)
                return false;

            var nsDb = _dataLayer.GetNamespace(symbol, true, true);

            if (nsDb == null)
                return false;

            // have to save newly-added entity so it can be found
            _dataLayer.SaveChanges();

            return _dataLayer.GetAssemblyNamespace(assemblyDb, nsDb, true) != null;
        }

        private bool UpdateNamedTypeInDatabase( INamedTypeSymbol symbol )
        {
            var typeDb = _dataLayer.GetUnspecifiedType( symbol, true, true );

            if( typeDb == null )
                return false;

            // update type arguments if this is a generic type
            if( typeDb is GenericTypeDb genericDb )
            {
                if( !UpdateTypeArgumentsInDatabase( symbol, genericDb ) )
                    return false;
            }

            if (symbol.BaseType == null)
                return true;

            if (!UpdateTypeAncestorsInDatabase(typeDb, symbol.BaseType))
                return false;

            var allOkay = true;

            foreach (var interfaceSymbol in symbol.Interfaces)
            {
                allOkay &= UpdateTypeAncestorsInDatabase(typeDb!, interfaceSymbol);

                if (!allOkay && _context.StopOnFirstError)
                    break;
            }

            return allOkay;
        }

        private bool UpdateTypeArgumentsInDatabase( INamedTypeSymbol symbol, GenericTypeDb genericDb  )
        {
            var allOkay = true;

            for (var ordinal = 0; ordinal < symbol.TypeArguments.Length; ordinal++)
            {
                var typeArgSymbol = symbol.TypeArguments[ordinal];

                if( _dataLayer.GetTypeArgument( genericDb, typeArgSymbol, ordinal, true ) != null ) 
                    continue;

                _logger.Error<string>("Couldn't find type for type argument '{0}' in database ",
                    typeArgSymbol.ToFullName());

                allOkay = false;

                if (_context.StopOnFirstError)
                    break;
            }

            return allOkay;
        }

        private bool UpdateParametricTypeInDatabase( ITypeParameterSymbol symbol )
        {
            BaseTypeDb? typeDb = null;

            if( symbol.DeclaringType != null )
                typeDb = _dataLayer.GetParametricType( symbol, true, true );
            else
            {
                if( symbol.DeclaringMethod != null )
                    typeDb = _dataLayer.GetParametricMethodType( symbol, true, true );
            }

            if( typeDb == null )
            {
                _logger.Error<string>(
                    "ITypeParameterSymbol '{0}' is declared by neither an INamedTypeSymbol nor an IMethodSymbol",
                    symbol.ToUniqueName() );

                return false;
            }

            if( symbol.BaseType == null )
                return true;

            if( !UpdateTypeAncestorsInDatabase( typeDb, symbol.BaseType ) )
                return false;

            var allOkay = true;

            foreach (var interfaceSymbol in symbol.Interfaces)
            {
                allOkay &= UpdateTypeAncestorsInDatabase(typeDb!, interfaceSymbol);

                if( !allOkay && _context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }

        private bool UpdateArrayTypeInDatabase( IArrayTypeSymbol symbol )
        {
            var typeDb = _dataLayer.GetArrayType( symbol, true, true );

            if( typeDb == null )
                return false;

            var allOkay = true;

            foreach (var interfaceSymbol in symbol.Interfaces)
            {
                allOkay &= UpdateTypeAncestorsInDatabase(typeDb!, interfaceSymbol);

                if (!allOkay && _context.StopOnFirstError)
                    break;
            }

            return allOkay;
        }

        private bool UpdateTypeAncestorsInDatabase(BaseTypeDb typeDb, INamedTypeSymbol ancestorSymbol)
        {
            var ancestorDb = _dataLayer.GetImplementableType(ancestorSymbol);

            if (ancestorDb == null)
                return false;

            var typeAncestorDb = _dataLayer.GetTypeAncestor(typeDb, ancestorDb!, true);

            if (typeAncestorDb == null)
                return false;

            typeAncestorDb.Synchronized = true;

            return true;
        }

        private bool UpdateMethodInDatabase( IMethodSymbol symbol )
        {
            if( _dataLayer.GetMethod( symbol, true, true ) == null )
                return false;

            // have to save newly-added entity so it can be found
            _dataLayer.SaveChanges();

            var allOkay = true;

            foreach( var argSymbol in symbol.Parameters )
            {
                allOkay &= _dataLayer.GetArgument( argSymbol, true, true ) != null;

                if( !allOkay && _context.StopOnFirstError )
                    return false;
            }

            return allOkay;
        }

        private bool UpdatePropertyInDatabase( IPropertySymbol symbol )
        {
            if( _dataLayer.GetProperty( symbol, true, true ) == null )
                return false;

            // have to save newly-added entity so it can be found
            _dataLayer.SaveChanges();

            var allOkay = true;

            foreach ( var paramSymbol in symbol.Parameters )
            {
                allOkay &= _dataLayer.GetPropertyParameter( paramSymbol, true, true ) != null;

                if (!allOkay && _context.StopOnFirstError)
                    return false;
            }

            return allOkay;
        }

        #endregion
    }
}
