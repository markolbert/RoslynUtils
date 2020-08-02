using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchronization process
            DbContext.TypeDefinitions.ForEachAsync( td => td.Synchronized = false );
            DbContext.TypeParameters.ForEachAsync( tp => tp.Synchronized = false );
            //foreach( var td in DbContext.TypeDefinitions )
            //{
            //    td.Synchronized = false;
            //}

            //foreach( var tp in DbContext.TypeParameters )
            //{
            //    tp.Synchronized = false;
            //}

            DbContext.SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            // we want to add all the parent types for each symbol's inheritance tree.
            // but to do that we first have to add all the relevant assemblies and namespaces
            var allOkay = true;

            var typeList = _typeSymbols.Select(ts => ts.Value)
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

            foreach( var generic in typeList.Where( td => td.IsGenericType ) )
            {
                allOkay &= ProcessGeneric( generic );
            }

            DbContext.SaveChanges();

            return allOkay;
        }

        private bool ProcessGeneric( INamedTypeSymbol generic )
        {
            var typeName = SymbolName.GetFullyQualifiedName( generic );

            var typeDefDb = DbContext.TypeDefinitions.FirstOrDefault( td =>
                td.FullyQualifiedName == typeName );

            if( typeDefDb == null )
            {
                Logger.Error<string>( "Couldn't find type '{0}' in database", typeName );
                return false;
            }

            var allOkay = true;

            foreach( var tp in generic.TypeParameters )
            {
                var tpDb = ProcessTypeParameter( typeDefDb, tp );

                foreach( var typeConstraint in tp.ConstraintTypes )
                {
                    allOkay &= ProcessTypeConstraint( tpDb, typeConstraint );
                }
            }

            return allOkay;
        }

        private TypeParameter ProcessTypeParameter( TypeDefinition typeDefDb, ITypeParameterSymbol tpSymbol )
        {
            var retVal = DbContext.TypeParameters
                .Include(x => x.TypeConstraints)
                .FirstOrDefault(x => x.ParameterIndex == tpSymbol.Ordinal && x.TypeDefinitionID == typeDefDb.ID);

            if (retVal == null)
            {
                retVal = new TypeParameter
                {
                    TypeDefinitionID = typeDefDb.ID,
                    ParameterIndex = tpSymbol.Ordinal
                };

                DbContext.TypeParameters.Add(retVal);
            }

            retVal.Synchronized = true;
            retVal.ParameterName = tpSymbol.Name;
            retVal.Constraints = tpSymbol.GetGenericConstraints();

            return retVal;
        }

        private bool ProcessTypeConstraint(TypeParameter tpDb, ITypeSymbol typeConstraint )
        {
            var constraintName = SymbolName.GetFullyQualifiedName(typeConstraint);

            var constraintDb = DbContext.TypeDefinitions.FirstOrDefault(td =>
                td.FullyQualifiedName == constraintName);

            if (constraintDb == null)
            {
                Logger.Error<string>("Couldn't find generic constraining type '{0}'", constraintName);
                return false;
            }
            else
            {
                if (tpDb.TypeConstraints == null 
                    || tpDb.TypeConstraints.All(x => x.ConstrainingTypeID != constraintDb.ID))
                    DbContext.TypeConstraints.Add(new TypeConstraint
                    {
                        ConstrainingTypeID = constraintDb.ID,
                        TypeParameter = tpDb
                    });
            }

            return true;
        }

        public override bool TryGetSunkValue(INamedTypeSymbol symbol, out TypeDefinition? result)
        {
            var symbolName = SymbolName.GetFullyQualifiedName(symbol);

            var retVal = DbContext.TypeDefinitions.FirstOrDefault(a => a.FullyQualifiedName == symbolName);

            if (retVal == null)
            {
                result = null;
                return false;
            }

            result = retVal;

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, INamedTypeSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if (retVal.AlreadyProcessed)
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
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                var symbolName = SymbolName.GetFullyQualifiedName( interfaceSymbol );

                if( _typeSymbols.ContainsKey( symbolName ) ) 
                    continue;

                _typeSymbols.Add(symbolName, interfaceSymbol  );

                AddAncestorTypes(interfaceSymbol);
            }

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                var symbolInfo = new SymbolInfo( baseSymbol, SymbolName );

                if( !_typeSymbols.ContainsKey( symbolInfo.SymbolName ) )
                    _typeSymbols.Add( symbolInfo.SymbolName, (INamedTypeSymbol) symbolInfo.Symbol );

                baseSymbol = ( (ITypeSymbol) symbolInfo.Symbol ).BaseType;
            }
        }

        private bool ProcessSymbol( ISyntaxWalker syntaxWalker, SymbolInfo symbolInfo )
        {
            switch( symbolInfo.TypeKind )
            {
                case TypeKind.Error:
                    Logger.Error<string>("Unhandled or incorrect type error for named type '{0}'",
                        symbolInfo.SymbolName);

                    return false;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        symbolInfo.SymbolName, 
                        symbolInfo.TypeKind);

                    return false;
            }

            if ( !_assemblySink.TryGetSunkValue( symbolInfo.Symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !_nsSink.TryGetSunkValue( symbolInfo.Symbol.ContainingNamespace, out var dbNS ) )
                return false;

            var dbSymbol = DbContext.TypeDefinitions.FirstOrDefault( nt => nt.FullyQualifiedName == symbolInfo.SymbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new TypeDefinition() { FullyQualifiedName = symbolInfo.SymbolName };

            if (isNew)
                DbContext.TypeDefinitions.Add(dbSymbol);

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolName.GetName(symbolInfo.OriginalSymbol);
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbolInfo.Symbol.ContainingAssembly );

            DbContext.SaveChanges();

            return true;
        }
    }
}
