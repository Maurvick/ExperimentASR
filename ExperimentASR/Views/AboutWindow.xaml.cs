using System.Diagnostics;
using System.Windows;

namespace ExperimentASR.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            GetAppVersion();
		}

        private void GetAppVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            TxtVersion.Text = $"Version: {version}";
		} 

        private void LinkProjectRepo_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            string url = "https://github.com/Maurvick/ExperimentASR";
			Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
		}

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
		}   
    }
}
