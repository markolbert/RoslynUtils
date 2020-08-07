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
    public class PropertySink : RoslynDbSink<IPropertySymbol, Property>
    {
        private readonly ISymbolSink<INamedTypeSymbol, TypeDefinition> _typeSink;

        public PropertySink(
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
            MarkUnsynchronized<Property>();

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

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, IPropertySymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if( !_typeSink.TryGetSunkValue( symbol.ContainingType, out var dtDb ) )
                return retVal;
            
            var ptInfo = new SymbolInfo( symbol.Type, SymbolName );

            if( !_typeSink.TryGetSunkValue( (INamedTypeSymbol) ptInfo.Symbol, out var rtDb ) )
                return retVal;

            // get the TypeDefinitions for the parameters, if any
            if (!GetParameterTypeDefinitions(symbol, out var paramTypeEntities))
                return retVal;

            // construct/update the method entity
            if ( !GetByFullyQualifiedName( retVal.SymbolName, out var propDb ) )
                propDb = AddEntity( retVal.SymbolName );

            propDb!.Name = SymbolName.GetName( symbol );
            propDb.PropertyTypeID = rtDb!.ID;
            propDb.DefiningTypeID = dtDb!.ID;
            propDb.DeclarationModifier = symbol.GetDeclarationModifier();
            propDb.GetAccessibility = symbol.GetMethod?.DeclaredAccessibility ?? null;
            propDb.SetAccessibility = symbol.SetMethod?.DeclaredAccessibility ?? null;
            propDb.ReturnsByRef = symbol.ReturnsByRef;
            propDb.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            propDb.IsReadOnly = symbol.IsReadOnly;
            propDb.IsWithEvents = symbol.IsWithEvents;
            propDb.IsWriteOnly = symbol.IsWriteOnly;
            propDb.Synchronized = true;

            // construct/update the argument entities related to the method entity
            foreach (var parameter in symbol.Parameters)
            {
                ProcessParameter(parameter, propDb, paramTypeEntities);
            }

            retVal.WasOutput = true;

            return retVal;
        }

        private bool GetParameterTypeDefinitions(IPropertySymbol propSymbol, out Dictionary<string, List<TypeDefinition>> result)
        {
            result = new Dictionary<string, List<TypeDefinition>>();
            var tdSet = GetDbSet<TypeDefinition>();

            foreach (var arg in propSymbol.Parameters)
            {
                result.Add(arg.Name, new List<TypeDefinition>());

                if (arg.Type is ITypeParameterSymbol tpSymbol)
                {
                    foreach (var typeConstraint in tpSymbol.ConstraintTypes.Cast<INamedTypeSymbol>())
                    {
                        if (typeConstraint == null)
                            continue;

                        if (!get_type_definition(typeConstraint, out var tpDb))
                            return false;

                        result[arg.Name].Add(tpDb!);
                    }
                }
                else
                {
                    if (!get_type_definition(arg.Type, out var tpDb))
                        return false;

                    result[arg.Name].Add(tpDb!);
                }
            }

            return true;

            bool get_type_definition(ISymbol symbol, out TypeDefinition? innerResult)
            {
                var symbolInfo = new SymbolInfo(symbol, SymbolName);

                innerResult = tdSet.FirstOrDefault(x => x.FullyQualifiedName == symbolInfo.SymbolName);

                return innerResult != null;
            }
        }

        private void ProcessParameter(
            IParameterSymbol paramSymbol,
            Property propDb,
            Dictionary<string, List<TypeDefinition>> paramTypeEntities)
        {
            var genSet = GetDbSet<GenericPropertyParameter>();
            var closedSet = GetDbSet<ClosedPropertyParameter>();

            var propParam = paramSymbol.Type is ITypeParameterSymbol
                ? (PropertyParameter)genSet
                    .FirstOrDefault(x => x.Name == paramSymbol.Name && x.PropertyID == propDb.ID)
                : (PropertyParameter)closedSet
                    .FirstOrDefault(x => x.Name == paramSymbol.Name && x.PropertyID == propDb.ID);

            if (propParam == null)
            {
                propParam = paramSymbol.Type is ITypeParameterSymbol
                    ? (PropertyParameter)new GenericPropertyParameter()
                    : (PropertyParameter)new ClosedPropertyParameter();

                if (propDb.ID == 0)
                    propParam.Property = propDb;
                else propParam.PropertyID = propDb.ID;

                propParam.Name = paramSymbol.Name;

                if (propParam is GenericPropertyParameter)
                    genSet.Add((GenericPropertyParameter)propParam);
                else
                    closedSet.Add((ClosedPropertyParameter)propParam);
            }

            propParam.ParameterIndex = paramSymbol.Ordinal;
            propParam.Name = paramSymbol.Name;

            if (paramSymbol.Type is ITypeParameterSymbol tpSymbol)
                ProcessGenericParameter((GenericPropertyParameter)propParam, paramTypeEntities[propParam.Name]);
            else
                ((ClosedPropertyParameter)propParam).ParameterTypeID = paramTypeEntities[paramSymbol.Name].First().ID;
        }

        private void ProcessGenericParameter(
            GenericPropertyParameter genParam,
            List<TypeDefinition> typeConstraints)
        {
            var ptSet = GetDbSet<PropertyTypeConstraint>();

            foreach (var constTypeDb in typeConstraints)
            {
                var constraintDb = ptSet
                    .FirstOrDefault(x =>
                       x.ConstrainingTypeID == constTypeDb.ID && x.GenericPropertyParameterID == genParam.ID);

                if (constraintDb != null)
                    continue;

                constraintDb = new PropertyTypeConstraint { ConstrainingTypeID = constTypeDb.ID };

                if (genParam.ID == 0)
                    constraintDb.GenericPropertyParameter = genParam;
                else
                    constraintDb.GenericPropertyParameterID = genParam.ID;

                ptSet.Add(constraintDb);
            }
        }
    }
}
