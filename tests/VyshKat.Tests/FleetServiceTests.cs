using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VyshKat.ConsoleApp;
using VyshKat.Core.Model;
using VyshKat.Core.Services;

namespace VyshKat.Tests;

public sealed class FleetServiceTests
{
    [Theory]
    [InlineData(true, AcceptanceResult.Accepted, 1)]
    [InlineData(false, AcceptanceResult.RejectedByInspection, 0)]
    public void InspectionDecisionIsReplacedThroughContainer(bool decision, AcceptanceResult expected, int count)
    {
        var inspection = new StubServiceCenter(decision);
        var services = new ServiceCollection().AddVyshKat(TextReader.Null, TextWriter.Null);
        services.RemoveAll<IServiceCenter>();
        services.AddSingleton<IServiceCenter>(inspection);
        using var provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        var fleet = provider.GetRequiredService<FleetService>();
        // Состояние противоположно ответу заглушки: проверяем именно решение зависимости.
        var scooter = new Scooter("S-1", "Самокат", 8, !decision, 2m);

        Assert.Equal(expected, fleet.AddTransport(scooter));
        Assert.Equal(count, fleet.TransportCount);
        Assert.Equal(count * 2m, fleet.TotalDailyEnergyKwh);
        Assert.Equal(count, fleet.GetBeginnerTransport().Count);
        Assert.Equal(count, fleet.GetInventory().Count);
        Assert.Same(scooter, Assert.Single(inspection.Inspected));
    }

    [Fact]
    public void DefaultContainerBuildsWorkingApplicationAndSharedFleet()
    {
        using var provider = new ServiceCollection().AddVyshKat(TextReader.Null, TextWriter.Null)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        Assert.IsType<ServiceCenter>(provider.GetRequiredService<IServiceCenter>());
        Assert.NotNull(provider.GetRequiredService<ConsoleApplication>());
        Assert.Same(provider.GetRequiredService<FleetService>(), provider.GetRequiredService<FleetService>());
    }

    [Fact]
    public void EmptyFleetHasEmptyReports()
    {
        var fleet = NewFleet();
        Assert.Equal(0, fleet.TransportCount);
        Assert.Equal(0, fleet.EquipmentCount);
        Assert.Equal(0m, fleet.TotalDailyEnergyKwh);
        Assert.Empty(fleet.GetInventory());
        Assert.Empty(fleet.GetBeginnerTransport());
    }

    [Fact]
    public void ReportIncludesEnergyOfTransportAndEquipment()
    {
        var fleet = NewFleet();
        fleet.AddTransport(new Scooter("S-1", "Самокат", 6, true, 2.5m));
        fleet.AddTransport(new Bicycle("B-1", "Велосипед", 9, true));
        fleet.AddTransport(new ElectricBicycle("E-1", "Электровелосипед", 5, true, 1.5m));
        fleet.AddEquipment(new Helmet("H-1", "Шлем"));
        fleet.AddEquipment(new ChargingStation("C-1", "Станция", 4m));
        fleet.AddTransport(new Scooter("S-2", "Неисправный", 10, false, 100m));

        Assert.Equal(3, fleet.TransportCount);
        Assert.Equal(2, fleet.EquipmentCount);
        Assert.Equal(8m, fleet.TotalDailyEnergyKwh);
        Assert.Equal(new[] { "S-1", "B-1" }, fleet.GetBeginnerTransport().Select(item => item.InventoryNumber));
        Assert.Equal(5, fleet.GetInventory().Count);
        Assert.Contains(fleet.GetInventory(), item => item.InventoryNumber == "H-1" && item.Name == "Шлем");
    }

    [Fact]
    public void DuplicateTransportIsNotInspectedAgain()
    {
        var inspection = new StubServiceCenter(true);
        var fleet = new FleetService(inspection);
        fleet.AddTransport(new Bicycle("B-1", "Первый", 6, true));

        var result = fleet.AddTransport(new Bicycle(" b-1 ", "Второй", 10, true));

        Assert.Equal(AcceptanceResult.DuplicateInventoryNumber, result);
        Assert.Single(inspection.Inspected);
        Assert.Equal("Первый", Assert.Single(fleet.GetInventory()).Name);
    }

