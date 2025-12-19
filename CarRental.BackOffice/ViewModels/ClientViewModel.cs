using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace CarRental.BackOffice.ViewModels
{
    public partial class ClientViewModel : ObservableObject
    {
        private readonly IUnitOfWork _unitOfWork;

        [ObservableProperty]
        private ObservableCollection<Client> _clients = new();

        [ObservableProperty]
        private Client? _selectedClient;

        [ObservableProperty]
        private bool _isEditing;

        // Form Fields
        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _driverLicenseNumber = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        public ClientViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ = LoadClientsAsync();
        }

        [RelayCommand]
        private async Task LoadClientsAsync()
        {
            try
            {
                var clients = await _unitOfWork.Repository<Client>().GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedClientChanged(Client? value)
        {
            if (value != null)
            {
                IsEditing = true;
                FirstName = value.FirstName;
                LastName = value.LastName;
                Email = value.Email;
                PhoneNumber = value.PhoneNumber ?? string.Empty;
                DriverLicenseNumber = value.DriverLicenseNumber ?? string.Empty;
                Address = value.Address ?? string.Empty;
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
            SelectedClient = null;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            DriverLicenseNumber = string.Empty;
            Address = string.Empty;
        }

        [RelayCommand]
        private async Task AddClientAsync()
        {
            if (!ValidateForm()) return;

            try
            {
                // Check if email already exists
                var existing = await _unitOfWork.Repository<Client>().FindAsync(c => c.Email == Email);
                if (existing.Any())
                {
                    MessageBox.Show("A client with this email already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var client = new Client
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber,
                    DriverLicenseNumber = string.IsNullOrWhiteSpace(DriverLicenseNumber) ? null : DriverLicenseNumber,
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Client>().AddAsync(client);
                await _unitOfWork.CompleteAsync();
                await LoadClientsAsync();
                ClearForm();
                MessageBox.Show("Client added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateClientAsync()
        {
            if (SelectedClient == null || !ValidateForm()) return;

            try
            {
                SelectedClient.FirstName = FirstName;
                SelectedClient.LastName = LastName;
                SelectedClient.Email = Email;
                SelectedClient.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber;
                SelectedClient.DriverLicenseNumber = string.IsNullOrWhiteSpace(DriverLicenseNumber) ? null : DriverLicenseNumber;
                SelectedClient.Address = string.IsNullOrWhiteSpace(Address) ? null : Address;

                _unitOfWork.Repository<Client>().Update(SelectedClient);
                await _unitOfWork.CompleteAsync();
                await LoadClientsAsync();
                ClearForm();
                MessageBox.Show("Client updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedClient.FirstName} {SelectedClient.LastName}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _unitOfWork.Repository<Client>().Remove(SelectedClient);
                await _unitOfWork.CompleteAsync();
                await LoadClientsAsync();
                ClearForm();
                MessageBox.Show("Client deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting client: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                MessageBox.Show("First Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(LastName))
            {
                MessageBox.Show("Last Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
