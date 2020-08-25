using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class MethodArgumentProcessor : BaseProcessorDb<IParameterSymbol, List<IMethodSymbol>>
    {
        public MethodArgumentProcessor( 
            RoslynDbContext dbContext, 
            ISymbolInfoFactory symbolInfo, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<IParameterSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            foreach( var argSymbol in methodSymbol.Parameters )
            {
                yield return argSymbol;
            }
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol )
        {
            return symbol.Type switch
            {
                INamedTypeSymbol ntSymbol => ProcessNamedTypeSymbol( symbol, ntSymbol ),
                IArrayTypeSymbol arraySymbol => ProcessArraySymbol( symbol, arraySymbol ),
                ITypeParameterSymbol tpSymbol => ProcessTypeParameterSymbol( symbol, tpSymbol ),
                _ => ProcessUnsupportedSymbol( symbol )
            };
        }

        private bool ProcessNamedTypeSymbol( IParameterSymbol symbol, INamedTypeSymbol ntSymbol )
        {
            if( !GetByFullyQualifiedName<TypeDefinition>( ntSymbol, out var tdDb ) )
                return false;

            if( !GetByFullyQualifiedName<Method>( symbol.ContainingSymbol, out var methodDb ) )
                return false;

            ProcessDefinedType( tdDb!, methodDb! );

            return true;
        }

        private bool ProcessArraySymbol( IParameterSymbol symbol, IArrayTypeSymbol arraySymbol )
        {
            if (!GetByFullyQualifiedName<TypeDefinition>(arraySymbol, out var tdDb))
                return false;

            if (!GetByFullyQualifiedName<Method>(symbol.ContainingSymbol, out var methodDb))
                return false;

            ProcessDefinedType(tdDb!, methodDb!);

            return true;
        }

        private void ProcessDefinedType( TypeDefinition tdDb, Method methodDb )
        {
            var tdMethodArgs = GetDbSet<TypeDefinitionMethodArgument>();

            var tdMethodArg = tdMethodArgs
                .FirstOrDefault(x => x.TypeDefinitionID == tdDb.ID && x.DeclaringMethodID == methodDb.ID);

            if (tdMethodArg == null)
            {
                tdMethodArg = new TypeDefinitionMethodArgument
                {
                    DeclaringMethodID = methodDb!.ID,
                    TypeDefinitionID = tdDb.ID
                };

                tdMethodArgs.Add(tdMethodArg);
            }

            tdMethodArg.Synchronized = true;
        }

        private bool ProcessTypeParameterSymbol( IParameterSymbol symbol, ITypeParameterSymbol tpSymbol )
        {
            if (!GetByFullyQualifiedName<Method>(symbol.ContainingSymbol, out var methodDb))
                return false;

            var typeParameters = GetDbSet<TypeParameter>();

            var constraints = tpSymbol.GetTypeParameterConstraint();

            var fqnTypeConstraints = tpSymbol.ConstraintTypes
                .Select(ct => SymbolInfo.GetFullyQualifiedName(ct))
                .ToList();

            // see if the TypeParameter entity is already in the database
            // match on Constraints and identical TypeConstraints
            var tpDb = typeParameters
                .FirstOrDefault(x => x.Constraints == constraints
                                     && !x.TypeConstraints
                                         .Select(tc => tc.ConstrainingType.FullyQualifiedName)
                                         .Except(fqnTypeConstraints).Any()
                                     && x.TypeConstraints.Count == fqnTypeConstraints.Count);

            if( tpDb == null )
            {
                Logger.Error<string>("Couldn't find TypeParameter for symbol '{0}'", tpSymbol.Name);
                return false;
            }

            var tpMethodArgs = GetDbSet<TypeParameterMethodArgument>();

            var tpMethodArg = tpMethodArgs
                .FirstOrDefault(x => x.TypeParameterID == tpDb.ID && x.DeclaringMethodID == methodDb.ID);

            if (tpMethodArg == null)
            {
                tpMethodArg = new TypeParameterMethodArgument
                {
                    DeclaringMethodID = methodDb!.ID,
                    TypeParameterID = tpDb.ID
                };

                tpMethodArgs.Add(tpMethodArg);
            }

            tpMethodArg.Synchronized = true;

            return true;
        }

        private bool ProcessUnsupportedSymbol( IParameterSymbol symbol )
        {
            Logger.Error<string, TypeKind>( "IParameterSymbol '{0}' has an unsupported Type ({1})", 
                symbol.Name,
                symbol.Type.TypeKind );

            return false;
        }
    }
}
