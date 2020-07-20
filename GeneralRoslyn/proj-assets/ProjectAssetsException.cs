using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsException : Exception
    {
        public static ProjectAssetsException CreateAndLog(
            string message,
            Type callingType,
            IJ4JLogger logger,
            [ CallerMemberName ] string callingMethod = "" )
        {
            var retVal = new ProjectAssetsException(message, callingType, callingMethod);

            logger.Error( retVal.ToString() );

            return retVal;
        }

        public ProjectAssetsException(string message, Type callingType, string callingMethod)
            : base(message)
        {
            CallingMethod = callingMethod;
            CallingType = callingType;
        }

        public Type CallingType { get; }
        public string CallingMethod { get; }
        public string? TextElement { get; set; }
        public Type? ContainerType { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder(Message);

            sb.Append( "In " );
            sb.Append( CallingMethod.Equals( ".ctor", StringComparison.OrdinalIgnoreCase )
                ? $"the constructor for {CallingType.Name}: "
                : $"{CallingType.Name}:{CallingMethod}():" );
            sb.Append( Message );

            if( !string.IsNullOrEmpty( TextElement ) )
            {
                if( sb.Length > 0 )
                    sb.Append( " [" );

                sb.Append( $"TextElement was '{TextElement}'" );
            }

            if (ContainerType != null )
            {
                if( sb.Length > 0 )
                    sb.Append( string.IsNullOrEmpty( TextElement ) ? " [" : ", " );

                sb.Append($"ContainerType was '{ContainerType}']");
            }
            else
            {
                if( !string.IsNullOrEmpty( TextElement ) )
                    sb.Append( "]" );
            }

            return sb.ToString();
        }
    }
}
