using System;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.Roslyn.Testing
{
    class Program
    {
        static void Main( string[] args )
        {
            var rootDir = args == null || args.Length < 2 ? "C:/Programming" : args[ 1 ];

            var testFiles = Directory.GetFiles( rootDir, "project.assets.json", SearchOption.AllDirectories )
                .Distinct()
                .ToList();

            var logger = AppServiceProvider.Instance.GetRequiredService<IJ4JLogger<Program>>();

            var projAssets = AppServiceProvider.Instance.GetRequiredService<ProjectAssets>();

            var allOkay = true;

            foreach( var testFile in testFiles )
            {
                Console.WriteLine( testFile );
                Console.WriteLine( new string( '=', testFile.Length ) );

                if( projAssets.Initialize( testFile ) )
                    Console.WriteLine( "passed" );
                else allOkay = false;

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine( allOkay ? "all passed" : "errors encountered" );
        }
    }
}
