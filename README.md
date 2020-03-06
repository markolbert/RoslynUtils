# Roslyn Utilities
This is a collection of utilities I've put together to simplify working on certain Roslyn/Microsoft CodeAnalysis C# 
projects I'm working on.
It includes two sets of classes:
* stuff for dealing with 'versioned text' (e.g., 'netstandard2.1', 'netcoreapp3.0', 'Autofac/5.3.1')
* a parser for version 3 `project.assets.json` files

All the classes here depend on [Autofac](https://autofac.org/) although I suspect it wouldn't be too hard to modify them
to work with another dependency injection library. The classes contain extensive logging based on [Serilog](https://serilog.net/)
as modified by an extension/wrapper library I wrote, [J4JLogging](https://github.com/markolbert/J4JLogging).
## project.assets.json
The `project.assets.json` file contains a wealth of information about the various packages and projects required to build
a Visual Studio project. Unfortunately it's apparently not well-documented. 

What's worse, from a C# perspective, is that a
number of the property names, while valid JSON, don't map directly to a valid C# property name. I believe C# JSON parsers work
around this by editing the "invalid" names on the fly but I'm not sure and wasn't able to find documentation on how the
editing is done by either [NewtonSoft's Json.NET](https://www.newtonsoft.com/json) or 
[Microsoft's System.Text.Json deserializers](https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=netcore-3.1).

So I wrote my own parser. It has two parts:
* `JsonProjectAssetsConverter`, which converts `project.assets.json` to a set of nested ExpandoObjects; and,
* `ProjectAssets` which traverses the ExpandoObject created by `JsonProjectAssetsConverter` and converts it into a well-formed
C# object.

While these routines fulfill my needs they are definitely a work in progress. For one thing, since I don't have any documentation
on what can be in `project.assets.json` I have to work off of the instances created by Visual Studio for me. There's no guarantee
I haven't missed something important. But if nothing else this may serve as a useful starting place for you.

### Documentation
At this point all that's available are these notes. Someday I hope to provide more thorough documentation. But I wrote these
libraries as part of a longer-term project to create a documentation management system...which isn't ready yet.
