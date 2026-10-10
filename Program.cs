
using LocalMonitoring;
using Spectre.Console;

Console.Title = "System Monitor";

using var monitor = new SystemMonitor();

while (true)
{
    var metrics = monitor.Read();

    AnsiConsole.Clear();

    AnsiConsole.Write(
        new FigletText("SENTINEL")
            .Centered()
            .Color(Color.Green));

    var table = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold cyan]Live System Metrics[/]");

    table.AddColumn("Metric");
    table.AddColumn("Usage");
    table.AddColumn("Status");

    string cpuStatus = metrics.CpuUsage >= 80
        ? "[red]HIGH[/]"
        : "[green]NORMAL[/]";

    double ramPercent = metrics.RamTotalGb > 0
        ? metrics.RamUsedGb / metrics.RamTotalGb * 100
        : 0;

    string ramStatus = ramPercent >= 85
        ? "[red]HIGH[/]"
        : "[green]NORMAL[/]";

    table.AddRow(
        "CPU",
        $"{metrics.CpuUsage:F1}%",
        cpuStatus);

    table.AddRow(
        "RAM",
        $"{metrics.RamUsedGb:F2} / {metrics.RamTotalGb:F2} GB ({ramPercent:F1}%)",
        ramStatus);

    AnsiConsole.Write(table);

    AnsiConsole.MarkupLine(
        "\n[grey]Refreshing every second. Press Ctrl+C to exit.[/]");

    await Task.Delay(1000);
}
