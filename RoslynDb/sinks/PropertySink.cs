using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class PropertySink : RoslynDbSink<IPropertySymbol, Property>
    {
        public PropertySink(
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

            MarkUnsynchronized<Property>();

            SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            var allOkay = true;

            foreach( var symbol in Symbols )
            {
                allOkay &= ProcessSymbol( symbol );
            }

            SaveChanges();

            return allOkay;
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
                var symbolInfo = SymbolInfo.Create( symbol );

                innerResult = tdSet.FirstOrDefault(x => x.FullyQualifiedName == symbolInfo.SymbolName);

                return innerResult != null;
            }
        }

        private bool ProcessSymbol( IPropertySymbol symbol )
        {
            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if (!GetByFullyQualifiedName<TypeDefinition>(symbol.ContainingType, out var dtDb))
                return false;

            if (!GetByFullyQualifiedName<TypeDefinition>(symbol.Type, out var rtDb))
                return false;

            // get the TypeDefinitions for the parameters, if any
            if (!GetParameterTypeDefinitions(symbol, out var paramTypeEntities))
                return false;

            // construct/update the method entity
            var symbolInfo = SymbolInfo.Create( symbol );

            if (!GetByFullyQualifiedName<Property>(symbol, out var propDb))
                propDb = AddEntity(symbolInfo.SymbolName);

            propDb!.Name = SymbolInfo.GetName(symbol);
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

            return true;
        }

        private void ProcessParameter(
            IParameterSymbol paramSymbol,
            Property propDb,
            Dictionary<string, List<TypeDefinition>> paramTypeEntities )
        {
            var ppSet = GetDbSet<PropertyParameter>();

            var propParamDb = ppSet
                .FirstOrDefault( x => x.Name == paramSymbol.Name && x.PropertyID == propDb.ID );

            if( propParamDb == null )
            {
                propParamDb = new PropertyParameter();

                if( propDb.ID == 0 )
                    propParamDb.Property = propDb;
                else propParamDb.PropertyID = propDb.ID;

                propParamDb.Name = paramSymbol.Name;

                ppSet.Add( propParamDb );
            }

            propParamDb.Ordinal = paramSymbol.Ordinal;
            propParamDb.Name = paramSymbol.Name;

            //ProcessPropertyParameterType( propParamDb, paramTypeEntities[ propParamDb.Name ] );
        }

        //private void ProcessPropertyParameterType(
        //    PropertyParameter propParamDb,
        //    List<TypeDefinition> typeConstraints)
        //{
        //    var tiSet = GetDbSet<TypeAncestor>();

        //    foreach (var constTypeDb in typeConstraints)
        //    {
        //        var tiDb = tiSet
        //            .FirstOrDefault(x =>
        //               x.ChildTypeID == constTypeDb.ID 
        //               && x.PropertyParameter != null
        //               && x.PropertyParameter.ID == propParamDb.ID);

        //        if (tiDb != null)
        //            continue;

        //        tiDb = new TypeAncestor { ChildTypeID = constTypeDb.ID };

        //        if (propParamDb.ID == 0 || tiDb.PropertyParameter == null)
        //            tiDb.PropertyParameter = propParamDb;
        //        else
        //            tiDb.PropertyParameter.ID = propParamDb.ID;

        //        tiSet.Add(tiDb);
        //    }
        //}
    }
}
