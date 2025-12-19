using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarRental.Core.Entities;
using CarRental.Core.Enums;
using CarRental.Core.Interfaces;
using CarRental.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace CarRental.BackOffice.ViewModels
{
    public partial class VehicleViewModel : ObservableObject
    {
        private readonly IVehicleService _vehicleService;
        private readonly IImportExportService _importExportService;
        private readonly IUnitOfWork _unitOfWork;

        [ObservableProperty]
        private ObservableCollection<Vehicle> _vehicles = new();

        [ObservableProperty]
        private ObservableCollection<VehicleType> _vehicleTypes = new();

        [ObservableProperty]
        private Vehicle? _selectedVehicle;

        [ObservableProperty]
        private bool _isEditing;

        // Form Fields
        [ObservableProperty]
        private string _brand = string.Empty;

        [ObservableProperty]
        private string _model = string.Empty;

        [ObservableProperty]
        private string _licensePlate = string.Empty;

        [ObservableProperty]
        private int _year = DateTime.Now.Year;

        [ObservableProperty]
        private string _color = string.Empty;

        [ObservableProperty]
        private decimal _dailyPrice;

        [ObservableProperty]
        private VehicleType? _selectedVehicleType;

        [ObservableProperty]
        private VehicleStatus _selectedStatus = VehicleStatus.Available;

        [ObservableProperty]
        private string _imagePath = string.Empty;

        public Array VehicleStatuses => Enum.GetValues(typeof(VehicleStatus));

        public VehicleViewModel(IVehicleService vehicleService, IImportExportService importExportService, IUnitOfWork unitOfWork)
        {
            _vehicleService = vehicleService;
            _importExportService = importExportService;
            _unitOfWork = unitOfWork;
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadVehicleTypesAsync();
            await LoadVehiclesAsync();
        }

        [RelayCommand]
        private async Task LoadVehiclesAsync()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                Vehicles = new ObservableCollection<Vehicle>(vehicles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVehicleTypesAsync()
        {
            try
            {
                var types = await _unitOfWork.Repository<VehicleType>().GetAllAsync();
                VehicleTypes = new ObservableCollection<VehicleType>(types);
                if (VehicleTypes.Any())
                    SelectedVehicleType = VehicleTypes.First();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicle types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedVehicleChanged(Vehicle? value)
        {
            if (value != null)
            {
                IsEditing = true;
                Brand = value.Brand;
                Model = value.Model;
                LicensePlate = value.LicensePlate;
                Year = value.Year;
                Color = value.Color ?? string.Empty;
                DailyPrice = value.DailyPrice;
                SelectedStatus = value.Status;
                SelectedVehicleType = VehicleTypes.FirstOrDefault(vt => vt.Id == value.VehicleTypeId);
            }
            else
            {
                ClearForm();
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            IsEditing = false;
            SelectedVehicle = null;
            Brand = string.Empty;
            Model = string.Empty;
            LicensePlate = string.Empty;
            Year = DateTime.Now.Year;
            Color = string.Empty;
            DailyPrice = 0;
            SelectedStatus = VehicleStatus.Available;
            ImagePath = string.Empty;
            if (VehicleTypes.Any())
                SelectedVehicleType = VehicleTypes.First();
        }

        [RelayCommand]
        private void BrowseImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*",
                Title = "Select Vehicle Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePath = openFileDialog.FileName;
            }
        }

        [RelayCommand]
        private async Task AddVehicleAsync()
        {
            if (!ValidateForm()) return;

            try
            {
                var vehicle = new Vehicle
                {
                    Brand = Brand,
                    Model = Model,
                    LicensePlate = LicensePlate,
                    Year = Year,
                    Color = Color,
                    DailyPrice = DailyPrice,
                    Status = SelectedStatus,
                    VehicleTypeId = SelectedVehicleType!.Id
                };

                await _vehicleService.AddVehicleAsync(vehicle);

                // Add image if selected
                if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
                {
                    var fileName = $"{vehicle.Id}_{Path.GetFileName(ImagePath)}";
                    // Save to FrontOffice wwwroot - use absolute path for reliability
                    // Get the solution directory by going up from the csproj location
                    var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    var projectDir = currentDir;
                    // Navigate up until we find the solution directory containing CarRental.FrontOffice
                    while (!string.IsNullOrEmpty(projectDir) && !Directory.Exists(Path.Combine(projectDir, "CarRental.FrontOffice")))
                    {
                        projectDir = Directory.GetParent(projectDir)?.FullName;
                    }
                    
                    if (string.IsNullOrEmpty(projectDir))
                    {
                        // Fallback to hardcoded path
                        projectDir = @"c:\Users\hp\Documents\projects\dotnet\locationVoiture\CarRental";
                    }
                    
                    var destFolder = Path.Combine(projectDir, "CarRental.FrontOffice", "wwwroot", "VehicleImages");
                    Directory.CreateDirectory(destFolder);
                    var destPath = Path.Combine(destFolder, fileName);
                    File.Copy(ImagePath, destPath, true);

                    // Add image to vehicle
                    var vehicleImage = new VehicleImage
                    {
                        VehicleId = vehicle.Id,
                        ImageUrl = $"/VehicleImages/{fileName}"
                    };
                    await _unitOfWork.Repository<VehicleImage>().AddAsync(vehicleImage);
                    await _unitOfWork.CompleteAsync();
                }

                await LoadVehiclesAsync();
                ClearForm();
                MessageBox.Show("Vehicle added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateVehicleAsync()
        {
            if (SelectedVehicle == null || !ValidateForm()) return;

            try
            {
                SelectedVehicle.Brand = Brand;
                SelectedVehicle.Model = Model;
                SelectedVehicle.LicensePlate = LicensePlate;
                SelectedVehicle.Year = Year;
                SelectedVehicle.Color = Color;
                SelectedVehicle.DailyPrice = DailyPrice;
                SelectedVehicle.Status = SelectedStatus;
                SelectedVehicle.VehicleTypeId = SelectedVehicleType!.Id;

                await _vehicleService.UpdateVehicleAsync(SelectedVehicle);
                await LoadVehiclesAsync();
                ClearForm();
                MessageBox.Show("Vehicle updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedVehicle.Brand} {SelectedVehicle.Model}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _vehicleService.DeleteVehicleAsync(SelectedVehicle.Id);
                await LoadVehiclesAsync();
                ClearForm();
                MessageBox.Show("Vehicle deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ExportCsvAsync()
        {
            try
            {
                var vehicles = await _vehicleService.GetAllVehiclesAsync();
                var csvData = _importExportService.ExportVehiclesToCsv(vehicles);

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"vehicles_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, csvData);
                    MessageBox.Show($"Exported to {saveFileDialog.FileName}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Brand))
            {
                MessageBox.Show("Brand is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Model))
            {
                MessageBox.Show("Model is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(LicensePlate))
            {
                MessageBox.Show("License Plate is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (SelectedVehicleType == null)
            {
                MessageBox.Show("Please select a Vehicle Type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (DailyPrice <= 0)
            {
                MessageBox.Show("Daily Price must be greater than 0.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
