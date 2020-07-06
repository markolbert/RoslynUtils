using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using NuGet.Versioning;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ConfigurationBase
    {
        private IJ4JLogger? _logger;

        protected ConfigurationBase( Func<IJ4JLogger> loggerFactory )
        {
            LoggerFactory = loggerFactory;
        }

        protected Func<IJ4JLogger> LoggerFactory { get; }

        protected IJ4JLogger Logger
        {
            get
            {
                if( _logger != null ) 
                    return _logger;

                _logger = LoggerFactory();
                _logger.SetLoggedType( this.GetType() );

                return _logger;
            }
        }

        protected TProp GetProperty<TProp>(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false)
        {
            if (string.IsNullOrEmpty(propName))
                return default!;

            var asDict = (IDictionary<string, object>)container;

            // ExpandoObject keys are always case sensitive...so if we want a case insensitive match we have to 
            // go a bit convoluted...
            bool hasKey = false;

            if (caseSensitive) hasKey = asDict.ContainsKey(propName);
            else
            {
                // case insensitive matches
                switch (asDict.Keys.Count(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase)))
                {
                    case 0:
                        // no match; key not found so default value of hasKey is okay
                        break;

                    case 1:
                        // replace the case-insensitive property name with the correctly-cased value
                        propName = asDict.Keys.First(k => k.Equals(propName, StringComparison.OrdinalIgnoreCase));
                        hasKey = true;

                        break;

                    default:
                        // multiple case-insensitive matches; case insensitive doesn't work
                        break;
                }
            }

            // it's okay if optional properties don't exist
            if (!hasKey && optional)
                return default!;

            if (asDict[propName] is TProp retVal)
                return retVal;

            LogAndThrow( $"Could not find property", propName, typeof(ExpandoObject) );

            // we'll never get here but need to keep the compiler happy...
            return default!;
        }

        protected TEnum GetEnum<TEnum>(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false)
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException($"{typeof(TEnum)} is not an enum type");

            var text = GetProperty<string>(container, propName, caseSensitive, optional);

            if (Enum.TryParse(typeof(TEnum), text, true, out var retVal))
                return (TEnum)retVal!;

            LogAndThrow( $"Couldn't create an instance of {typeof(TEnum)}", propName, typeof(ExpandoObject) );

            // we'll never get here but need to keep the compiler happy...
            return default!;
        }


        protected TEnum GetEnum<TEnum>( string text )
        {
            if (Enum.TryParse(typeof(TEnum), text, true, out var retVal))
                return (TEnum)retVal!;

            LogAndThrow( $"Couldn't parse {text} to an instance of {typeof(TEnum)}" );

            // we'll never get here but need to keep the compiler happy...
            return default!;
        }

        protected SemanticVersion GetSemanticVersion(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false)
        {
            var text = GetProperty<string>(container, propName, caseSensitive, optional);

            if (Versioning.GetSemanticVersion(text, out var version))
                return version!;

            LogAndThrow($"Couldn't parse '{version}' to a {typeof(SemanticVersion)}");

            // we'll never get here but need to keep the compiler happy...
            return new SemanticVersion( 0, 0, 0 );
        }

        protected SemanticVersion GetSemanticVersion( string text )
        {
            if (Versioning.GetSemanticVersion(text, out var version))
                return version!;

            LogAndThrow($"Couldn't parse '{version}' to a {typeof(SemanticVersion)}");

            // we'll never get here but need to keep the compiler happy...
            return new SemanticVersion(0, 0, 0);
        }

        protected void LogAndThrow( 
            string message, 
            string? textElement = null, 
            Type? containerType = null,
            [ CallerMemberName ] string callerName = "" )
        {
            var genType = typeof(ProjectAssetsException<>).MakeGenericType( this.GetType() );

#pragma warning disable 8601
            var toThrow = (Exception) Activator.CreateInstance( genType,
                new object[] { message, callerName, textElement, containerType } )!;
#pragma warning restore 8601

            Logger.Error( toThrow.ToString() );

            throw toThrow;
        }
    }
}