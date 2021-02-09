#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SymbolAuditor
    {
        private readonly List<Type> _symbolTypes = new();
        private readonly MethodInfo _genericMethod;
        private readonly IJ4JLogger? _logger;

        public SymbolAuditor( IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            var temp = typeof(NodeCollectorExtensions).GetMethod( "GetSymbol" );
            if( temp == null )
                throw new NullReferenceException(
                    $"Couldn't find {nameof(MethodInfo)} for {nameof(NodeCollectorExtensions.GetSymbol)}" );

            _genericMethod = temp;
        }

        public SymbolAuditor Register<TSymbol>()
            where TSymbol : class, ISymbol
        {
            var symbolType = typeof(TSymbol);

            if( _symbolTypes.All( x => x != symbolType ) )
                _symbolTypes.Add( symbolType );
            else _logger?.Debug( "Attempted to register duplicate symbol type {0}", symbolType );

            return this;
        }

        public IFoundSymbol FindSymbol( SyntaxNode node, CompiledFile compiledFile )
        {
            return node.Kind() switch
            {
                SyntaxKind.AttributeList => GetSymbol( node.Parent, compiledFile ),
            }
            var retVal = new List<IFoundSymbol>();

            foreach( var symbolType in _symbolTypes )
            {
                var getSymbol = _genericMethod.MakeGenericMethod( symbolType );

                var args = new object?[] { node, compiledFile, null };
                getSymbol.Invoke( null, args );

                if( args[ 2 ] == null ) 
                    continue;

                retVal.Add( new FoundSymbol( (ISymbol) args[ 2 ]!, symbolType ) );
            }

            if( !node.IsKind( SyntaxKind.AttributeList ) || node.Parent == null ) 
                return retVal;

            var attributedSymbol = GetSymbol( node.Parent, compiledFile );

            if( attributedSymbol != null )
                retVal.Add( new FoundAttributeList( attributedSymbol ) );

            return retVal;
        }

        private ISymbol? GetSymbol( SyntaxNode node, CompiledFile compFile )
        {
            var symbolInfo = compFile.Model.GetSymbolInfo( node );

            return symbolInfo.Symbol ?? compFile.Model.GetDeclaredSymbol( node );
        }
    }
}

#endif
