using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol>
    {
        private readonly List<ITypeProcessor> _processors;
        private readonly ITypeProcessor _coreProcessor;
        private readonly List<ITypeSymbol> _typeSymbols = new List<ITypeSymbol>();

        public TypeSink(
            RoslynDbContext dbContext,
            ISymbolName symbolName,
            IEnumerable<ITypeProcessor> typeProcessors,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            var temp = typeProcessors.ToList();

            var error = TopologicalSorter.CreateSequence( temp, out var processors );

            if( error != null )
                Logger.Error( error );

            _coreProcessor = temp.FirstOrDefault(x => x.SupportedType == typeof(ITypeSymbol))
                             ?? throw new NullReferenceException( $"Couldn't find {typeof(ITypeProcessor)} for {typeof(ITypeSymbol)}" );

            _processors = processors ?? new List<ITypeProcessor>();
        }

        public override bool InitializeSink()
        {
            // clear the collection of processed type symbols
            _typeSymbols.Clear();

            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchronization process
            foreach( var ns in DbContext.NamedTypes )
            {
                ns.Synchronized = false;
            }

            DbContext.SaveChanges();

            return true;
        }

        public override bool FinalizeSink()
        {
            if( !base.FinalizeSink() )
                return false;

            // we want to add all the parent types for each symbol's inheritance tree.
            // but to do that we first have to add all the relevant assemblies and namespaces
            var allOkay = true;

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( _typeSymbols );
            }

            return allOkay;
        }

        protected override (OutputResult status, string symbolName) OutputSymbolInternal(ITypeSymbol symbol )
        {
            var (status, symbolName) = base.OutputSymbolInternal(symbol);

            if (status != OutputResult.Succeeded)
                return (status, symbolName);

            // output the symbol to the database
            if( !ProcessSymbol( symbol, symbolName ) )
                return ( OutputResult.Failed, symbolName );

            // store the processed symbol so we can later walk up its inheritance tree
            _typeSymbols.Add( symbol );

            return ( status, symbolName );
        }

        private bool ProcessSymbol(  ITypeSymbol symbol, string symbolName )
        {
            var dbSymbol = DbContext.NamedTypes.FirstOrDefault(nt => nt.FullyQualifiedName == symbolName);

            var nsName = SymbolName.GetSymbolName(symbol.ContainingNamespace);
            var dbNS = DbContext.Namespaces.FirstOrDefault(ns => ns.FullyQualifiedName == nsName);
            if (dbNS == null)
            {
                Logger.Error<string, string>("Could not find Namespace entity '{0}' referenced by named type '{1}'",
                    nsName,
                    symbolName);

                return false;
            }

            var assemblyName = SymbolName.GetSymbolName(symbol.ContainingAssembly);
            var dbAssembly = DbContext.Assemblies.FirstOrDefault(a => a.FullyQualifiedName == assemblyName);
            if (dbAssembly == null)
            {
                Logger.Error<string, string>("Could not find Assembly entity '{0}' referenced by named type '{1}'",
                    assemblyName,
                    symbolName);

                return false;
            }

            var nature = symbol switch
            {
                INamedTypeSymbol ntSymbol => ntSymbol.TypeKind,
                IArrayTypeSymbol arSymbol => TypeKind.Array,
                IDynamicTypeSymbol dynSymbol => TypeKind.Dynamic,
                IPointerTypeSymbol ptrSymbol => TypeKind.Pointer,
                _ => TypeKind.Error
            };

            if (nature == TypeKind.Error)
            {
                Logger.Error<string, string>("Unhandled or incorrect type error for named type '{1}'",
                    assemblyName,
                    symbolName);

                return false;
            }

            bool isNew = dbSymbol == null;

            dbSymbol ??= new NamedType() { FullyQualifiedName = symbolName };

            if (isNew)
                DbContext.NamedTypes.Add(dbSymbol);

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;
            dbSymbol.AssemblyID = dbAssembly.ID;
            dbSymbol.NamespaceId = dbNS.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbol.GetDeclarationModifier();
            dbSymbol.Nature = nature;

            DbContext.SaveChanges();

            return true;
        }
    }
}
