#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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

using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public partial class DataLayer : IDataLayer
    {
        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        public DataLayer(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public void Deprecate()
        {
            _dbContext.Deprecate();
        }

        public void SaveChanges() => _dbContext.SaveChanges();
    }
}