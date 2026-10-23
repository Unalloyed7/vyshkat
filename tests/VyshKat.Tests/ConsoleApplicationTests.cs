using Microsoft.Extensions.DependencyInjection;
using VyshKat.ConsoleApp;
using VyshKat.Core.Services;

namespace VyshKat.Tests;

public sealed class ConsoleApplicationTests
{
    [Fact]
    public void InteractiveSessionCoversAllTransportAndEquipmentTypes()
    {
        var (text, fleet) = Run("""
            1
            1
            S-1
            Самокат
            6
            да
            2,5
            1
            2
            B-1
            Велосипед
            10
            yes
            1
            3
            E-1
            Электровелосипед
            5
            да
            1.5
            1
            1
            S-2
            Сломанный самокат
            7
            нет
            100
            2
            1
            H-1
            Шлем
            2
            2
            C-1
            Станция
            4
            3
            4
            5
            0
            """);

        Assert.Contains("Транспорт: 3; вещи: 2; всего на балансе: 5", text);
        Assert.Contains("8 кВт·ч", text);
        Assert.Contains("отказ: техосмотр не пройден", text);
        Assert.Contains("H-1 | Шлем", text);
        Assert.Equal(2, fleet.GetBeginnerTransport().Count);
    }

    [Theory]
    [InlineData("1\n2\nB-1\nВелосипед\n11\nда\n", "Простота должна быть от 1 до 10")]
    [InlineData("2\n1\n\nШлем\n", "Ошибка:")]
    [InlineData("2\n2\nC-1\nСтанция\n-1\n", "Ошибка:")]
    public void InvalidInputCancelsAdditionAndKeepsMenuUsable(string script, string expected)
    {
        var (text, fleet) = Run(script + "3\n0\n");
        Assert.Contains(expected, text);
        Assert.Contains("всего на балансе: 0", text);
        Assert.Empty(fleet.GetInventory());
    }

    private static (string Text, FleetService Fleet) Run(string script)
    {
        using var input = new StringReader(script);
        using var output = new StringWriter();
        using var provider = new ServiceCollection().AddVyshKat(input, output).BuildServiceProvider();
        provider.GetRequiredService<ConsoleApplication>().Run();
        return (output.ToString(), provider.GetRequiredService<FleetService>());
    }
}
