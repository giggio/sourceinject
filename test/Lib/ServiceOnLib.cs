using Microsoft.Extensions.DependencyInjection;

namespace Lib
{
    [Inject(ServiceLifetime.Singleton)]
    public class ServiceOnLib
    {
        public string Value => "Hello from Lib!";
    }
}
