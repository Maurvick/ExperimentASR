using ExperimentASR.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpeechMaster.Models;
using SpeechMaster.Services;
using System.Windows;

namespace SpeechMaster
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            var builder = Host.CreateApplicationBuilder();

            // 1. Register DbContext (Transient allows injection into Singleton windows)
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=app_data.db"),
                ServiceLifetime.Transient);

            // 2. Register Services
            builder.Services.AddTransient<HistoryService>();
            builder.Services.AddTransient<BenchmarkRunner>();

            // 3. Register Windows
            builder.Services.AddSingleton<BenchmarkWindow>();
            builder.Services.AddSingleton<MainWindow>();

            // Build the host
            _host = builder.Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Resolve and show the main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // Graceful shutdown
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}