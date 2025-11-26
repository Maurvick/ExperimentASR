using ExperimentASR.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace ExperimentASR.Services
{
    public class TranscribeService
    {
        private readonly string _pythonExe;
        private readonly string _scriptPath;
        private readonly string _audioLanguage;
        private readonly string _whisperModelSize;
        private string _rawPythonOutput;

        LogParser _logParser = new LogParser();

        public TranscribeService(string pythonExe = "python", string scriptPath = "asr_engine.py", 
            string audioLanguage = "en", string whisperModelSize = "tiny")
        {
            _pythonExe = pythonExe;
            _scriptPath = scriptPath;
            _audioLanguage = audioLanguage;
            _whisperModelSize = whisperModelSize;
        }

        public string AsrEngineLocation
        {
            get { return _scriptPath;  }
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
                Arguments = $"\"{_scriptPath}\" \"{audioPath}\" \"{_audioLanguage}\"",
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
                    _rawPythonOutput = output;
                    process.WaitForExit();

                    return _logParser.Parse(output, error);
                }
                catch (Exception ex)
                {
                    return new TranscriptResult
                    {
                        Status = "error",
                        Message = $"Failed to start Python process: {ex.Message}. \nRaw python script output: {_rawPythonOutput}"
                    };
                }
            }
        }
    }
}
