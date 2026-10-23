using System.Globalization;
using VyshKat.Core.Model;
using VyshKat.Core.Services;

namespace VyshKat.ConsoleApp;

public sealed class ConsoleApplication
{
    private readonly FleetService fleet;
    private readonly IServiceCenter serviceCenter;
    private readonly TextReader input;
    private readonly TextWriter output;

    public ConsoleApplication(FleetService fleet, IServiceCenter serviceCenter, TextReader input, TextWriter output)
    {
        this.fleet = fleet;
        this.serviceCenter = serviceCenter;
        this.input = input;
        this.output = output;
    }

    public void Run()
    {
        output.WriteLine("ВышКат - Доедем до дедлайна");
        output.WriteLine("Данные хранятся в памяти до завершения программы.");
        while (true)
        {
            output.WriteLine("\n1 - Принять транспорт; 2 - Добавить вещь; 3 - Отчёт;");
            output.WriteLine("4 - Транспорт для новичков; 5 - Инвентарь; 6 - Демонстрация; 0 - Выход");
            try
            {
                switch (ReadText("Выбор: "))
                {
                    case "0":
                        return;
                    case "1":
                        AddTransport();
                        break;
                    case "2":
                        AddEquipment();
                        break;
                    case "3":
                        PrintReport();
                        break;
                    case "4":
                        PrintBeginners();
                        break;
                    case "5":
                        PrintInventory();
                        break;
                    case "6":
                        RunDemo();
                        break;
                    default:
                        output.WriteLine("Неизвестная команда. Выберите пункт от 0 до 6.");
                        break;
                }
            }
            catch (EndOfStreamException)
            {
                output.WriteLine("\nВвод завершён. До свидания!");
                return;
            }
            catch (ArgumentException exception)
            {
                output.WriteLine($"Ошибка: {exception.Message} Добавление отменено.");
            }
            catch (FormatException exception)
            {
                output.WriteLine($"Ошибка: {exception.Message} Добавление отменено.");
            }
        }
    }

    public void RunDemo()
    {
        // Демо не должно менять уже введённые данные.
        FleetService demoFleet = new FleetService(serviceCenter);
        ConsoleApplication demo = new ConsoleApplication(demoFleet, serviceCenter, TextReader.Null, output);
        output.WriteLine("=== Демонстрация на отдельном парке ===");
        demo.PrintReport();
        demo.PrintBeginners();
        demo.PrintInventory();
        demo.PrintAcceptance(new Scooter("S-001", "Самокат Старт", 6, true, 2.5m));
        demo.PrintAcceptance(new Bicycle("B-001", "Велосипед Город", 8, true));
        demo.PrintAcceptance(new ElectricBicycle("E-001", "Электровелосипед Про", 5, true, 1.5m));
        demo.PrintAcceptance(new Scooter("S-002", "Неисправный самокат", 10, false, 9m));
        demo.PrintAcceptance(new Bicycle("s-001", "Повтор номера", 7, true));
        demo.PrintEquipment(new Helmet("H-001", "Шлем"));
        demo.PrintEquipment(new ChargingStation("C-001", "Зарядная станция", 4m));
        demo.PrintEquipment(new Helmet("S-001", "Вещь с номером транспорта"));
        demo.PrintReport();
        demo.PrintBeginners();
        demo.PrintInventory();
        output.WriteLine("Проверка некорректных данных (простота 11):");
        try
        {
            _ = new Bicycle("B-002", "Некорректный велосипед", 11, true);
        }
        catch (ArgumentOutOfRangeException)
        {
            output.WriteLine("Ошибка обработана: простота должна быть от 1 до 10.");
        }

        output.WriteLine("=== Конец демонстрации ===");
    }

