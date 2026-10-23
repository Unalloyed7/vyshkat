using Microsoft.Extensions.DependencyInjection;
using VyshKat.Core.Services;

namespace VyshKat.ConsoleApp;

public static class CompositionRoot
{
    public static IServiceCollection AddVyshKat(this IServiceCollection services, TextReader input, TextWriter output)
    {
        services.AddSingleton<IServiceCenter, ServiceCenter>();
        services.AddSingleton<FleetService>();
        services.AddSingleton(input);
        services.AddSingleton(output);
        services.AddTransient<ConsoleApplication>();
        return services;
    }
}
