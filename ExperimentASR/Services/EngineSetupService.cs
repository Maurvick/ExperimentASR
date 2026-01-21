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
			_httpClient.DefaultRequestHeaders.Add("User-Agent", "SpeechMaster-App");

			_toolsDir = Path.Combine(_baseDir, "Tools", "whisper");

			_whisperExePath = Path.Combine(_toolsDir, "whisper-cli.exe");
		}

		public bool IsEngineInstalled()
		{
			string modelPath = Path.Combine(_baseDir, "Models", DefaultModelName);
			return File.Exists(_whisperExePath) && File.Exists(modelPath);
		}

		public string GetWhisperFolderPath()
		{
			return _toolsDir;
		}

		public async Task EnsureEngineExistsAsync()
		{
			if (!Directory.Exists(_toolsDir)) Directory.CreateDirectory(_toolsDir);

			if (File.Exists(_whisperExePath))
			{
				StatusService.Instance.UpdateStatus("Engine integrity check: OK");
				return;
			}

			string zipPath = Path.Combine(_baseDir, "engine_temp.zip");

			try
			{
				StatusService.Instance.SetProgress(10);
				StatusService.Instance.UpdateStatus("Checking GitHub for latest version...");

				string downloadUrl = await GetLatestDownloadUrlAsync();

				StatusService.Instance.SetProgress(25);
				StatusService.Instance.UpdateStatus("Downloading latest Whisper Engine...");

				await DownloadFileAsync(downloadUrl, zipPath);

				StatusService.Instance.SetProgress(50);
				StatusService.Instance.UpdateStatus("Extracting whisper-cli.exe...");

				ExtractFileFromZip(zipPath, "whisper-cli.exe", _whisperExePath);

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
								name.Contains(".zip", StringComparison.OrdinalIgnoreCase))
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
				throw new Exception($"Failed to connect to GitHub API: {ex.Message}");
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
						// Update progress here
					}
				}
			}
		}

		private void ExtractFileFromZip(string zipPath, string targetFileName, string destinationPath)
		{
			using (ZipArchive archive = ZipFile.OpenRead(zipPath))
			{
				var entry = archive.Entries.FirstOrDefault(e =>
					e.FullName.EndsWith(targetFileName, StringComparison.OrdinalIgnoreCase));

				if (entry != null)
				{
					entry.ExtractToFile(destinationPath, overwrite: true);
				}
				else
				{
					// Find main.exe if whisper-cli.exe is not found
					var structure = string.Join("\n", archive.Entries.Select(e => e.FullName));
					throw new FileNotFoundException($"Could not find '{targetFileName}' in zip.\nContents:\n{structure}");
				}
			}
		}

		public async Task EnsureModelExistsAsync(string modelName = DefaultModelName)
		{
			string modelFolder = Path.Combine(_baseDir, "Models");
			string modelPath = Path.Combine(modelFolder, modelName);
			if (!Directory.Exists(modelFolder)) Directory.CreateDirectory(modelFolder);
			if (File.Exists(modelPath)) { StatusService.Instance.UpdateStatus("Model check: OK"); return; }

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
	}
}