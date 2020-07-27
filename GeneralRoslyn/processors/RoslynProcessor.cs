using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Roslyn
{
    public abstract class RoslynProcessor<TInput> : IRoslynProcessor<TInput>
    {
        protected RoslynProcessor(
            IJ4JLogger logger
        )
        {
            Logger = logger;
            Logger.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public abstract bool Process( ISyntaxWalker syntaxWalker, TInput inputData );

        bool IRoslynProcessor.Process( ISyntaxWalker syntaxWalker, object inputData )
        {
            if( inputData is TInput castData )
                return Process( syntaxWalker, castData );

            Logger.Error<Type, Type>( "Expected a {0} but got a {1}", typeof(TInput), inputData.GetType() );

            return false;
        }
    }
}
