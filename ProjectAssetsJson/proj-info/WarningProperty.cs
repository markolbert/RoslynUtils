using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class WarningProperty : ProjectAssetsBase, IInitializeFromNamed<List<string>>
    {
        public WarningProperty( IJ4JLogger<ProjectAssetsBase> logger ) 
            : base( logger )
        {
        }

        public WarningType WarningType { get; set; }
        public List<string> Codes { get; set; }

        public bool Initialize( string rawName, List<string> container )
        {
            if( !ValidateInitializationArguments( rawName, container ) )
                return false;

            WarningType warnType;
            if( !Enum.TryParse<WarningType>( rawName, true, out warnType ) )
            {
                Logger.Error($"Couldn't parse '{rawName}' as a {nameof(WarningType)}");

                return false;
            }

            WarningType = warnType;
            Codes = container;

            return true;
        }
    }
}