    private void AddTransport()
    {
        string kind = ReadText("Вид: 1 - самокат, 2 - велосипед, 3 - электровелосипед: ");
        if (kind != "1" && kind != "2" && kind != "3")
        {
            throw new FormatException("Неизвестный вид транспорта.");
        }

        string number = ReadText("Инвентарный номер: ");
        string name = ReadText("Наименование: ");
        int ease;
        if (!int.TryParse(ReadText("Простота для новичка (1-10): "), out ease))
        {
            throw new FormatException("Простота должна быть целым числом.");
        }

        string condition = ReadText("Технически исправен? (да/нет): ").ToLowerInvariant();
        bool isSound;
        if (condition == "да" || condition == "yes")
        {
            isSound = true;
        }
        else if (condition == "нет" || condition == "no")
        {
            isSound = false;
        }
        else
        {
            throw new FormatException("Введите «да» или «нет».");
        }

        Transport transport;
        switch (kind)
        {
            case "1":
                transport = new Scooter(number, name, ease, isSound, ReadEnergy());
                break;
            case "2":
                transport = new Bicycle(number, name, ease, isSound);
                break;
            default:
                transport = new ElectricBicycle(number, name, ease, isSound, ReadEnergy());
                break;
        }

        PrintAcceptance(transport);
    }

    private void AddEquipment()
    {
        string kind = ReadText("Вид: 1 - шлем, 2 - зарядная станция: ");
        if (kind != "1" && kind != "2")
        {
            throw new FormatException("Неизвестный вид вещи.");
        }

        string number = ReadText("Инвентарный номер: ");
        string name = ReadText("Наименование: ");
        Equipment equipment;
        if (kind == "1")
        {
            equipment = new Helmet(number, name);
        }
        else
        {
            equipment = new ChargingStation(number, name, ReadEnergy());
        }

        PrintEquipment(equipment);
    }

    private string ReadText(string prompt)
    {
        output.Write(prompt);
        string? line = input.ReadLine();
        if (line == null)
        {
            throw new EndOfStreamException();
        }

        return line.Trim();
    }

    private decimal ReadEnergy()
    {
        string value = ReadText("Суточный расход энергии (кВт·ч, например 2,5): ").Replace(',', '.');
        decimal energy;
        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out energy))
        {
            throw new FormatException("Расход энергии должен быть числом.");
        }

        return energy;
    }

    private void PrintAcceptance(Transport transport)
    {
        PrintResult(transport, fleet.AddTransport(transport));
    }

    private void PrintEquipment(Equipment equipment)
    {
        PrintResult(equipment, fleet.AddEquipment(equipment));
    }

    private void PrintResult(IInventoryItem item, AcceptanceResult result)
    {
        string message;
        switch (result)
        {
            case AcceptanceResult.Accepted:
                message = "принято на баланс";
                break;
            case AcceptanceResult.RejectedByInspection:
                message = "отказ: техосмотр не пройден";
                break;
            default:
                message = "отказ: инвентарный номер уже занят";
                break;
        }

        output.WriteLine($"{item.Name} [{item.InventoryNumber}]: {message}.");
    }

    private void PrintReport()
    {
        output.WriteLine($"Транспорт: {fleet.TransportCount}; вещи: {fleet.EquipmentCount}; " +
                         $"всего на балансе: {fleet.GetInventory().Count}.");
        output.WriteLine(FormattableString.Invariant($"Суточная энергия всего парка: {fleet.TotalDailyEnergyKwh:0.##} кВт·ч."));
    }

    private void PrintBeginners()
    {
        output.WriteLine("Транспорт для новичков (простота >= 6):");
        IReadOnlyList<Transport> vehicles = fleet.GetBeginnerTransport();
        if (vehicles.Count == 0)
        {
            output.WriteLine("Подходящего транспорта пока нет.");
        }

        foreach (Transport transport in vehicles)
        {
            output.WriteLine($"{transport.Name} [{transport.InventoryNumber}], простота: {transport.BeginnerEase}.");
        }
    }

    private void PrintInventory()
    {
        output.WriteLine("Инвентарь:");
        IReadOnlyList<IInventoryItem> items = fleet.GetInventory();
        if (items.Count == 0)
        {
            output.WriteLine("Баланс пуст.");
        }

        foreach (IInventoryItem item in items)
        {
            output.WriteLine($"{item.InventoryNumber} | {item.Name}");
        }
    }
}
