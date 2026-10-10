
using LocalMonitoring;
using System.Diagnostics;
//using System.Diagnostics.PerformanceCounter;

namespace LocalMonitoring;

public sealed class SystemMonitor : IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _ramCounter;

    public SystemMonitor()
    {
        _cpuCounter = new PerformanceCounter( 
            "Processor",
            "% Processor Time",
            "_Total");

        _ramCounter = new PerformanceCounter(
            "Memory",
            "% Committed Bytes In Use");

        // اولین مقدار CPU برای آماده‌سازی شمارنده است.
        _cpuCounter.NextValue();
    }

    public SystemMetrics Read()
    {
        double cpu = _cpuCounter.NextValue();
        double ram = _ramCounter.NextValue();

        // حافظه فیزیکی از طریق GC به دست نمی‌آید؛
        // مقدار کل و آزاد را از API ویندوز می‌گیریم.
        var memory = MemoryInfo.Get();

        return new SystemMetrics(
            Math.Clamp(cpu, 0, 100),
            Math.Clamp(ram, 0, 100),
            memory.UsedGb,
            memory.TotalGb);
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
        _ramCounter.Dispose();
    }
}

public readonly record struct MemorySnapshot(
    double UsedGb,
    double TotalGb);

public static class MemoryInfo
{
    public static MemorySnapshot Get()
    {
        var status = new NativeMemoryStatus();
        status.Length = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMemoryStatus>();

        if (!GlobalMemoryStatusEx(ref status))
            throw new InvalidOperationException(
                "Unable to read physical memory information.");

        double total = status.TotalPhysical / 1024d / 1024d / 1024d;
        double available = status.AvailablePhysical / 1024d / 1024d / 1024d;

        return new MemorySnapshot(
            total - available,
            total);
    }

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct NativeMemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [System.Runtime.InteropServices.DllImport(
        "kernel32.dll",
        EntryPoint = "GlobalMemoryStatusEx",
        SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(
        ref NativeMemoryStatus buffer);
}
