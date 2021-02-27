#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ElementInfo : BaseInfo
    {
        private static readonly Regex _attributeGroup = new(@"\s*(\[.*\])\s*(.*)", RegexOptions.Compiled);
        private static readonly Regex _attributes = new(@$"\[([^]]*)\]", RegexOptions.Compiled);

        protected ElementInfo( ElementNature nature, ElementSource src )
            : base( nature, src.Name )
        {
            Accessibility = ParseAccessibility( src.Accessibility, out var temp )
                ? temp
                : Accessibility.Undefined;
        }

        public Accessibility Accessibility { get; }

        protected bool ExtractAttributes(string text, out string? postAttribute, out List<string> attributes)
        {
            postAttribute = null;
            attributes = new List<string>();

            var groupMatch = _attributeGroup.Match(text);

            // if there were no attributes just return the text
            if (!groupMatch.Success)
            {
                postAttribute = text.Trim();
                return true;
            }

            switch (groupMatch.Groups.Count)
            {
                case < 2:
                case > 3:
                    return false;

                case > 2:
                    postAttribute = groupMatch.Groups[2].Value.Trim();
                    break;
            }

            var remainder = groupMatch.Groups[1].Value.Trim();

            var itemMatches = _attributes.Matches(remainder);

            if (!itemMatches.Any())
                return false;

            for (var idx = 0; idx < itemMatches.Count; idx++)
            {
                if (itemMatches[idx].Groups.Count != 2)
                    return false;

                attributes.Add(itemMatches[idx].Groups[1].Value.Trim());
            }

            return true;
        }

        protected bool ParseAccessibility(string toParse, out Accessibility result)
        {
            if (string.IsNullOrEmpty(toParse))
            {
                result = Accessibility.Private;
                return true;
            }

            result = Accessibility.Undefined;

            if (!Enum.TryParse(typeof(Accessibility),
                toParse.Replace(" ", string.Empty),
                true,
                out var parsed))
                return false;

            result = (Accessibility)parsed!;

            return true;
        }
    }
}