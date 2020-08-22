using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.walkers;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : TopologicallySortedCollection<ISyntaxWalker, AssemblyWalker>, ISyntaxWalkers
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

        protected override void SetPredecessors()
        {
            SetPredecessor<NamespaceWalker, AssemblyWalker>();
            SetPredecessor<TypeDefinitionWalker, NamespaceWalker>();
            SetPredecessor<MethodWalker, TypeDefinitionWalker>();
            //SetPredecessor<PropertyWalker, TypeDefinitionWalker>();
        }
    }
}