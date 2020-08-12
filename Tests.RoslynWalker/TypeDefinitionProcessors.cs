using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;

namespace Tests.RoslynWalker
{
    public sealed class TypeDefinitionProcessors 
        : TopologicallySortedCollection<IAtomicProcessor<TypeProcessorContext>, TypeAssemblyProcessor>, ITypeDefinitionProcessors
    {
        public TypeDefinitionProcessors( 
            IEnumerable<IAtomicProcessor<TypeProcessorContext>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override void SetPredecessors()
        {
            SetPredecessor<TypeNamespaceProcessor, TypeAssemblyProcessor>();
            SetPredecessor<TypeDiscoveredTypesProcessor, TypeNamespaceProcessor>();
            SetPredecessor<TypeGenericTypesProcessor, TypeDiscoveredTypesProcessor>();
            SetPredecessor<TypeAncestorProcessor, TypeGenericTypesProcessor>();
        }

        public bool Process( TypeProcessorContext context )
        {
            var allOkay = true;

            foreach( var processor in ExecutionSequence )
            {
                allOkay &= processor.Process( context );
            }

            return allOkay;
        }
    }
}