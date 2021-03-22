# Source Inject

A source generator for C# that uses [Roslyn](https://github.com/dotnet/roslyn) (the C# compiler) to allow you to generate
your dependencies injection during compile time. By doing this
you avoid using reflection and services are automatically
registered.

<!-- [![Nuget count](https://img.shields.io/nuget/v/sourceinject.svg)](https://www.nuget.org/packages/sourceinject/) -->

[![License](https://img.shields.io/github/license/giggio/sourceinject.svg)](https://github.com/giggio/sourceinject/blob/master/LICENSE.txt)

## How to use it

Install it and add an attribute to the classes you want injected in your service provider, like so:

```csharp
[Inject]
public class ExampleService
{
    private readonly AnotherService anotherService;

    public ExampleService(AnotherService anotherService) =>
        this.anotherService = anotherService;

    public string GetValue() => anotherService.Value;
}

[Inject(ServiceLifetime.Singleton)]
public class AnotherService
{
    public string Value => "Hello World!";
}
```

As you can see above you can define the lifetime. The default lifetime is transient.
You can also use the following attributes:

-   `InjectSingletonAttribute`
-   `InjectScopedAttribute`
-   `InjectTransientAttribute`

The last one is the same as using `Inject` without any arguments.

You then have to call the `Discover` method so these classes are found and the source is generated.
You can then require them using constructor injection or service locator, for example:

```csharp
var services = new ServiceCollection();
services.Discover();
var serviceProvider = services.BuildServiceProvider();
var exampleService = serviceProvider.GetRequiredService<ExampleService>();
```

You can also discover services in other assemblies. To be able to do this you have to call
the method `DiscoverIn<AssemblyName>` or `<AssemblyName>Discoverer.Discover(services)`. If your
assembly name has dots `.` in them they will be replaced by underscore `_`.

All these methods (`Discover` et all) and attributes will be generated in your project for you.

You can see the generated code using Visual Studio. See [here](https://docs.microsoft.com/en-us/visualstudio/releases/2019/media/16.9/16.9_p3_source_generators_node.png) for an example.

## Installing

The package is available ([on NuGet](https://www.nuget.org/packages/sourceinject).
To install from the command line:

```shell
dotnet add package sourceinject --prerelease
```

Or use the Package Manager in Visual Studio.

## Contributing

The main supported IDE for development is Visual Studio 2019.

Questions, comments, bug reports, and pull requests are all welcome.
Bug reports that include steps to reproduce (including code) are
preferred. Even better, make them in the form of pull requests.
Before you start to work on an existing issue, check if it is not assigned
to anyone yet, and if it is, talk to that person.

## Maintainers/Core team

-   [Giovanni Bassi](http://blog.lambda3.com.br/L3/giovannibassi/), aka Giggio,
    [Lambda3](http://www.lambda3.com.br), [@giovannibassi](https://twitter.com/giovannibassi)

Contributors can be found at the [contributors](https://github.com/giggio/sourceinject/graphs/contributors) page on Github.

## License

This software is open source, licensed under the MIT License.
See [LICENSE](https://github.com/giggio/sourceinject/blob/master/LICENSE) for details.
Check out the terms of the license before you contribute, fork, copy or do anything
with the code. If you decide to contribute you agree to grant copyright of all your contribution to this project and agree to
mention clearly if do not agree to these terms. Your work will be licensed with the project at MIT, along the rest of the code.
