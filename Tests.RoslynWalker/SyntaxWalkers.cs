using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;

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

        public bool Process( List<CompiledProject> compResults, bool stopOnFirstError = false )
        {
            var allOkay = true;

            foreach( var walker in ExecutionSequence )
            {
                allOkay &= walker.Process( compResults, stopOnFirstError );

                if( !allOkay && stopOnFirstError )
                    break;
            }

            return allOkay;
        }

        protected override bool SetPredecessors()
        {
            return SetPredecessor<NamespaceWalker, AssemblyWalker>()
                   && SetPredecessor<TypeWalker, NamespaceWalker>()
                   && SetPredecessor<MethodWalker, TypeWalker>()
                   && SetPredecessor<PropertyWalker, TypeWalker>()
                   && SetPredecessor<FieldWalker, TypeWalker>();
        }
    }
}