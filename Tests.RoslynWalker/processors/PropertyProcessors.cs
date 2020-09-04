using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class PropertyProcessors 
        : SymbolProcessors<IPropertySymbol> // TopologicallySortedCollection<IAtomicProcessor<IEnumerable<IPropertySymbol>>, PropertyProcessor>, ISymbolSetProcessor<IPropertySymbol>
    {
        public PropertyProcessors( 
            IEnumerable<IAtomicProcessor<IPropertySymbol>> items, 
            IJ4JLogger logger 
        ) : base( items, logger )
        {
        }

        protected override bool SetPredecessors()
        {
            return SetPredecessor<ParameterProcessor, PropertyProcessor>();
        }

        //// ensure the context object is able to reset itself so it can 
        //// handle multiple iterations
        //public bool Process( IEnumerable<IPropertySymbol> context )
        //{
        //    var allOkay = true;

        //    foreach( var processor in ExecutionSequence )
        //    {
        //        allOkay &= processor.Process( context );
        //    }

        //    return allOkay;
        //}
    }
}