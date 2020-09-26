﻿using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeInScopeAssemblyInfoProcessor : BaseProcessorDb<ITypeSymbol, IAssemblySymbol>
    {
        public TypeInScopeAssemblyInfoProcessor(
            IRoslynDataLayer dataLayer,
            ExecutionContext context,
            IJ4JLogger logger)
            : base(dataLayer, context, logger)
        {
        }

        protected override List<IAssemblySymbol> ExtractSymbols( IEnumerable<ITypeSymbol> inputData )
        {
            var retVal = new List<IAssemblySymbol>();

            foreach (var symbol in inputData)
            {
                var assemblySymbol = symbol is IArrayTypeSymbol arraySymbol
                    ? arraySymbol.ElementType.ContainingAssembly
                    : symbol.ContainingAssembly;

                if (assemblySymbol == null)
                    Logger.Information<string>("ITypeSymbol '{0}' does not have a ContainingAssembly", symbol.Name);
                else
                {
                    if (!ExecutionContext.InDocumentationScope(symbol.ContainingAssembly))
                        continue;

                    if (ExecutionContext.HasCompiledProject(symbol.ContainingAssembly))
                        retVal.Add(symbol.ContainingAssembly);
                    else
                        Logger.Error<string>("Couldn't find CompiledProject for IAssemblySymbol '{0}'",
                            symbol.ContainingAssembly.ToUniqueName());
                }
            }

            return retVal;
        }

        // symbol is guaranteed to be in the documentation scope and having an associated CompiledProject
        protected override bool ProcessSymbol( IAssemblySymbol symbol ) =>
            DataLayer.GetInScopeAssemblyInfo( ExecutionContext[ symbol ]!, true, true ) != null;
    }
}