using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol, Method>
    {
        private readonly List<IMethodSymbol> _symbols = new List<IMethodSymbol>();

        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolInfoFactory symbolInfo,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if (!base.InitializeSink(syntaxWalker))
                return false;

            _symbols.Clear();

            MarkUnsynchronized<Method>();
            MarkUnsynchronized<MethodParameter>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            var allOkay = true;

            foreach( var symbol in _symbols.Distinct( Comparer ) )
            {
                allOkay &= ProcessSymbol( symbol );
            }

            SaveChanges();

            return allOkay;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, IMethodSymbol symbol )
        {
            if (!base.OutputSymbol(syntaxWalker, symbol))
                return false;

            _symbols.Add( symbol );

            return true;

            //var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            //if( retVal.AlreadyProcessed )
            //    return retVal;

            //// validate that we can identify all the related entities we'll need to create/update
            //// the method entity
            //if ( !GetByFullyQualifiedName<TypeDefinition>( symbol.ContainingType, out var dtDb ) )
            //    return retVal;

            //if( !GetByFullyQualifiedName<TypeDefinition>( symbol.ReturnType, out var rtDb ) )
            //    return retVal;

            //if( !GetParameterTypeDefinitions( symbol, out var paramTypeEntities ) )
            //    return retVal;

            //// construct/update the method entity
            //if( !GetByFullyQualifiedName<Method>( symbol, out var methodDb ) )
            //    methodDb = AddEntity( retVal.SymbolName );

            //methodDb!.Name = SymbolInfo.GetName( symbol );
            //methodDb.Kind = symbol.MethodKind;
            //methodDb.ReturnTypeID = rtDb!.ID;
            //methodDb.DefiningTypeID = dtDb!.ID;
            //methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            //methodDb.Accessibility = symbol.DeclaredAccessibility;
            //methodDb.Synchronized = true;

            //// construct/update the argument entities related to the method entity
            //foreach( var parameter in symbol.Parameters )
            //{
            //    ProcessParameter(parameter, methodDb, paramTypeEntities);
            //}

            //retVal.WasOutput = true;

            //return retVal;
        }

        private bool GetParameterTypeDefinitions( IMethodSymbol methodSymbol, out Dictionary<string, List<TypeDefinition>> result )
        {
            result = new Dictionary<string, List<TypeDefinition>>();
            var tdSet = GetDbSet<TypeDefinition>();

            foreach (var arg in methodSymbol.Parameters)
            {
                result.Add(arg.Name, new List<TypeDefinition>());

                if (arg.Type is ITypeParameterSymbol tpSymbol)
                {
                    foreach (var typeConstraint in tpSymbol.ConstraintTypes.Cast<INamedTypeSymbol>())
                    {
                        if (typeConstraint == null)
                            continue;

                        if( !get_type_definition( typeConstraint, out var tpDb ) )
                            return false;

                        result[arg.Name].Add(tpDb!);
                    }
                }
                else
                {
                    if( !get_type_definition( arg.Type, out var tpDb ) )
                        return false;

                    result[ arg.Name ].Add( tpDb! );
                }
            }

            return true;

            bool get_type_definition( ISymbol symbol, out TypeDefinition? innerResult )
            {
                var symbolInfo = SymbolInfo.Create( symbol );

                innerResult = tdSet.FirstOrDefault( x => x.FullyQualifiedName == symbolInfo.SymbolName );

                return innerResult != null;
            }
        }

        private bool ProcessSymbol( IMethodSymbol symbol )
        {
            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if( !GetByFullyQualifiedName<TypeDefinition>( symbol.ContainingType, out var dtDb ) )
                return false;

            if (!GetByFullyQualifiedName<TypeDefinition>(symbol.ReturnType, out var rtDb))
                return false;

            if (!GetParameterTypeDefinitions(symbol, out var paramTypeEntities))
                return false;

            // construct/update the method entity
            var symbolInfo = SymbolInfo.Create(symbol);

            if (!GetByFullyQualifiedName<Method>(symbol, out var methodDb))
                methodDb = AddEntity(symbolInfo.SymbolName);

            methodDb!.Name = SymbolInfo.GetName(symbol);
            methodDb.Kind = symbol.MethodKind;
            methodDb.ReturnTypeID = rtDb!.ID;
            methodDb.DefiningTypeID = dtDb!.ID;
            methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            methodDb.Accessibility = symbol.DeclaredAccessibility;
            methodDb.Synchronized = true;

            // construct/update the argument entities related to the method entity
            foreach (var parameter in symbol.Parameters)
            {
                ProcessParameter(parameter, methodDb, paramTypeEntities);
            }

            return true;
        }

        private void ProcessParameter(
            IParameterSymbol paramSymbol,
            Method methodDb,
            Dictionary<string, List<TypeDefinition>> paramTypeEntities )
        {
            var mpSet = GetDbSet<MethodParameter>();

            var methodParamDb = mpSet
                .FirstOrDefault( x => x.Name == paramSymbol.Name && x.DeclaringMethodID == methodDb.ID );

            if( methodParamDb == null )
            {
                methodParamDb = new MethodParameter();

                if( methodDb.ID == 0 )
                    methodParamDb.DeclaringMethod = methodDb;
                else methodParamDb.DeclaringMethodID = methodDb.ID;

                methodParamDb.Name = paramSymbol.Name;

                mpSet.Add( methodParamDb );
            }

            methodParamDb.Ordinal = paramSymbol.Ordinal;
            methodParamDb.IsDiscard = paramSymbol.IsDiscard;
            methodParamDb.IsOptional = paramSymbol.IsOptional;
            methodParamDb.IsParams = paramSymbol.IsParams;
            methodParamDb.IsThis = paramSymbol.IsThis;
            methodParamDb.ReferenceKind = paramSymbol.RefKind;
            methodParamDb.DefaultText = paramSymbol.HasExplicitDefaultValue
                ? paramSymbol.ExplicitDefaultValue?.ToString() ?? null
                : null;

            //ProcessParameterType( methodParamDb, paramTypeEntities[ methodParamDb.Name ] );
        }

        //private void ProcessParameterType( 
        //    MethodParameter methodParamDb,
        //    List<TypeDefinition> typeConstraints )
        //{
        //    var typeDefinitions = GetDbSet<TypeDefinition>();

        //    foreach( var constTypeDb in typeConstraints )
        //    {
        //        var tiDb = typeDefinitions.FirstOrDefault( x => x.ID == constTypeDb.ID );

        //        if( tiDb != null )
        //            continue;

        //        tiDb = new TypeAncestor { ChildTypeID = constTypeDb.ID };

        //        if( methodParamDb.ID == 0 || tiDb.MethodParameter == null )
        //            tiDb.MethodParameter = methodParamDb;
        //        else
        //            tiDb.MethodParameter.ID = methodParamDb.ID;

        //        typeDefinitions.Add( tiDb );
        //    }
        //}
    }
}
