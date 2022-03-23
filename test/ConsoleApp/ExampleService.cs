using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

[Inject]
public class ExampleService
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
public class AnotherService : IAnotherService
{
    public string Value => "Hello World!";
}

interface IGeneric { }
interface IGeneric<T> : IGeneric { }
[InjectTransient]
class C : IGeneric<string> { }
