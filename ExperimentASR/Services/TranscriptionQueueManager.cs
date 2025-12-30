using ExperimentASR.Models.Transcription;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ExperimentASR.Services
{
    public class TranscriptionQueueManager
    {
        // This collection binds directly to your UI
        public ObservableCollection<TranscriptionJob> Jobs { get; set; }
            = new ObservableCollection<TranscriptionJob>();

        private bool _isProcessing = false;

        public void AddFile(string path)
        {
            Jobs.Add(new TranscriptionJob
            {
                FileName = System.IO.Path.GetFileName(path),
                FilePath = path,
                Status = "Pending",
                Result = ""
            });
        }

        public async Task StartProcessing()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            // Process items one by one
            while (Jobs.Any(j => j.Status == "Pending"))
            {
                // Get the next pending job
                var currentJob = Jobs.First(j => j.Status == "Pending");

                currentJob.Status = "Processing...";

                // Run the actual transcription on a background thread
                // Replace 'SimulateTranscribe' with your actual ASR function
                var text = await Task.Run(() => SimulateTranscribe(currentJob.FilePath));

                currentJob.Result = text;
                currentJob.Status = "Completed";
            }

            _isProcessing = false;
        }

        // PLUG YOUR ASR CODE HERE
        private string SimulateTranscribe(string path)
        {
            // Simulate a delay (e.g., 2 seconds)
            Thread.Sleep(2000);
            return "This is a simulated transcription result.";
        }
    }
}
