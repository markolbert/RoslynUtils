﻿using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class BaseProcessorDb<TSource, TResult> : TopoAction<TSource>
        where TResult : class, ISymbol
        where TSource : class, ISymbol
    {
        protected BaseProcessorDb(
            string name,
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger logger
        )
        : base( logger )
        {
            Name = name;
            DataLayer = dataLayer;
            ExecutionContext = context;
        }

        protected IRoslynDataLayer DataLayer { get; }
        protected ActionsContext ExecutionContext { get; }
        
        protected abstract List<TResult> ExtractSymbols( IEnumerable<TSource> inputData );
        protected abstract bool ProcessSymbol( TResult symbol );

        public string Name { get; }

        protected override bool Initialize( IEnumerable<TSource> inputData )
        {
            Logger.Information<string>("Staring {0}...", Name);

            return base.Initialize( inputData );
        }

        protected override bool ProcessLoop( IEnumerable<TSource> inputData )
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
            catch( Exception e )
            {
                return false;
            }

            return allOkay;
        }

        protected override bool Finalize(IEnumerable<TSource> inputData)
        {
            Logger.Information<string>( "...finished {0}", Name );

            if (!base.Finalize(inputData))
                return false;

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
    }
}