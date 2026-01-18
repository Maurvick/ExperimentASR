using Microsoft.EntityFrameworkCore;

namespace SpeechMaster.Models
{
	public class AppDbContext : DbContext
	{
		public DbSet<TranscriptionEntry> Transcriptions { get; set; }
		public DbSet<BenchmarkEntry> Benchmarks { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Опціонально: налаштування дати як рядок ISO 8601
			modelBuilder.Entity<TranscriptionEntry>()
				.Property(e => e.DateTime)
				.HasConversion(v => v.ToString("o"), v => DateTime.Parse(v));

			modelBuilder.Entity<BenchmarkEntry>()
				.Property(e => e.DateTime)
				.HasConversion(v => v.ToString("o"), v => DateTime.Parse(v));
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
