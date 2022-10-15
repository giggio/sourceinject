using Lib;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

[Inject]
public class ExampleService : LibBaseService, IExampleService
{
    private readonly AnotherService anotherService;

    public ExampleService(AnotherService anotherService) =>
        this.anotherService = anotherService;

    public string GetValue() => anotherService.Value;
}

public interface IAnotherService
{
    string Value { get; }
}

[Inject(ServiceLifetime.Singleton)]
public class AnotherService : LibBaseService, IAnotherService
{
    public string Value => "Hello World!";
}

interface IGeneric { }
interface IGeneric<T> : IGeneric { }
[InjectTransient]
class C : IGeneric<string> { }
