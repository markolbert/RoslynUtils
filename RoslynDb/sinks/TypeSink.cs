using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Serilog;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol, FixedTypeDb>
    {
        private readonly ISymbolSetProcessor<ITypeSymbol> _processors;

        public TypeSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISymbolSetProcessor<ITypeSymbol> processors,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, logger )
        {
            _processors = processors;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<FixedTypeDb>();
            MarkUnsynchronized<GenericTypeDb>();
            MarkUnsynchronized<ParametricTypeDb>();
            MarkUnsynchronized<TypeAncestorDb>();
            MarkUnsynchronized<TypeArgumentDb>();
            MarkUnsynchronized<MethodPlaceholderDb>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            //var allOkay = true;

            //// add the types we initially collected
            //foreach( var typeSymbol in Symbols )
            //{
            //    allOkay &= ProcessSymbol( typeSymbol );
            //}

            //SaveChanges();

            return _processors.Process( Symbols );

            //return allOkay;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            // we don't call the base implementation because it tries to add the symbol
            // which is fine but we need to drill into it's parentage and we only want to
            // do that if we haven't visited it before
            StoreTypeTree( symbol );

            return true;
        }

        private void StoreTypeTree( ITypeSymbol symbol )
        {
            // if we've visited this symbol go no further
            if( /*symbol is ITypeParameterSymbol ||*/ !Symbols.Add( symbol ) )
                return;

            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                if( !Symbols.Add( interfaceSymbol ) )
                    continue;

                // add ancestors related to the interface symbol
                StoreTypeTree( interfaceSymbol );

                // add ancestors related to closed generic types, if any, in
                // the interface
                foreach( var closingSymbol in interfaceSymbol.TypeArguments
                    .Where( x => !( x is ITypeParameterSymbol ) ) )
                {
                    StoreTypeTree( closingSymbol );
                }
            }

            // array symbols have two ancestry paths, one pointing to Array
            // and the other pointing to whatever type of element they contain
            if( symbol is IArrayTypeSymbol arraySymbol )
                StoreTypeTree(arraySymbol.ElementType);

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                Symbols.Add( baseSymbol );

                baseSymbol = baseSymbol.BaseType;
            }
        }

        //private bool ProcessSymbol( ITypeSymbol symbol )
        //{
        //    var symbolInfo = SymbolInfo.Create(symbol);

        //    switch ( symbolInfo.TypeKind )
        //    {
        //        case TypeKind.Error:
        //            Logger.Error<string>( "Unhandled or incorrect type error for named type '{0}'",
        //                symbolInfo.SymbolName );

        //            return false;

        //        case TypeKind.Dynamic:
        //        case TypeKind.Pointer:
        //            Logger.Error<string, TypeKind>(
        //                "named type '{0}' is a {1} and not supported",
        //                symbolInfo.SymbolName,
        //                symbolInfo.TypeKind );

        //            return false;
        //    }

        //    if( !GetByFullyQualifiedName<Assembly>( symbolInfo.ContainingAssembly, out var dbAssembly ) )
        //        return false;

        //    if( !GetByFullyQualifiedName<Namespace>( symbolInfo.ContainingNamespace, out var dbNS ) )
        //        return false;

        //    if( !GetByFullyQualifiedName<TypeDefinition>(symbolInfo.Symbol, out var dbSymbol))
        //        dbSymbol = AddEntity( symbolInfo.SymbolName );

        //    dbSymbol!.Synchronized = true;
        //    dbSymbol.Name = SymbolInfo.GetName( symbolInfo.Symbol );
        //    dbSymbol.AssemblyID = dbAssembly!.ID;
        //    dbSymbol.NamespaceId = dbNS!.ID;
        //    dbSymbol.Accessibility = symbolInfo.Symbol.DeclaredAccessibility;
        //    dbSymbol.DeclarationModifier = symbolInfo.Symbol.GetDeclarationModifier();
        //    dbSymbol.Nature = symbolInfo.TypeKind;
        //    dbSymbol.InDocumentationScope = dbAssembly.InScopeInfo != null;

        //    return true;
        //}
    }
}
