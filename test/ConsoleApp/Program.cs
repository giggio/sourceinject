using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main()
        {
            var services = new ServiceCollection();
            services.AddServicesToDI();
            var serviceProvider = services.BuildServiceProvider();
            var exampleService = serviceProvider.GetRequiredService<ExampleService>();
            Console.WriteLine(exampleService.GetValue());
        }
    }
}
