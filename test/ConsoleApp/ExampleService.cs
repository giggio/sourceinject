using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp
{
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
}

