using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class SortedTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeSymbol>
    {
        public SortedTypeProcessor(
            IRoslynDataLayer dataLayer,
            IJ4JLogger logger)
            : base(dataLayer, logger)
        {
        }

        protected override IEnumerable<ITypeSymbol> ExtractSymbols( ISymbol item )
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if (typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol)
            {
                Logger.Error<string>("Unhandled ITypeSymbol '{0}'", typeSymbol.Name);
                yield break;
            }

            if (typeSymbol is IErrorTypeSymbol)
            {
                Logger.Error("ITypeSymbol is an IErrorTypeSymbol, ignored");
                yield break;
            }

            var fullName = typeSymbol.ToFullName();

            if( DataLayer.SharpObjectInDatabase<BaseTypeDb>( typeSymbol ) )
                yield break;

            yield return typeSymbol;
        }

        protected override bool ProcessSymbol( ITypeSymbol typeSymbol )
        {
            var fullName = typeSymbol.ToFullName();

            if( DataLayer.GetUnspecifiedType( typeSymbol, true ) == null )
                return false;

            DataLayer.SaveChanges();

            return true;
        }

        //private bool ProcessFixedType( INamedTypeSymbol symbol )
        //{
        //    if (!EntityFactories.Get<AssemblyDb>(symbol.ContainingAssembly, out var assemblyDb))
        //        return false;

        //    if (!EntityFactories.Get<NamespaceDb>(symbol.ContainingNamespace, out var nsDb))
        //        return false;

        //    if (!EntityFactories.Create<FixedTypeDb>(symbol, out var dbSymbol))
        //    {
        //        Logger.Error<string>("Could not create entity for '{0}'",
        //            symbol.ToFullName());

        //        return false;
        //    }

        //    DataLayer.MarkSynchronized(dbSymbol!);

        //    dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
        //    dbSymbol.NamespaceID = nsDb!.SharpObjectID;

        //    return true;
        //}

        //private bool ProcessGenericType( INamedTypeSymbol symbol )
        //{
        //    if( !EntityFactories.Get<AssemblyDb>( symbol.ContainingAssembly, out var assemblyDb ) )
        //        return false;

        //    if( !EntityFactories.Get<NamespaceDb>( symbol.ContainingNamespace, out var nsDb ) )
        //        return false;

        //    if( !EntityFactories.Create<GenericTypeDb>( symbol, out var dbSymbol ) )
        //    {
        //        Logger.Error<string>( "Could not create entity for '{0}'",
        //            symbol.ToFullName() );

        //        return false;
        //    }

        //    DataLayer.MarkSynchronized( dbSymbol! );

        //    dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
        //    dbSymbol.NamespaceID = nsDb!.SharpObjectID;

        //    return true;
        //}

        //private bool ProcessArrayType(IArrayTypeSymbol symbol)
        //{
        //    var fqn = symbol.ToFullName();

        //    if (!EntityFactories.Get<AssemblyDb>(symbol.ElementType.ContainingAssembly, out var assemblyDb))
        //        return false;

        //    if (!EntityFactories.Get<NamespaceDb>(symbol.ElementType.ContainingNamespace, out var nsDb))
        //        return false;

        //    if (!EntityFactories.Create<BaseTypeDb>(symbol, out var dbSymbol))
        //    {
        //        Logger.Error<string>("Could not retrieve TypeDb entity for '{0}'",
        //            symbol.ToFullName());

        //        return false;
        //    }

        //    DataLayer.MarkSynchronized(dbSymbol!);

        //    dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
        //    dbSymbol.NamespaceID = nsDb!.SharpObjectID;

        //    return true;
        //}

        //private bool ProcessTypeParameter(ITypeParameterSymbol symbol)
        //{
        //    if (!EntityFactories.Get<AssemblyDb>(symbol.ContainingAssembly, out var assemblyDb))
        //        return false;

        //    if (!EntityFactories.Get<NamespaceDb>(symbol.ContainingNamespace, out var nsDb))
        //        return false;

        //    if (!EntityFactories.Create<ParametricTypeDb>(symbol, out var dbSymbol))
        //    {
        //        Logger.Error<string>("Could not retrieve ParametricTypeDb entity for '{0}'",
        //            symbol.ToFullName());

        //        return false;
        //    }

        //    DataLayer.MarkSynchronized(dbSymbol!);

        //    dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
        //    dbSymbol.NamespaceID = nsDb!.SharpObjectID;

        //    return true;
        //}

    }
}
