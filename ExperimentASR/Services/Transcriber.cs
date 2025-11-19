using ExperimentASR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ExperimentASR.Services
{
    public class Transcriber
    {
        private readonly string _pythonExe;
        private readonly string _scriptPath;

        public Transcriber(string pythonExe = "python", string scriptPath = "asr.py")
        {
            _pythonExe = pythonExe;
            _scriptPath = scriptPath;
        }

        public TranscriptResult Transcribe(string audioPath)
        {
            if (!File.Exists(audioPath))
            {
                throw new FileNotFoundException("Audio file not found.", audioPath);
            }
            if (!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException("asr.py not found.", _scriptPath);
            }

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _pythonExe,
                Arguments = $"\"{_scriptPath}\" \"{audioPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = new System.Diagnostics.Process { StartInfo = processStartInfo })
            {
                try
                {
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // If Python printed JSON, parse it
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<TranscriptResult>(output);
                        }
                        catch
                        {
                            // If Python printed non-JSON, wrap it
                            return new TranscriptResult
                            {
                                Status = "error",
                                Message = "Invalid JSON from script:\n" + output
                            };
                        }
                    }

                    // Python may have crashed before printing JSON
                    return new TranscriptResult
                    {
                        Status = "error",
                        Message = "Python error:\n" + error
                    };

                }
                catch (Exception ex)
                {
                    return new TranscriptResult
                    {
                        Status = "error",
                        Message = "Failed to start Python process: " + ex.Message
                    };
                }
            }
        }
    }
}
