using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using NuGet.Versioning;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsBase
    {
        private IJ4JLogger? _logger;

        protected ProjectAssetsBase( Func<IJ4JLogger> loggerFactory )
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
            bool optional = false,
            [CallerMemberName] string callerName = "" )
        {
            if( container.GetProperty<TProp>( propName, out var result, caseSensitive, optional ) )
                return result;

            var toThrow = new ProjectAssetsException( 
                $"Couldn't get {typeof(TProp)} property '{propName}'", 
                this.GetType(), 
                callerName );

            Logger.Error( toThrow.ToString() );

            throw toThrow;
        }

        protected TEnum GetEnum<TEnum>(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false,
            [CallerMemberName] string callerName = "")
        {
            if( container.GetEnum<TEnum>( propName, out var retVal, caseSensitive, optional ) )
                return retVal;

            var toThrow = new ProjectAssetsException(
                $"Couldn't get {typeof(TEnum)} property '{propName}'",
                this.GetType(),
                callerName);

            Logger.Error(toThrow.ToString());

            throw toThrow;
        }

        protected TEnum GetEnum<TEnum>( string text, [CallerMemberName] string callerName = "" )
        {
            if( text.ToEnum<TEnum>( out var retVal ) )
                return retVal;

            var toThrow = new ProjectAssetsException(
                $"Couldn't create {typeof(TEnum)} from '{text}'",
                this.GetType(),
                callerName);

            Logger.Error(toThrow.ToString());

            throw toThrow;
        }

        protected SemanticVersion GetSemanticVersion(
            ExpandoObject container,
            string propName,
            bool caseSensitive = false,
            bool optional = false,
            [CallerMemberName] string callerName = "")
        {
            if ( container.GetSemanticVersion( propName, out var retVal, caseSensitive, optional ) )
                return retVal;

            var toThrow = new ProjectAssetsException(
                $"Couldn't get {typeof(SemanticVersion)} property '{propName}'",
                this.GetType(),
                callerName);

            Logger.Error(toThrow.ToString());

            throw toThrow;
        }

        protected SemanticVersion GetSemanticVersion( string text, [CallerMemberName] string callerName = "")
        {
            if( text.ToSemanticVersion( out var retVal ) )
                return retVal;
            
            var toThrow = new ProjectAssetsException(
                $"Couldn't create {typeof(SemanticVersion)} from '{text}'",
                this.GetType(),
                callerName);

            Logger.Error(toThrow.ToString());

            throw toThrow;
        }

        protected TargetFramework GetTargetFramework(
            string text,
            TargetFrameworkTextStyle style,
            [ CallerMemberName ] string callerName = "" )
        {
            if (TargetFramework.Create(text, style, out var retVal))
                return retVal;


            var toThrow = new ProjectAssetsException(
                $"Couldn't create {typeof(TargetFramework)} from '{text}'",
                this.GetType(),
                callerName);

            Logger.Error(toThrow.ToString());

            throw toThrow;
        }
    }
}