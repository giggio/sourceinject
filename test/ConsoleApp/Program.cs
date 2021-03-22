using Microsoft.Extensions.DependencyInjection;
using static System.Console;

var services = new ServiceCollection();
services.Discover();
services.DiscoverInLib();
var serviceProvider = services.BuildServiceProvider();
var exampleService = serviceProvider.GetRequiredService<ConsoleApp.ExampleService>();
WriteLine(exampleService.GetValue());
var serviceOnLib = serviceProvider.GetRequiredService<Lib.ServiceOnLib>();
WriteLine(serviceOnLib.Value);

