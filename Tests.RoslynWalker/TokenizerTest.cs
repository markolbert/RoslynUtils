using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.RoslynWalker
{
    public class TokenizerTest
    {
        private readonly ITokenizer _tokenizer = ServiceProvider.Instance.GetRequiredService<ITokenizer>();

        [Theory]
        [InlineData("C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\DelegateClass.cs", true)]
        //[InlineData("C:\\Programming\\RoslynUtils\\RoslynNetStandardTestLib\\897.cs", false)]
        public void SingleFileTest( string srcPath, bool success )
        {
            _tokenizer.Tokenize( srcPath, out var statements ).Should().Be( success );
        }
    }
}
