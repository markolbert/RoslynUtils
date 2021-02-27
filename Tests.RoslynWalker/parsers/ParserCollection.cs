using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class ParserCollection
    {
        private readonly List<IParse> _parsers;

        public ParserCollection()
        {
            _parsers = new List<IParse>
            {
                new ParseUsing(),
                new ParseNamespace(),
                new ParseClass(),
                new ParseInterface(),
                new ParseDelegate(),
                new ParseMethod(),
                new ParseProperty(),
                new ParseEvent(),
                new ParseField()
            };
        }

        public List<BaseInfo>? Parse( SourceLine srcLine )
        {
            foreach( var parser in _parsers )
            {
                var parsed = parser.Parse( srcLine );

                if( parsed != null )
                    return parsed;
            }

            return null;
        }
    }
}