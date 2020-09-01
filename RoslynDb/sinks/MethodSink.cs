using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol, MethodDb>
    {
        private readonly ISymbolSetProcessor<IMethodSymbol> _processors;

        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISymbolSetProcessor<IMethodSymbol> processors,
            IJ4JLogger logger )
            : base( dbContext, symbolNamer, logger )
        {
            _processors = processors;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            MarkUnsynchronized<MethodDb>();
            MarkUnsynchronized<ArgumentDb>();
            MarkUnsynchronized<MethodParametricTypeDb>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            return _processors.Process(Symbols);
            //var allOkay = true;

            //foreach( var symbol in Symbols )
            //{
            //    allOkay &= ProcessSymbol( symbol );
            //}

            //SaveChanges();

            //return allOkay;
        }


        //private bool ProcessSymbol(IMethodSymbol methodSymbol)
        //{
        //    // validate that we can identify all the related entities we'll need to create/update
        //    // the method entity
        //    if (!GetByFullyQualifiedName<TypeDefinition>(methodSymbol.ContainingType, out var dtDb))
        //        return false;

        //    if (!GetByFullyQualifiedName<TypeDefinition>(methodSymbol.ReturnType, out var rtDb))
        //        return false;

        //    // construct/update the method entity
        //    var symbolInfo = SymbolInfo.Create(methodSymbol);

        //    if (!GetByFullyQualifiedName<Method>(methodSymbol, out var methodDb))
        //        methodDb = AddEntity(symbolInfo.SymbolName);

        //    methodDb!.Name = SymbolInfo.GetName(methodSymbol);
        //    methodDb.Kind = methodSymbol.MethodKind;
        //    methodDb.ReturnTypeID = rtDb!.ID;
        //    methodDb.DefiningTypeID = dtDb!.ID;
        //    methodDb.DeclarationModifier = methodSymbol.GetDeclarationModifier();
        //    methodDb.Accessibility = methodSymbol.DeclaredAccessibility;
        //    methodDb.Synchronized = true;

        //    return true;
        //}

        //private bool GetArgumentTypeDefinitions(IMethodSymbol methodSymbol, out Dictionary<string, List<TypeDefinition>> result)
        //{
        //    result = new Dictionary<string, List<TypeDefinition>>();
        //    var tdSet = GetDbSet<TypeDefinition>();

        //    foreach (var arg in methodSymbol.Parameters)
        //    {
        //        result.Add(arg.Name, new List<TypeDefinition>());

        //        if (arg.Type is ITypeParameterSymbol tpSymbol)
        //        {
        //            foreach (var typeConstraint in tpSymbol.ConstraintTypes.Cast<INamedTypeSymbol>())
        //            {
        //                if (typeConstraint == null)
        //                    continue;

        //                if (!get_type_definition(typeConstraint, out var tpDb))
        //                    return false;

        //                result[arg.Name].Add(tpDb!);
        //            }
        //        }
        //        else
        //        {
        //            if (!get_type_definition(arg.Type, out var tpDb))
        //                return false;

        //            result[arg.Name].Add(tpDb!);
        //        }
        //    }

        //    return true;

        //    bool get_type_definition(ISymbol symbol, out TypeDefinition? innerResult)
        //    {
        //        var symbolInfo = SymbolInfo.Create(symbol);

        //        innerResult = tdSet.FirstOrDefault(x => x.FullyQualifiedName == symbolInfo.SymbolName);

        //        return innerResult != null;
        //    }
        //}
    }
}
