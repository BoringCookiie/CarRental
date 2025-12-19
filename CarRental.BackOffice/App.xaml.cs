using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CarRental.Data;
using CarRental.Services;
using CarRental.Core.Interfaces;
using CarRental.BackOffice.ViewModels;
using CarRental.BackOffice.Views;
using Microsoft.EntityFrameworkCore;

namespace CarRental.BackOffice
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            // Add global exception handlers
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show($"Fatal Error:\n{ex?.Message}\n\n{ex?.InnerException?.Message}", 
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            
            DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show($"Dispatcher Error:\n{e.Exception.Message}\n\n{e.Exception.InnerException?.Message}", 
                    "Dispatcher Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            try
            {
                Console.WriteLine("Starting App constructor...");
                
                AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        // Database
                        var connectionString = "Server=localhost;Database=CarRentalDB;User=root;Password=admin;";
                        services.AddDbContext<CarRentalDbContext>(options =>
                            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)),
                            ServiceLifetime.Transient);

                        // Core Services
                        services.AddTransient<IUnitOfWork, UnitOfWork>();
                        services.AddTransient<IVehicleService, VehicleService>();
                        services.AddTransient<IRentalService, RentalService>();
                        services.AddTransient<IAuthService, AuthService>();
                        services.AddTransient<IImportExportService, ImportExportService>();
                        services.AddTransient<INotificationService, NotificationService>();
                        services.AddTransient<IPdfService, PdfService>();
                        services.AddTransient<IQrCodeService, QrCodeService>();
                        
                        // ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<DashboardViewModel>();
                        services.AddTransient<VehicleViewModel>();
                        services.AddTransient<ClientViewModel>();
                        services.AddTransient<RentalViewModel>();

                        // Windows
                        services.AddTransient<LoginWindow>();
                        services.AddTransient<MainWindow>();
                    })
                    .Build();
                    
                Console.WriteLine("App constructor completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Constructor Error:\n{ex.Message}\n\n{ex.InnerException?.Message}", 
                    "Constructor Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await AppHost!.StartAsync();

                var startupWindow = AppHost.Services.GetRequiredService<LoginWindow>();
                startupWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start:\n\n{ex.Message}\n\nDetails:\n{ex.InnerException?.Message}", 
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}

