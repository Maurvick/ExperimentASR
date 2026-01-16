using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SpeechMaster.Models;
using SpeechMaster.Models.Transcription;

namespace SpeechMaster.Services
{
	class SqliteService
	{
		private readonly AppDbContext _context;

		public SqliteService(AppDbContext context)
		{
			_context = context;
			_context.Database.Migrate();
		}

		public async Task SaveTranscriptionAsync(TranscriptionResult result, string modelName, string audioPath, double duration, double wer, double rtf)
		{
			var entry = new TranscriptionEntry
			{
				DateTime = DateTime.Now,
				ModelName = modelName,
				AudioPath = audioPath,
				TranscriptionText = result.Text,
				Duration = duration,
				Wer = wer,
				Rtf = rtf
			};

			_context.Transcriptions.Add(entry);
			await _context.SaveChangesAsync();
		}

		public async Task SaveBenchmarkAsync(string datasetName, List<BenchmarkResult> results)
		{
			var json = JsonSerializer.Serialize(results);
			var entry = new BenchmarkEntry
			{
				DateTime = DateTime.Now,
				DatasetName = datasetName,
				ResultsJson = json
			};

			_context.Benchmarks.Add(entry);
			await _context.SaveChangesAsync();
		}

		public async Task<List<TranscriptionEntry>> GetTranscriptionHistoryAsync()
		{
			return await _context.Transcriptions.OrderByDescending(e => e.DateTime).ToListAsync();
		}

		public async Task<List<BenchmarkEntry>> GetBenchmarkHistoryAsync()
		{
			return await _context.Benchmarks.OrderByDescending(e => e.DateTime).ToListAsync();
		}

		public static async Task ExportToTxtAsync(string text, string filePath)
		{
			await File.WriteAllTextAsync(filePath, text, Encoding.UTF8);
		}

		public static async Task ExportToSrtAsync(List<Segment> segments, string filePath)
		{
			using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
			for (int i = 0; i < segments.Count; i++)
			{
				var segment = segments[i];
				await writer.WriteLineAsync((i + 1).ToString());
				await writer.WriteLineAsync($"{segment.Start:mm\\:ss,fff} --> {segment.End:mm\\:ss,fff}");
				await writer.WriteLineAsync(segment.Text);
				await writer.WriteLineAsync();
			}
		}

		public static async Task ExportToJsonAsync(object data, string filePath)
		{
			var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
			await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
		}
	}
}
