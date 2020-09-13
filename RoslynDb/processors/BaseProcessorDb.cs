using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSource, TResult> : AtomicProcessor<TSource>
        where TResult : class, ISymbol
        where TSource : class, ISymbol
    {
        protected BaseProcessorDb(
            EntityFactories factories,
            IJ4JLogger logger
        )
        : base( logger )
        {
            EntityFactories = factories;
        }

        protected EntityFactories EntityFactories { get; }
        
        protected abstract IEnumerable<TResult> ExtractSymbols( ISymbol item );
        protected abstract bool ProcessSymbol( TResult symbol );

        protected override bool ProcessInternal( IEnumerable<TSource> inputData )
        {
            var allOkay = true;

            try
            {
                foreach( var symbol in FilterSymbols( inputData ) )
                {
                    allOkay = ProcessSymbol( symbol );

                    if( !allOkay && StopOnFirstError )
                        break;
                }
            }
            catch( Exception e )
            {
                return false;
            }

            return allOkay;
        }

        protected override bool FinalizeProcessor(IEnumerable<TSource> inputData)
        {
            if (!base.FinalizeProcessor(inputData))
                return false;

            EntityFactories.DbContext.SaveChanges();

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
                if (typeArg == null)
                    continue;

                if (typeArg is INamedTypeSymbol ntTypeArg && !visitedNames.Any(n => n.Equals(typeArg.Name, StringComparison.Ordinal)))
                {
                    visitedNames.Add(typeArg.Name);

                    if (HasParametricTypes(ntTypeArg, ref visitedNames))
                        return true;
                }
            }

            foreach (var interfaceSymbol in symbol.Interfaces)
            {
                if (!visitedNames.Any(n => n.Equals(interfaceSymbol.Name, StringComparison.Ordinal)))
                {
                    visitedNames.Add(interfaceSymbol.Name);

                    if (HasParametricTypes(interfaceSymbol, ref visitedNames))
                        return true;
                }
            }

            return false;
        }

        private IEnumerable<TResult> FilterSymbols(IEnumerable<TSource> source)
        {
            var processed = new Dictionary<string, TResult>();

            var procName = this.GetType().Name;

            foreach (var item in source)
            {
                if (item == null )
                    continue;

                foreach( var symbol in ExtractSymbols(item) )
                {
                    var crap = EntityFactories.GetFullName( symbol );
                    if( !EntityFactories.GetUniqueName( symbol!, out var fqn ) )
                    {
                        var mesg = $"Couldn't get unique name for ISymbol '{EntityFactories.GetFullName( symbol )}'";

                        Logger.Error( mesg );

                        throw new ArgumentException( mesg );
                    }

                    if (processed.ContainsKey(fqn))
                        continue;

                    processed.Add(fqn, symbol!);

                    yield return symbol!;
                }
            }
        }
    }
}