    [Fact]
    public void InventoryNumbersAreUniqueAcrossEquipmentAndTransport()
    {
        var fleet = NewFleet();
        Assert.Equal(AcceptanceResult.Accepted, fleet.AddEquipment(new Helmet("X-1", "Шлем")));
        Assert.Equal(AcceptanceResult.DuplicateInventoryNumber,
            fleet.AddTransport(new Bicycle("x-1", "Велосипед", 6, true)));
        Assert.Equal(AcceptanceResult.DuplicateInventoryNumber,
            fleet.AddEquipment(new ChargingStation("X-1", "Станция", 10m)));
        Assert.Equal(0m, fleet.TotalDailyEnergyKwh);
        Assert.Single(fleet.GetInventory());

        fleet.AddTransport(new Bicycle("B-1", "Велосипед", 6, true));
        Assert.Equal(AcceptanceResult.DuplicateInventoryNumber,
            fleet.AddEquipment(new Helmet("b-1", "Другой шлем")));
    }

    [Fact]
    public void RejectedTransportDoesNotReserveInventoryNumber()
    {
        var fleet = NewFleet();
        Assert.Equal(AcceptanceResult.RejectedByInspection,
            fleet.AddTransport(new Bicycle("B-1", "Неисправный", 6, false)));
        Assert.Equal(AcceptanceResult.Accepted,
            fleet.AddTransport(new Bicycle("B-1", "После ремонта", 6, true)));
        Assert.Equal("После ремонта", Assert.Single(fleet.GetInventory()).Name);
    }

    [Fact]
    public void EquipmentDoesNotRequireTransportInspection()
    {
        var inspection = new StubServiceCenter(false);
        var fleet = new FleetService(inspection);
        Assert.Equal(AcceptanceResult.Accepted, fleet.AddEquipment(new Helmet("H-1", "Шлем")));
        Assert.Empty(inspection.Inspected);
    }

    [Fact]
    public void InventoryReportDoesNotExposeMutableStorage()
    {
        var fleet = NewFleet();
        var helmet = new Helmet("H-1", "Шлем");
        fleet.AddEquipment(helmet);
        var snapshot = fleet.GetInventory();
        ((IInventoryItem[])snapshot)[0] = new Helmet("H-2", "Подмена");
        Assert.Same(helmet, Assert.Single(fleet.GetInventory()));
        fleet.AddEquipment(new Helmet("H-3", "Ещё шлем"));
        Assert.Single(snapshot);
    }

    [Fact]
    public void NewTransportTypeWorksWithoutChangingFleetService()
    {
        var fleet = NewFleet();
        var vehicle = new TestElectricMoped();
        Assert.Equal(AcceptanceResult.Accepted, fleet.AddTransport(vehicle));
        Assert.Same(vehicle, Assert.Single(fleet.GetBeginnerTransport()));
        Assert.Equal(3m, fleet.TotalDailyEnergyKwh);
    }

    [Fact]
    public void NullDependenciesAndItemsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new FleetService(null!));
        var fleet = NewFleet();
        Assert.Throws<ArgumentNullException>(() => fleet.AddTransport(null!));
        Assert.Throws<ArgumentNullException>(() => fleet.AddEquipment(null!));
    }

    private static FleetService NewFleet()
    {
        return new FleetService(new ServiceCenter());
    }

    private sealed class TestElectricMoped : ElectricTransport
    {
        public TestElectricMoped() : base("M-1", "Мопед", 7, true, 3m)
        {
        }
    }

    private sealed class StubServiceCenter : IServiceCenter
    {
        private readonly bool decision;

        public StubServiceCenter(bool decision)
        {
            this.decision = decision;
        }

        public List<Transport> Inspected { get; } = new List<Transport>();

        public bool Inspect(Transport transport)
        {
            Inspected.Add(transport);
            return decision;
        }
    }
}
