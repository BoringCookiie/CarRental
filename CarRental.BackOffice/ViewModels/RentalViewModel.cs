using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CarRental.Core.Entities;
using CarRental.Core.Enums;
using CarRental.Core.Interfaces;
using CarRental.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CarRental.BackOffice.ViewModels
{
    public partial class RentalViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;
        private readonly IUnitOfWork _unitOfWork;

        [ObservableProperty]
        private ObservableCollection<Rental> _rentals = new();

        [ObservableProperty]
        private Rental? _selectedRental;

        [ObservableProperty]
        private RentalStatus _selectedStatus;

        public Array RentalStatuses => Enum.GetValues(typeof(RentalStatus));

        public RentalViewModel(IRentalService rentalService, IUnitOfWork unitOfWork)
        {
            _rentalService = rentalService;
            _unitOfWork = unitOfWork;
            _ = LoadRentalsAsync();
        }

        [RelayCommand]
        private async Task LoadRentalsAsync()
        {
            try
            {
                var rentals = await _rentalService.GetAllRentalsAsync();
                Rentals = new ObservableCollection<Rental>(rentals);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rentals: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        partial void OnSelectedRentalChanged(Rental? value)
        {
            if (value != null)
            {
                SelectedStatus = value.Status;
            }
        }

        [RelayCommand]
        private async Task UpdateStatusAsync()
        {
            if (SelectedRental == null) return;

            try
            {
                SelectedRental.Status = SelectedStatus;
                _unitOfWork.Repository<Rental>().Update(SelectedRental);
                await _unitOfWork.CompleteAsync();
                await LoadRentalsAsync();
                MessageBox.Show("Status updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ConfirmRentalAsync()
        {
            if (SelectedRental == null) return;
            SelectedStatus = RentalStatus.Confirmed;
            await UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task CompleteRentalAsync()
        {
            if (SelectedRental == null) return;
            SelectedStatus = RentalStatus.Completed;
            await UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task CancelRentalAsync()
        {
            if (SelectedRental == null) return;

            var result = MessageBox.Show("Are you sure you want to cancel this rental?",
                "Confirm Cancel", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            SelectedStatus = RentalStatus.Cancelled;
            await UpdateStatusAsync();
        }
    }
}
