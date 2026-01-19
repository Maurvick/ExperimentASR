using SpeechMaster.Models;
using SpeechMaster.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ExperimentASR.Views
{
	/// <summary>
	/// Interaction logic for BenchmarkWindow.xaml
	/// </summary>
	public partial class BenchmarkWindow : Window
    {
		private readonly DatasetLoader _datasetReader = new();
		private readonly EngineManager _engineManager = new();

        public BenchmarkWindow()
        { 
            InitializeComponent();
        }

        private async void BtnBenchmark_Click(object sender, RoutedEventArgs e)
        {
			var engines = _engineManager.AvailableEngines;
			StatusService.Instance.UpdateStatus("Loading dataset...");

			StatusService.Instance.UpdateStatus($"{_datasetReader.TestItems.Count} " +
				$"samples loaded from dataset...");

			var results = new ObservableCollection<BenchmarkResult>();
			BenchmarkDataGrid.ItemsSource = results;

			foreach (var engine in engines)
			{
				StatusService.Instance.UpdateStatus($"Testing {engine.Name}...");
			}

			StatusService.Instance.UpdateStatus("✅ Benchmark completed!");
		}
	}
}
