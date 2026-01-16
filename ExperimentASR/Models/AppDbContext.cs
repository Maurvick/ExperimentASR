using Microsoft.EntityFrameworkCore;

namespace SpeechMaster.Models
{

	public class AppDbContext : DbContext
	{
		public DbSet<TranscriptionEntry> Transcriptions { get; set; }
		public DbSet<BenchmarkEntry> Benchmarks { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=app_data.db");
		}
	}

	public class TranscriptionEntry
	{
		public int Id { get; set; }
		public DateTime DateTime { get; set; }
		public string ModelName { get; set; }
		public string AudioPath { get; set; }
		public string TranscriptionText { get; set; }
		public double Duration { get; set; }
		public double Wer { get; set; }
		public double Rtf { get; set; }
	}

	public class BenchmarkEntry
	{
		public int Id { get; set; }
		public DateTime DateTime { get; set; }
		public string DatasetName { get; set; }
		public string ResultsJson { get; set; }
	}
}
