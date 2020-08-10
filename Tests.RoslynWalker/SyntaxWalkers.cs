using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.walkers;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : TopologicallySortedCollection<ISyntaxWalker>, ISyntaxWalkers
    {
        public SyntaxWalkers(
            IEnumerable<ISyntaxWalker> syntaxWalkers,
            IJ4JLogger logger
        )
        : base( syntaxWalkers, logger )
        {
        }

        public bool Process( List<CompiledProject> compResults )
        {
            var allOkay = true;

            foreach( var walker in ExecutionSequence )
            {
                allOkay &= walker.Process( compResults );
            }

            return allOkay;
        }

        protected override void SetPredecessors( List<ISyntaxWalker> items )
        {
            SetPredecessor<NamespaceWalker, AssemblyWalker>( items );
            SetPredecessor<TypeDefinitionWalker, NamespaceWalker>( items );
            SetPredecessor<MethodWalker, TypeDefinitionWalker>( items );
            SetPredecessor<PropertyWalker, TypeDefinitionWalker>( items );
        }
    }
}