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
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class FieldProcessors : RoslynDbProcessors<IFieldSymbol>
    {
        public FieldProcessors(
            IRoslynDataLayer dataLayer,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory
        ) : base( "Field processing", dataLayer, context, loggerFactory() )
        {
            AddIndependentNode( new FieldProcessor( dataLayer, context, loggerFactory() ) );
        }

        protected override bool Initialize( List<IFieldSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<FieldDb>();

            return true;
        }
    }
}