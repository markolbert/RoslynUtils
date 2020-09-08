using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class WarningProperty : ProjectAssetsBase
    {
        public WarningProperty( 
            string text,
            List<string> codes,
            Func<IJ4JLogger> loggerFactory
            ) 
            : base( loggerFactory )
        {
            WarningType = GetEnum<WarningType>(text);
            Codes = codes;
        }

        public WarningType WarningType { get; }
        public List<string> Codes { get; }
    }
}