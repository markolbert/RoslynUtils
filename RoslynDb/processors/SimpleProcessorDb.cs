﻿#region license

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

using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class SimpleProcessorDb<TSymbol> : BaseProcessorDb<List<TSymbol>, TSymbol>
        where TSymbol : class, ISymbol
    {
        protected SimpleProcessorDb(
            string name,
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            IJ4JLogger? logger
        )
            : base( name, dataLayer, context, logger )
        {
        }

        protected override List<TSymbol> ExtractSymbols( List<TSymbol> inputData )
        {
            return inputData;
        }
    }
}