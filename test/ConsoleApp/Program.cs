using Microsoft.Extensions.DependencyInjection;
var services = new ServiceCollection();
services.AddServicesToDI();
var serviceProvider = services.BuildServiceProvider();
var exampleService = serviceProvider.GetRequiredService<ConsoleApp.ExampleService>();
System.Console.WriteLine(exampleService.GetValue());
