using CarRental.BackOffice.Views;
using CarRental.Core.Entities;
using CarRental.Core.Enums;
using CarRental.Core.Interfaces;
using CarRental.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CarRental.BackOffice.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _title = "Car Rental - Admin";

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            // Initial View - Dashboard with real data
            try 
            {
                CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void NavigateToDashboard() => CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>();

        [RelayCommand]
        private void NavigateToVehicles() => CurrentView = _serviceProvider.GetRequiredService<VehicleViewModel>();

        [RelayCommand]
        private void NavigateToRentals() => CurrentView = _serviceProvider.GetRequiredService<RentalViewModel>();

        [RelayCommand]
        private void NavigateToClients() => CurrentView = _serviceProvider.GetRequiredService<ClientViewModel>();
        
        [RelayCommand]
        private void Logout()
        {
            // Resolve LoginWindow from DI to ensure dependencies are injected
            if (App.AppHost?.Services.GetService<LoginWindow>() is LoginWindow loginWindow)
            {
                 loginWindow.Show();
                 Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            }
            else 
            {
                // Fallback if DI fails (shouldn't happen)
                System.Diagnostics.Process.Start(Environment.ProcessPath!);
                Application.Current.Shutdown();
            }
        }
    }
    
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IVehicleService _vehicleService;
        private readonly IUnitOfWork _unitOfWork;

        [ObservableProperty]
        private int _activeRentalsCount;

        [ObservableProperty]
        private int _availableVehiclesCount;

        [ObservableProperty]
        private int _totalClientsCount;

        [ObservableProperty]
        private int _pendingRentalsCount;

        [ObservableProperty]
        private decimal _totalRevenue;

        public DashboardViewModel(IRentalService rentalService, IVehicleService vehicleService, IUnitOfWork unitOfWork)
        {
            _rentalService = rentalService;
            _vehicleService = vehicleService;
            _unitOfWork = unitOfWork;
            _ = LoadStatisticsAsync();
        }

        [RelayCommand]
        private async Task LoadStatisticsAsync()
        {
            try
            {
                // Get all rentals
                var rentals = await _rentalService.GetAllRentalsAsync();
                var rentalsList = rentals.ToList();
                
                ActiveRentalsCount = rentalsList.Count(r => r.Status == RentalStatus.Active || r.Status == RentalStatus.Confirmed);
                PendingRentalsCount = rentalsList.Count(r => r.Status == RentalStatus.Pending);
                TotalRevenue = rentalsList.Where(r => r.Status == RentalStatus.Completed).Sum(r => r.TotalPrice);

                // Get available vehicles
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                AvailableVehiclesCount = vehicles.Count(v => v.Status == VehicleStatus.Available);

                // Get total clients
                var clients = await _unitOfWork.Repository<Client>().GetAllAsync();
                TotalClientsCount = clients.Count();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard stats: {ex.Message}");
            }
        }
    }
}

