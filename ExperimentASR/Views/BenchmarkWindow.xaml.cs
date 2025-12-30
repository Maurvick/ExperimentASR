using System;
using System.Collections.Generic;
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
    /// Interaction logic for BenchmarkWindow.xaml
    /// </summary>
    public partial class BenchmarkWindow : Window
    {
        public BenchmarkWindow()
        {
            InitializeComponent();
        }

        private void btnBenchmark_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for benchmark logic
            MessageBox.Show("Benchmark started!");
        }
    }
}
