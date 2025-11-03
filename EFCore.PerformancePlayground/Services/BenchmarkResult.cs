namespace EFCore.PerformancePlayground.Services;

public class BenchmarkResult
{
    public string ScenarioName { get; set; } = string.Empty;
    public string Approach1 { get; set; } = string.Empty;
    public long Approach1Time { get; set; }
    public string Approach2 { get; set; } = string.Empty;
    public long Approach2Time { get; set; }
    public double Speedup { get; set; }
    public long TimeSaved { get; set; }
    public double TimeSavedPercent { get; set; }
}

