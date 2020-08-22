using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class MethodSink : RoslynDbSink<IMethodSymbol, Method>
    {
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

            MarkUnsynchronized<Method>();
            MarkUnsynchronized<MethodArgument>();

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


        private bool ProcessSymbol(IMethodSymbol symbol)
        {
            // validate that we can identify all the related entities we'll need to create/update
            // the method entity
            if (!GetByFullyQualifiedName<TypeDefinition>(symbol.ContainingType, out var dtDb))
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

            var allOkay = true;

            // construct/update the argument entities related to the method entity
            foreach (var parameter in symbol.Parameters)
            {
                allOkay &= ProcessParameter(parameter, methodDb, paramTypeEntities);
            }

            foreach( var tpSymbol in symbol.TypeParameters )
            {
                var methodTpDb = ProcessMethodTypeParameter( methodDb, tpSymbol );

                foreach( var conSymbol in tpSymbol.ConstraintTypes )
                {
                    allOkay &= ProcessTypeConstraints( methodTpDb, conSymbol );
                }
            }

            return allOkay;
        }

        private bool ProcessParameter(
            IParameterSymbol argSymbol,
            Method methodDb,
            Dictionary<string, List<TypeDefinition>> paramTypeEntities )
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( argSymbol.Type, out var argTypeDb ) )
                return false;

            var methodArguments = GetDbSet<MethodArgument>();

            var argDb = methodArguments
                .FirstOrDefault( x => x.Name == argSymbol.Name && x.DeclaringMethodID == methodDb.ID );

            if( argDb == null )
            {
                argDb = new MethodArgument();

                if( methodDb.ID == 0 )
                    argDb.DeclaringMethod = methodDb;
                else argDb.DeclaringMethodID = methodDb.ID;

                argDb.Name = argSymbol.Name;

                methodArguments.Add( argDb );
            }

            argDb.Ordinal = argSymbol.Ordinal;
            argDb.ArgumentTypeId = argTypeDb!.ID;
            argDb.IsDiscard = argSymbol.IsDiscard;
            argDb.IsOptional = argSymbol.IsOptional;
            argDb.IsParams = argSymbol.IsParams;
            argDb.IsThis = argSymbol.IsThis;
            argDb.ReferenceKind = argSymbol.RefKind;
            argDb.DefaultText = argSymbol.HasExplicitDefaultValue
                ? argSymbol.ExplicitDefaultValue?.ToString() ?? null
                : null;

            return true;
        }

        private bool GetParameterTypeDefinitions(IMethodSymbol methodSymbol, out Dictionary<string, List<TypeDefinition>> result)
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
                var symbolInfo = SymbolInfo.Create(symbol);

                innerResult = tdSet.FirstOrDefault(x => x.FullyQualifiedName == symbolInfo.SymbolName);

                return innerResult != null;
            }
        }

        private MethodTypeParameter ProcessMethodTypeParameter(Method methodDb, ITypeParameterSymbol tpSymbol)
        {
            var methodTypeParameters = GetDbSet<MethodTypeParameter>();

            var methodTpDb = methodTypeParameters
                .FirstOrDefault(x => x.Ordinal == tpSymbol.Ordinal && x.DeclaringMethodID == methodDb.ID);

            if (methodTpDb == null)
            {
                methodTpDb = new MethodTypeParameter
                {
                    DeclaringMethodID = methodDb.ID,
                    Ordinal = tpSymbol.Ordinal
                };

                methodTypeParameters.Add(methodTpDb);
            }

            methodTpDb.Synchronized = true;
            methodTpDb.Name = tpSymbol.Name;
            methodTpDb.Constraints = tpSymbol.GetTypeParameterConstraint();

            return methodTpDb;
        }

        private bool ProcessTypeConstraints(
            MethodTypeParameter methodTpDb,
            ITypeSymbol constraintSymbol)
        {
            var symbolInfo = SymbolInfo.Create(constraintSymbol);

            if (!(symbolInfo.Symbol is INamedTypeSymbol) && symbolInfo.TypeKind != TypeKind.Array)
            {
                Logger.Error<string>(
                    "Constraining type '{0}' is neither an INamedTypeSymbol nor an IArrayTypeSymbol",
                    symbolInfo.SymbolName);
                return false;
            }

            var typeDefinitions = GetDbSet<TypeDefinition>();

            var conDb = typeDefinitions
                .FirstOrDefault(td => td.FullyQualifiedName == symbolInfo.SymbolName);

            if (conDb == null)
            {
                Logger.Error<string>("Constraining type '{0}' not found in database", symbolInfo.SymbolName);
                return false;
            }

            var typeConstraints = GetDbSet<TypeConstraint>();

            var typeConstraintDb = typeConstraints
                .FirstOrDefault(c => c.TypeParameterBaseID == methodTpDb.ID && c.ConstrainingTypeID == conDb.ID);

            if (typeConstraintDb == null)
            {
                typeConstraintDb = new TypeConstraint
                {
                    ConstrainingTypeID = conDb.ID,
                    TypeParameterBase = methodTpDb
                };

                typeConstraints.Add(typeConstraintDb);
            }

            typeConstraintDb.Synchronized = true;

            return true;
        }
    }
}
