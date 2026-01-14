using ExperimentASR.Services;
using System.Windows;

namespace ExperimentASR.Windows
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
			// Subscribe to our global status service
			StatusService.Instance.OnStatusChanged += UpdateStatus;
            StatusService.Instance.OnProgressChanged += SetProgress;
		}

		private void UpdateStatus(string message)
		{
			Dispatcher.Invoke(() => StatusText.Text = message);
		}

        private void SetProgress(double value)
        {
            Dispatcher.Invoke(() => MyProgressBar.Value = value);
		}
    }
}