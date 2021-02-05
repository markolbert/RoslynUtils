using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSource, TResult> : IAction<TSource>
        where TResult : class, ISymbol
    {
        protected BaseProcessorDb(
            string name,
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger
        )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );

            Name = name;
            DataLayer = dataLayer;
            ExecutionContext = context;
        }

        protected IJ4JLogger? Logger { get; }
        protected IRoslynDataLayer DataLayer { get; }
        protected ActionsContext ExecutionContext { get; }
        
        protected abstract List<TResult> ExtractSymbols( TSource inputData );
        protected abstract bool ProcessSymbol( TResult symbol );

        public string Name { get; }

        protected virtual bool Initialize( TSource inputData )
        {
            Logger?.Information<string>("Starting {0}...", Name);
            return true;
        }

        public virtual bool Process( TSource inputData )
        {
            var allOkay = true;

            var processed = new HashSet<string>();

            try
            {
                foreach( var symbol in ExtractSymbols(inputData) )
                {
                    var fqn = symbol.ToUniqueName();

                    // skip symbols we've already processed
                    if( processed.Contains( fqn ) )
                        continue;

                    allOkay = ProcessSymbol( symbol );

                    if( !allOkay && ExecutionContext.StopOnFirstError )
                        break;

                    processed.Add( fqn );
                }
            }
            catch
            {
                return false;
            }

            return allOkay;
        }

        protected virtual bool Finalize(TSource inputData)
        {
            Logger?.Information<string>( "...finished {0}", Name );

            DataLayer.SaveChanges();

            return true;
        }

        protected bool HasParametricTypes(INamedTypeSymbol symbol, ref List<string> visitedNames)
        {
            INamedTypeSymbol? curSymbol = symbol;

            while (curSymbol != null)
            {
                if (curSymbol.TypeArguments.Any(ta => ta is ITypeParameterSymbol))
                    return true;

                curSymbol = curSymbol!.BaseType;
            }

            foreach (var typeArg in symbol.TypeArguments)
            {
                switch( typeArg )
                {
                    case null:
                        continue;

                    case INamedTypeSymbol ntTypeArg when !visitedNames.Any(n => n.Equals(typeArg.Name, StringComparison.Ordinal)):
                    {
                        visitedNames.Add(typeArg.Name);

                        if (HasParametricTypes(ntTypeArg, ref visitedNames))
                            return true;
                        break;
                    }
                }
            }

            foreach (var interfaceSymbol in symbol.Interfaces)
            {
                if( visitedNames.Any( n => n.Equals( interfaceSymbol.Name, StringComparison.Ordinal ) ) ) 
                    continue;

                visitedNames.Add(interfaceSymbol.Name);

                if (HasParametricTypes(interfaceSymbol, ref visitedNames))
                    return true;
            }

            return false;
        }

        // processors are equal if they are the same type, so duplicate instances of the 
        // same type are always equal (and shouldn't be present in the processing set)
        public bool Equals( IAction<TSource>? other )
        {
            if (other == null)
                return false;

            return other.GetType() == GetType();
        }

        bool IAction.Process( object src )
        {
            if( src is TSource castSrc )
                return Process( castSrc );

            Logger?.Error( "Expected a {0} but received a {1}", typeof(TSource), src.GetType() );

            return false;
        }
    }
}