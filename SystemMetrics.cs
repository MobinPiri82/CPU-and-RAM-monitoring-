
namespace LocalMonitoring;

public sealed record SystemMetrics(
    double CpuUsage,
    double RamUsage,
    double RamUsedGb,
    double RamTotalGb
);
