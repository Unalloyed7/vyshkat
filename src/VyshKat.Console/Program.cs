using System.Text;
using Microsoft.Extensions.DependencyInjection;
using VyshKat.ConsoleApp;

namespace VyshKat;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length > 1 || (args.Length == 1 && args[0] != "--demo"))
        {
            Console.Error.WriteLine("Запуск: dotnet run --project src/VyshKat.Console [-- --demo]");
            return 2;
        }

        ServiceCollection services = new ServiceCollection();
        services.AddVyshKat(Console.In, Console.Out);

        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            ConsoleApplication application = provider.GetRequiredService<ConsoleApplication>();
            if (args.Length == 1)
            {
                application.RunDemo();
            }
            else
            {
                application.Run();
            }
        }

        return 0;
    }
}
