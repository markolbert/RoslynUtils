using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
    public class MethodSink : RoslynDbSink<IMethodSymbol, Method>
    {
        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolInfo symbolInfo,
            IJ4JLogger logger )
            : base( dbContext, symbolInfo, logger )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            MarkUnsynchronized<Method>();
            MarkUnsynchronized<MethodParameter>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            SaveChanges();

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, IMethodSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if ( !GetByFullyQualifiedName<TypeDefinition>( symbol.ContainingType, out var dtDb ) )
                return retVal;

            if( !GetByFullyQualifiedName<TypeDefinition>( symbol.ReturnType, out var rtDb ) )
                return retVal;

            if( !GetParameterTypeDefinitions( symbol, out var paramTypeEntities ) )
                return retVal;

            // construct/update the method entity
            if( !GetByFullyQualifiedName<Method>( symbol, out var methodDb ) )
                methodDb = AddEntity( retVal.SymbolName );

            methodDb!.Name = SymbolInfo.GetName( symbol );
            methodDb.Kind = symbol.MethodKind;
            methodDb.ReturnTypeID = rtDb!.ID;
            methodDb.DefiningTypeID = dtDb!.ID;
            methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            methodDb.Accessibility = symbol.DeclaredAccessibility;
            methodDb.Synchronized = true;

            // construct/update the argument entities related to the method entity
            foreach( var parameter in symbol.Parameters )
            {
                ProcessParameter(parameter, methodDb, paramTypeEntities);
            }

            retVal.WasOutput = true;

            return retVal;
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

            ProcessParameterType( methodParamDb, paramTypeEntities[ methodParamDb.Name ] );
        }

        private void ProcessParameterType( 
            MethodParameter methodParamDb,
            List<TypeDefinition> typeConstraints )
        {
            var tiSet = GetDbSet<TypeAncestor>();

            foreach( var constTypeDb in typeConstraints )
            {
                var tiDb = tiSet
                    .FirstOrDefault( x =>
                        x.ChildTypeID == constTypeDb.ID 
                        && x.MethodParameter != null
                        && x.MethodParameter.ID == methodParamDb.ID );

                if( tiDb != null )
                    continue;

                tiDb = new TypeAncestor { ChildTypeID = constTypeDb.ID };

                if( methodParamDb.ID == 0 || tiDb.MethodParameter == null )
                    tiDb.MethodParameter = methodParamDb;
                else
                    tiDb.MethodParameter.ID = methodParamDb.ID;

                tiSet.Add( tiDb );
            }
        }
    }
}
