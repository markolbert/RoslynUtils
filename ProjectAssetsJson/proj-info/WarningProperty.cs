using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class WarningProperty : ConfigurationBase, IInitializeFromNamed<List<string>>
    {
        public WarningProperty( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        public WarningType WarningType { get; private set; }
        public List<string> Codes { get; } = new List<string>();

        public bool Initialize( string rawName, List<string> container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            WarningType warnType;
            if( !Enum.TryParse<WarningType>( rawName, true, out warnType ) )
            {
                Logger.Error<string, string>( $"Couldn't parse '{0}' as a {1}", rawName, nameof(WarningType) );

                return false;
            }

            WarningType = warnType;

            Codes.Clear();
            Codes.AddRange(container);

            return true;
        }
    }
}