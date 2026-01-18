using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SpeechMaster.Models;

namespace ExperimentASR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        // Expose the service provider so windows/services can resolve dependencies
        public IServiceProvider Services => _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            // Register EF Core DbContext with SQLite connection string
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite("Data Source=app_data.db"));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider.Dispose();
            base.OnExit(e);
        }
    }
}
