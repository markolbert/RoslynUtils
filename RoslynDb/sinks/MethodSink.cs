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
        private readonly ISymbolSink<INamedTypeSymbol, TypeDefinition> _typeSink;

        public MethodSink(
            RoslynDbContext dbContext,
            ISymbolSink<INamedTypeSymbol, TypeDefinition> typeSink,
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            _typeSink = typeSink;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            MarkUnsynchronized<Method>();
            MarkUnsynchronized<GenericMethodParameter>();
            MarkUnsynchronized<ClosedMethodParameter>();

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
            if( !_typeSink.TryGetSunkValue( symbol.ContainingType, out var dtDb ) )
                return retVal;

            var rtInfo = new SymbolInfo( symbol.ReturnType, SymbolName );

            if( !_typeSink.TryGetSunkValue( (INamedTypeSymbol) rtInfo.Symbol, out var rtDb ) )
                return retVal;

            if( !GetParameterTypeDefinitions( symbol, out var paramTypeEntities ) )
                return retVal;

            // construct/update the method entity
            if( !GetByFullyQualifiedName( retVal.SymbolName, out var methodDb ) )
                methodDb = AddEntity( retVal.SymbolName );

            methodDb!.Name = SymbolName.GetName( symbol );
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
                var symbolInfo = new SymbolInfo( symbol, SymbolName );

                innerResult = tdSet.FirstOrDefault( x => x.FullyQualifiedName == symbolInfo.SymbolName );

                return innerResult != null;
            }
        }

        private void ProcessParameter(
            IParameterSymbol arg, 
            Method methodDb,
            Dictionary<string, List<TypeDefinition>> argTypeEntities )
        {
            var genSet = GetDbSet<GenericMethodParameter>();
            var closedSet = GetDbSet<ClosedMethodParameter>();

            var methodArg = arg.Type is ITypeParameterSymbol
                ? (MethodParameter) genSet
                    .FirstOrDefault( x => x.Name == arg.Name && x.DeclaringMethodID == methodDb.ID )
                : (MethodParameter) closedSet
                    .FirstOrDefault( x => x.Name == arg.Name && x.DeclaringMethodID == methodDb.ID );

            if( methodArg == null )
            {
                methodArg = arg.Type is ITypeParameterSymbol
                    ? (MethodParameter) new GenericMethodParameter()
                    : (MethodParameter) new ClosedMethodParameter();

                if( methodDb.ID == 0 )
                    methodArg.DeclaringMethod = methodDb;
                else methodArg.DeclaringMethodID = methodDb.ID;

                methodArg.Name = arg.Name;

                if( methodArg is GenericMethodParameter )
                    genSet.Add( (GenericMethodParameter) methodArg );
                else
                    closedSet.Add( (ClosedMethodParameter) methodArg );
            }

            methodArg.Ordinal = arg.Ordinal;
            methodArg.IsDiscard = arg.IsDiscard;
            methodArg.IsOptional = arg.IsOptional;
            methodArg.IsParams = arg.IsParams;
            methodArg.IsThis = arg.IsThis;
            methodArg.ReferenceKind = arg.RefKind;
            methodArg.DefaultText = arg.HasExplicitDefaultValue ? arg.ExplicitDefaultValue?.ToString() ?? null : null;

            if( arg.Type is ITypeParameterSymbol tpSymbol )
                ProcessGenericParameter( (GenericMethodParameter) methodArg, argTypeEntities[ methodArg.Name ] );
            else
                ( (ClosedMethodParameter) methodArg ).ParameterTypeID = argTypeEntities[ arg.Name ].First().ID;
        }

        private void ProcessGenericParameter( 
            GenericMethodParameter genArg,
            List<TypeDefinition> typeConstraints )
        {
            var mtSet = GetDbSet<MethodTypeConstraint>();

            foreach( var constTypeDb in typeConstraints )
            {
                var constraintDb = mtSet
                    .FirstOrDefault( x =>
                        x.ConstrainingTypeID == constTypeDb.ID && x.GenericMethodArgumentID == genArg.ID );

                if( constraintDb != null )
                    continue;

                constraintDb = new MethodTypeConstraint { ConstrainingTypeID = constTypeDb.ID };

                if( genArg.ID == 0 )
                    constraintDb.GenericMethodParameter = genArg;
                else
                    constraintDb.GenericMethodArgumentID = genArg.ID;

                mtSet.Add( constraintDb );
            }
        }
    }
}
