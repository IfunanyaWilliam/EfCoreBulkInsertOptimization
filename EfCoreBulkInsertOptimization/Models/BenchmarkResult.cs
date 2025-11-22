namespace EfCoreBulkInsertOptimization.Models
{
    public class BenchmarkResult
    {
        public string? Action { get; set; }
        public int Entities { get; set; }
        public string? Performance { get; set; }
        public string? TimeFaster { get; set; }
        public string? ReducedPercent { get; set; }
    }
}
