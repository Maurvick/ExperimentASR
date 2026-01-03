using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
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
