using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn.entities.types;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeDefinitionSink : RoslynDbSink<INamedTypeSymbol, TypeDefinition>
    {
        private readonly ITypeDefinitionProcessors _processors;
        private readonly Dictionary<string, INamedTypeSymbol> _typeSymbols = new Dictionary<string, INamedTypeSymbol>();

        public TypeDefinitionSink(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            ITypeDefinitionProcessors processors,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
            _processors = processors;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            // clear the collection of processed type symbols
            _typeSymbols.Clear();

            MarkUnsynchronized<TypeDefinition>();
            MarkUnsynchronized<TypeParameter>();
            MarkUnsynchronized<TypeAncestor>();
            MarkUnsynchronized<TypeClosure>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            var typeList = _typeSymbols.Select( ts => ts.Value )
                .ToList();

            if( !_processors.Process( new TypeProcessorContext( syntaxWalker, typeList ) ) )
                return false;

            //// add information related to any type definitions we found while
            //// processing the type definitions we found via the syntax walker. This would
            //// include base/parent types, their ancestors, etc.
            //foreach( var typeSymbol in typeList )
            //{
            //    allOkay &= ProcessSymbol( syntaxWalker, new SymbolInfo( typeSymbol, SymbolName ) );
            //}

            //// update information related to generic types
            //foreach( var generic in typeList.Where( td => td.IsGenericType ) )
            //{
            //    allOkay &= ProcessGeneric( generic );
            //}

            //// add the implementations (i.e., parent types, which includes a single
            //// possible class/struct and any number of interfaces)
            //foreach ( var typeSymbol in typeList )
            //{
            //    allOkay &= ProcessAncestors( typeSymbol );
            //}

            SaveChanges();

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, INamedTypeSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            // output the symbol to the database
            if( !ProcessSymbol( syntaxWalker, retVal ) )
                return retVal;

            // store the processed symbol and all of its ancestor types if we haven't
            // already processed it
            if( !_typeSymbols.ContainsKey( retVal.SymbolName ) )
            {
                _typeSymbols.Add( retVal.SymbolName, (INamedTypeSymbol) retVal.Symbol );

                AddAncestorTypes( symbol );
            }

            retVal.WasOutput = true;

            return retVal;
        }

        private void AddAncestorTypes( INamedTypeSymbol symbol )
        {
            StoreNamedTypeSymbol( symbol, out _ );

            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                if( !StoreNamedTypeSymbol( interfaceSymbol, out _ ) )
                    continue;

                // add ancestors related to the interface symbol
                AddAncestorTypes( interfaceSymbol );

                // add ancestors related to closed generic types, if any, in
                // the interface
                foreach( var closingSymbol in interfaceSymbol.TypeArguments
                    .Where( ta => ta is INamedTypeSymbol )
                    .Cast<INamedTypeSymbol>() )
                {
                    AddAncestorTypes( closingSymbol );
                }
            }

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                StoreNamedTypeSymbol( baseSymbol, out var symbolInfo );

                baseSymbol = ( (ITypeSymbol) symbolInfo.Symbol ).BaseType;
            }
        }

        private bool StoreNamedTypeSymbol( INamedTypeSymbol symbol, out SymbolInfo result )
        {
            result = SymbolInfo.Create( symbol );

            if( _typeSymbols.ContainsKey( result.SymbolName ) )
                return false;

            _typeSymbols.Add( result.SymbolName, (INamedTypeSymbol) result.Symbol );

            return true;
        }

        private bool ProcessSymbol( ISyntaxWalker syntaxWalker, SymbolInfo symbolInfo )
        {
            switch( symbolInfo.TypeKind )
            {
                case TypeKind.Error:
                    Logger.Error<string>( "Unhandled or incorrect type error for named type '{0}'",
                        symbolInfo.SymbolName );

                    return false;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        symbolInfo.SymbolName,
                        symbolInfo.TypeKind );

                    return false;
            }

            if( !GetByFullyQualifiedName<Assembly>( symbolInfo.Symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !GetByFullyQualifiedName<Namespace>( symbolInfo.Symbol.ContainingNamespace, out var dbNS ) )
                return false;

            if( !GetByFullyQualifiedName<TypeDefinition>(symbolInfo.Symbol, out var dbSymbol))
                dbSymbol = AddEntity( symbolInfo.SymbolName );

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = SymbolInfo.GetName( symbolInfo.OriginalSymbol );
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbolInfo.Symbol.ContainingAssembly );

            SaveChanges();

            return true;
        }

        //private bool ProcessGeneric( INamedTypeSymbol generic )
        //{
        //    if( !TryGetSunkValue( generic, out var typeDefDb ) )
        //        return false;

        //    var allOkay = true;

        //    // first create/update all the TypeParameters related to this generic type
        //    foreach ( var tpSymbol in generic.TypeParameters )
        //    {
        //        var tpDb = ProcessTypeParameter( typeDefDb!, tpSymbol );

        //        foreach( var conSymbol in tpSymbol.ConstraintTypes )
        //        {
        //            allOkay &= ProcessTypeConstraints( typeDefDb!, tpSymbol, conSymbol );
        //        }
        //    }

        //    return allOkay;
        //}

        //private TypeParameter ProcessTypeParameter( TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol )
        //{
        //    var tpSet = GetDbSet<TypeParameter>();

        //    var tpDb = tpSet
        //        .FirstOrDefault( x => x.Ordinal == tpSymbol.Ordinal && x.ContainingTypeID == typeDefDb.ID );

        //    if( tpDb == null )
        //    {
        //        tpDb = new TypeParameter
        //        {
        //            ContainingTypeID = typeDefDb.ID,
        //            Ordinal = tpSymbol.Ordinal
        //        };

        //        tpSet.Add( tpDb );
        //    }

        //    tpDb.Synchronized = true;
        //    tpDb.Name = tpSymbol.Name;
        //    tpDb.Constraints = tpSymbol.GetTypeParameterConstraint();

        //    return tpDb;
        //}

        //private bool ProcessTypeConstraints( 
        //    TypeDefinition typeDefDb, 
        //    ITypeParameterSymbol tpSymbol, 
        //    ITypeSymbol conSymbol )
        //{
        //    var typeSet = GetDbSet<TypeDefinition>();
        //    var closureSet = GetDbSet<TypeClosure>();

        //    var symbolInfo = new SymbolInfo( conSymbol, SymbolName );

        //    if( !( symbolInfo.Symbol is INamedTypeSymbol ) && symbolInfo.TypeKind != TypeKind.Array )
        //    {
        //        Logger.Error<string>(
        //            "Constraining type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
        //            symbolInfo.SymbolName );
        //        return false;
        //    }

        //    var conDb = typeSet.FirstOrDefault( td => td.FullyQualifiedName == symbolInfo.SymbolName );

        //    if( conDb == null )
        //    {
        //        Logger.Error<string>( "Constraining type '{0}' not found in database", symbolInfo.SymbolName );
        //        return false;
        //    }

        //    var closureDb = closureSet.FirstOrDefault( c =>
        //        c.TypeBeingClosedID == typeDefDb.ID && c.ClosingTypeID == conDb.ID );

        //    if( closureDb == null )
        //    {
        //        closureDb = new TypeClosure
        //        {
        //            ClosingType = conDb,
        //            TypeBeingClosed = typeDefDb!,
        //            Ordinal = tpSymbol.Ordinal
        //        };

        //        closureSet.Add( closureDb );
        //    }

        //    closureDb.Synchronized = true;

        //    return true;
        //}

        //private bool ProcessAncestors( INamedTypeSymbol typeSymbol )
        //{
        //    if( !TryGetSunkValue( typeSymbol, out var typeDb ) )
        //        return false;

        //    // process base type if it's defined
        //    if( typeSymbol.BaseType != null && !ProcessAncestor( typeDb!, typeSymbol.BaseType ) )
        //        return false;

        //    var allOkay = true;

        //    foreach( var interfaceSymbol in typeSymbol.Interfaces )
        //    {
        //        allOkay &= ProcessAncestor( typeDb!, interfaceSymbol );
        //    }

        //    return allOkay;
        //}

        //private bool ProcessAncestor( TypeDefinition typeDb, INamedTypeSymbol ancestorSymbol )
        //{
        //    if( !TryGetSunkValue( ancestorSymbol, out var implTypeDb ) )
        //        return false;

        //    var ancestorSet = GetDbSet<TypeAncestor>();

        //    var ancestorDb = ancestorSet
        //        .FirstOrDefault( ti => ti.ChildTypeID == typeDb!.ID && ti.ImplementingTypeID == implTypeDb!.ID );

        //    if( ancestorDb == null )
        //    {
        //        ancestorDb = new TypeAncestor
        //        {
        //            ImplementingTypeID = implTypeDb!.ID,
        //            ChildTypeID = typeDb!.ID
        //        };

        //        ancestorSet.Add( ancestorDb );
        //    }

        //    ancestorDb.Synchronized = true;

        //    return ProcessAncestorClosures( typeDb, ancestorSymbol );
        //}

        //private bool ProcessAncestorClosures( TypeDefinition typeDb, INamedTypeSymbol ancestorSymbol )
        //{
        //    var allOkay = true;
        //    var typeSet = GetDbSet<TypeDefinition>();
        //    var closureSet = GetDbSet<TypeClosure>();

        //    for ( int idx = 0; idx < ancestorSymbol.TypeArguments.Length; idx++ )
        //    {
        //        if( ancestorSymbol.TypeArguments[ idx ] is ITypeParameterSymbol )
        //            continue;

        //        var symbolInfo = new SymbolInfo(ancestorSymbol.TypeArguments[idx], SymbolName);

        //        if (!(symbolInfo.Symbol is INamedTypeSymbol) && symbolInfo.TypeKind != TypeKind.Array)
        //        {
        //            Logger.Error<string>(
        //                "Closing type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
        //                symbolInfo.SymbolName);
        //            allOkay = false;

        //            continue;
        //        }

        //        var conDb = typeSet.FirstOrDefault(td => td.FullyQualifiedName == symbolInfo.SymbolName);

        //        if (conDb == null)
        //        {
        //            Logger.Error<string>("Closing type '{0}' not found in database", symbolInfo.SymbolName);
        //            allOkay = false;

        //            continue;
        //        }

        //        var closureDb = closureSet
        //            .FirstOrDefault( c => c.TypeBeingClosedID == typeDb.ID && c.ClosingTypeID == conDb.ID );

        //        if (closureDb == null)
        //        {
        //            closureDb = new TypeClosure
        //            {
        //                ClosingType = conDb,
        //                TypeBeingClosed = typeDb!,
        //                Ordinal = idx
        //            };

        //            closureSet.Add(closureDb);
        //        }

        //        closureDb.Synchronized = true;
        //    }

        //    return allOkay;
        //}

    }
}
