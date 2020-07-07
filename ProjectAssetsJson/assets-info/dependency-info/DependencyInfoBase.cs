using System;
using J4JSoftware.Logging;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    public class DependencyInfoBase : ProjectAssetsBase
    {
        protected DependencyInfoBase(
            string assembly,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            Assembly = assembly;
        }

        public string Assembly { get; }
    }
}