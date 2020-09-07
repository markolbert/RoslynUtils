using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class AssemblySink : PostProcessDbSink<IAssemblySymbol, AssemblyDb>
    {
        public AssemblySink(
            RoslynDbContext dbContext,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger,
            ISymbolProcessors<IAssemblySymbol>? processors = null )
            : base( dbContext, symbolNamer, sharpObjMapper, logger, processors )
        {
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            MarkUnsynchronized<AssemblyDb>();
            SaveChanges();

            return true;
        }
    }
}
