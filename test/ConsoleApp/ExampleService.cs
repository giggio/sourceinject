namespace ConsoleApp
{
    [AddService]
    public class ExampleService
    {
        private readonly AnotherService anotherService;

        public ExampleService(AnotherService anotherService) =>
            this.anotherService = anotherService;

        public string GetValue() => anotherService.Value;
    }

    [AddService]
    public class AnotherService
    {
        public string Value => "Hello World!";
    }
}
