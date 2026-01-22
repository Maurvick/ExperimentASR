using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpeechMaster.Services
{
	public class EngineSetupService
	{
		private const string RepoApiUrl = "https://api.github.com/repos/ggerganov/whisper.cpp/releases/latest";
		private const string ModelBaseUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/";
		private const string DefaultModelName = "ggml-base.bin";

		private readonly string _baseDir;
		private readonly HttpClient _httpClient;
		private readonly string _toolsDir;
		private readonly string _whisperExePath;

		public EngineSetupService()
		{
			_baseDir = AppDomain.CurrentDomain.BaseDirectory;
			_httpClient = new HttpClient();
			// User-Agent обов'язковий для GitHub API
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "SpeechMaster-App");

			// Шлях: .../Tools/whisper/
			_toolsDir = Path.Combine(_baseDir, "Tools", "whisper");
			_whisperExePath = Path.Combine(_toolsDir, "whisper-cli.exe");
		}

		/// <summary>
		/// Перевіряє, чи існують файли рушія та моделі.
		/// </summary>
		public bool IsEngineInstalled()
		{
			string modelPath = Path.Combine(_baseDir, "Models", DefaultModelName);
			// Перевіряємо наявність exe та моделі
			return File.Exists(_whisperExePath) && File.Exists(modelPath);
		}

		/// <summary>
		/// Повертає шлях до папки з інструментами (для відкриття в Explorer тощо).
		/// </summary>
		public string GetWhisperFolderPath()
		{
			return _toolsDir;
		}

		public async Task EnsureEngineExistsAsync()
		{
			if (!Directory.Exists(_toolsDir)) Directory.CreateDirectory(_toolsDir);

			// Перевіряємо наявність .exe.
			if (File.Exists(_whisperExePath))
			{
				// Швидка перевірка: чи є поруч хоча б якісь .dll? 
				var dllFiles = Directory.GetFiles(_toolsDir, "*.dll");
				if (dllFiles.Length > 0)
				{
					StatusService.Instance.UpdateStatus("Engine integrity check: OK");
					return;
				}
			}

			string zipPath = Path.Combine(_baseDir, "engine_temp.zip");

			try
			{
				StatusService.Instance.SetProgress(10);
				StatusService.Instance.UpdateStatus("Checking GitHub for latest version...");

				string downloadUrl = await GetLatestDownloadUrlAsync();

				StatusService.Instance.SetProgress(25);
				StatusService.Instance.UpdateStatus("Downloading Whisper Engine...");

				await DownloadFileAsync(downloadUrl, zipPath);

				StatusService.Instance.SetProgress(50);
				StatusService.Instance.UpdateStatus("Extracting engine files...");

				// Розпаковуємо все (exe + dll)
				ExtractAllFilesFromZip(zipPath, _toolsDir);

				StatusService.Instance.SetProgress(75);
				StatusService.Instance.UpdateStatus("Engine installed successfully.");
			}
			catch (Exception ex)
			{
				StatusService.Instance.UpdateStatus($"Engine Setup Failed: {ex.Message}");
				if (File.Exists(_whisperExePath)) File.Delete(_whisperExePath);
				throw;
			}
			finally
			{
				if (File.Exists(zipPath)) File.Delete(zipPath);
			}
		}

		public async Task EnsureModelExistsAsync(string modelName = DefaultModelName)
		{
			string modelFolder = Path.Combine(_baseDir, "Models");
			string modelPath = Path.Combine(modelFolder, modelName);

			if (!Directory.Exists(modelFolder)) Directory.CreateDirectory(modelFolder);

			if (File.Exists(modelPath))
			{
				StatusService.Instance.UpdateStatus("Model check: OK");
				return;
			}

			StatusService.Instance.UpdateStatus($"Downloading Model: {modelName}...");
			try
			{
				await DownloadFileAsync($"{ModelBaseUrl}{modelName}", modelPath);
				StatusService.Instance.UpdateStatus($"Model {modelName} ready.");
			}
			catch (Exception ex)
			{
				if (File.Exists(modelPath)) File.Delete(modelPath);
				throw;
			}
		}

		// --- Private Helpers ---

		private void ExtractAllFilesFromZip(string zipPath, string targetDirectory)
		{
			using (ZipArchive archive = ZipFile.OpenRead(zipPath))
			{
				foreach (var entry in archive.Entries)
				{
					if (string.IsNullOrEmpty(entry.Name)) continue;

					// Розпаковуємо exe та dll, ігноруючи структуру папок
					if (entry.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
						entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
					{
						string destinationPath = Path.Combine(targetDirectory, entry.Name);
						entry.ExtractToFile(destinationPath, overwrite: true);
					}
				}
			}

			if (!File.Exists(Path.Combine(targetDirectory, "whisper-cli.exe")))
			{
				throw new FileNotFoundException("Critical file 'whisper-cli.exe' was not found in the downloaded archive.");
			}
		}

		private async Task<string> GetLatestDownloadUrlAsync()
		{
			try
			{
				string jsonResponse = await _httpClient.GetStringAsync(RepoApiUrl);
				using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
				{
					if (doc.RootElement.TryGetProperty("assets", out JsonElement assets))
					{
						foreach (JsonElement asset in assets.EnumerateArray())
						{
							string name = asset.GetProperty("name").GetString() ?? "";
							if (name.Contains("bin-x64", StringComparison.OrdinalIgnoreCase) &&
								name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
							{
								return asset.GetProperty("browser_download_url").GetString();
							}
						}
					}
				}
				throw new Exception("Could not find a 'bin-x64' zip asset in the latest GitHub release.");
			}
			catch (Exception ex)
			{
				throw new Exception($"GitHub API Error: {ex.Message}");
			}
		}

		private async Task DownloadFileAsync(string url, string destinationPath)
		{
			using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
			{
				if (!response.IsSuccessStatusCode)
					throw new HttpRequestException($"Download failed. Status: {response.StatusCode}");

				var totalBytes = response.Content.Headers.ContentLength ?? -1L;

				using (var contentStream = await response.Content.ReadAsStreamAsync())
				using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
				{
					var buffer = new byte[8192];
					long totalRead = 0;
					int bytesRead;
					while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
					{
						await fileStream.WriteAsync(buffer, 0, bytesRead);
						totalRead += bytesRead;
						if (totalBytes != -1 && totalRead % (1024 * 100) == 0)
						{
							double progress = Math.Round((double)totalRead / totalBytes * 100, 0);
							StatusService.Instance.SetProgress(progress);
						}
					}
				}
			}
		}
	}
}