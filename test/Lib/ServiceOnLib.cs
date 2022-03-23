using Microsoft.Extensions.DependencyInjection;

namespace Lib;

[Inject(ServiceLifetime.Singleton)]
public class ServiceOnLib
{
#pragma warning disable CA1822 // Mark members as static
    public string Value => "Hello from Lib!";
#pragma warning restore CA1822 // Mark members as static
}
