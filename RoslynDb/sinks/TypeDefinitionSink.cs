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
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeDefinitionSink : RoslynDbSink<INamedTypeSymbol, TypeDefinition>
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;
        private readonly ISymbolSink<INamespaceSymbol, Namespace> _nsSink;
        private readonly List<ITypeProcessor> _processors;
        private readonly Dictionary<string, INamedTypeSymbol> _typeSymbols = new Dictionary<string, INamedTypeSymbol>();

        public TypeDefinitionSink(
            RoslynDbContext dbContext,
            ISymbolSink<IAssemblySymbol, Assembly> assemblySink,
            ISymbolSink<INamespaceSymbol, Namespace> nsSink,
            ISymbolName symbolName,
            IEnumerable<ITypeProcessor> typeProcessors,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            _assemblySink = assemblySink;
            _nsSink = nsSink;

            var temp = typeProcessors.ToList();

            var error = TopologicalSorter.CreateSequence( temp, out var processors );

            if( error != null )
                Logger.Error( error );

            _processors = processors ?? new List<ITypeProcessor>();
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            // clear the collection of processed type symbols
            _typeSymbols.Clear();

            MarkUnsynchronized<TypeDefinition>();
            MarkUnsynchronized<TypeParameter>();
            MarkUnsynchronized<TypeImplementation>();
            MarkUnsynchronized<ClosedTypeParameter>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            // we want to add all the parent types for each symbol's inheritance tree.
            // but to do that we first have to add all the relevant assemblies and namespaces
            var allOkay = true;

            var typeList = _typeSymbols.Select( ts => ts.Value )
                .ToList();

            var context = new TypeProcessorContext( syntaxWalker, typeList );

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( context );
            }

            // add the parent types
            foreach( var typeSymbol in typeList )
            {
                allOkay &= OutputSymbol( syntaxWalker, typeSymbol );
            }

            // update information related to generic types
            foreach( var generic in typeList.Where( td => td.IsGenericType ) )
            {
                allOkay &= ProcessGeneric( generic );
            }

            // include type implementation details
            foreach( var typeSymbol in typeList )
            {
                allOkay &= ProcessImplementations( typeSymbol );
            }

            SaveChanges();

            return allOkay;
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
            result = new SymbolInfo( symbol, SymbolName );

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

            if( !_assemblySink.TryGetSunkValue( symbolInfo.Symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !_nsSink.TryGetSunkValue( symbolInfo.Symbol.ContainingNamespace, out var dbNS ) )
                return false;

            if( !GetByFullyQualifiedName( symbolInfo.SymbolName, out var dbSymbol ) )
                dbSymbol = AddEntity( symbolInfo.SymbolName );

            dbSymbol!.Synchronized = true;
            dbSymbol.Name = SymbolName.GetName( symbolInfo.OriginalSymbol );
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbolInfo.Symbol.ContainingAssembly );

            SaveChanges();

            return true;
        }

        private bool ProcessGeneric( INamedTypeSymbol generic )
        {
            if( !TryGetSunkValue( generic, out var typeDefDb ) )
                return false;

            var allOkay = true;

            foreach( var tp in generic.TypeParameters )
            {
                var tpDb = ProcessTypeParameter( typeDefDb!, tp );

                foreach( var typeConstraint in tp.ConstraintTypes.Cast<INamedTypeSymbol>() )
                {
                    allOkay &= ProcessTypeConstraint( tpDb, typeConstraint );
                }
            }

            return allOkay;
        }

        private TypeParameter ProcessTypeParameter( TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol )
        {
            var tpSet = GetDbSet<TypeParameter>();

            var retVal = tpSet
                .FirstOrDefault( x => x.ParameterIndex == tpSymbol.Ordinal && x.TypeDefinitionID == typeDefDb.ID );

            if( retVal == null )
            {
                retVal = new TypeParameter
                {
                    TypeDefinitionID = typeDefDb.ID,
                    ParameterIndex = tpSymbol.Ordinal
                };

                tpSet.Add( retVal );
            }

            retVal.Synchronized = true;
            retVal.ParameterName = tpSymbol.Name;
            retVal.Constraints = tpSymbol.GetGenericConstraints();

            return retVal;
        }

        private bool ProcessTypeConstraint( TypeParameter tpDb, INamedTypeSymbol constraintSymbol )
        {
            if( !TryGetSunkValue( constraintSymbol, out var constraintDb ) )
                return false;

            var tcSet = GetDbSet<TypeConstraint>();

            if( tcSet
                .Any( tc => tc.ConstrainingTypeID == constraintDb!.ID && tc.TypeParameterID == tpDb.ID ) )
                return true;

            tcSet.Add( new TypeConstraint
            {
                ConstrainingType = constraintDb!,
                TypeParameter = tpDb
            } );

            return true;
        }

        private bool ProcessImplementations( INamedTypeSymbol typeSymbol )
        {
            if( !TryGetSunkValue( typeSymbol, out var typeDb ) )
                return false;

            // process base type if it's defined
            if( typeSymbol.BaseType != null && !ProcessImplementation( typeDb!, typeSymbol.BaseType ) )
                return false;

            var allOkay = true;

            foreach( var interfaceSymbol in typeSymbol.Interfaces )
            {
                allOkay &= ProcessImplementation( typeDb!, interfaceSymbol );
            }

            return allOkay;
        }

        private bool ProcessImplementation( TypeDefinition typeDb, INamedTypeSymbol implSymbol )
        {
            if( !TryGetSunkValue( implSymbol, out var implTypeDb ) )
                return false;

            var implSet = GetDbSet<TypeImplementation>();

            var implDb = implSet
                .FirstOrDefault( ti => ti.TypeDefinitionID == typeDb!.ID && ti.ImplementedTypeID == implTypeDb!.ID );

            if( implDb == null )
            {
                implDb = new TypeImplementation
                {
                    ImplementedTypeID = implTypeDb!.ID,
                    TypeDefinitionID = typeDb!.ID
                };

                implSet.Add( implDb );
            }

            implDb.Synchronized = true;

            return ProcessTypeParameterClosures( implDb, implSymbol );
        }

        private bool ProcessTypeParameterClosures( TypeImplementation implDb, INamedTypeSymbol implSymbol )
        {
            var allOkay = true;

            for( var idx = 0; idx < implSymbol.TypeArguments.Length; idx++ )
            {
                if( !( implSymbol.TypeArguments[ idx ] is INamedTypeSymbol ntSymbol ) )
                    continue;

                if( !TryGetSunkValue( ntSymbol, out var closingTypeDb ) )
                {
                    Logger.Error<string>( "Couldn't find TypeDefinition entity for closing type '{0}'", ntSymbol.Name );
                    allOkay = false;

                    continue;
                }

                var closedSet = GetDbSet<ClosedTypeParameter>();

                var closureDb = implDb.ID != 0
                    ? closedSet
                        .FirstOrDefault( gc => gc.ParameterIndex == idx && gc.TypeImplementationID == implDb.ID )
                    : null;

                if( closureDb == null )
                {
                    closureDb = new ClosedTypeParameter
                    {
                        ParameterIndex = idx,
                        TypeImplementation = implDb
                    };

                    closedSet.Add( closureDb );
                }

                closureDb.ClosingTypeID = closingTypeDb!.ID;
                closureDb.Synchronized = true;
            }

            return allOkay;
        }

    }
}
