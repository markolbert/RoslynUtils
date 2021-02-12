#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
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

using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class PropertyProcessor : SimpleProcessorDb<IPropertySymbol>
    {
        public PropertyProcessor(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger )
            : base( "adding Properties to the database", dataLayer, context, logger )
        {
        }

        protected override bool ProcessSymbol( IPropertySymbol symbol )
        {
            return DataLayer.GetProperty( symbol, true, true ) != null;
        }
    }
}