namespace SpeechMaster.Models
{
    public class BenchmarkResult
    {
        public string ModelName { get; set; }
        public double AverageWer { get; set; }
        public double AverageCer { get; set; }
		public double AverageRtf { get; set; }
        public int TestCount { get; set; }
    }
}
