using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using CarRental.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public interface IRentalService
    {
        Task<Rental> CreateRentalAsync(int clientId, int vehicleId, DateTime start, DateTime end);
        Task<IEnumerable<Rental>> GetRentalsByClientIdAsync(int clientId);
        Task<IEnumerable<Rental>> GetAllRentalsAsync();
    }

    public class RentalService : IRentalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVehicleService _vehicleService;
        private readonly INotificationService _notificationService;

        public RentalService(IUnitOfWork unitOfWork, IVehicleService vehicleService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _vehicleService = vehicleService;
            _notificationService = notificationService;
        }

        public async Task<Rental> CreateRentalAsync(int clientId, int vehicleId, DateTime start, DateTime end)
        {
            // validate dates
            if (start >= end) throw new Exception("Invalid date range");

            // check availability
            var available = await _vehicleService.GetAvailableVehiclesAsync(start, end);
            if (!available.Any(v => v.Id == vehicleId))
            {
                throw new Exception("Vehicle not available for selected dates");
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null) throw new Exception("Vehicle not found");

            var days = (end - start).Days;
            if (days == 0) days = 1;

            var rental = new Rental
            {
                ClientId = clientId,
                VehicleId = vehicleId,
                StartDate = start,
                EndDate = end,
                TotalPrice = days * vehicle.DailyPrice,
                Status = RentalStatus.Pending,
                ContractPdfPath = string.Empty, // Will be set when contract is generated
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Rental>().AddAsync(rental);
            
            // Update vehicle status to Reserved so other users can't book it
            vehicle.Status = VehicleStatus.Reserved;
            _unitOfWork.Repository<Vehicle>().Update(vehicle);
            
            await _unitOfWork.CompleteAsync();

            // Send Email Notification
            // In real app, fetch Client email via ClientId. For now, assuming current user (Frontend passes email) or fetching.
            // Let's fetch client.
            var client = await _unitOfWork.Repository<Client>().GetByIdAsync(clientId);
            if (client != null)
            {
                var subject = "Booking Confirmation - Car Rental";
                var body = $"<h1>Booking Confirmed</h1><p>Thank you {client.FirstName}, your booking for {vehicle.Brand} {vehicle.Model} from {start:d} to {end:d} is pending approval.</p>";
                await _notificationService.SendEmailAsync(client.Email, subject, body);
            }

            return rental;
        }

        public async Task<IEnumerable<Rental>> GetRentalsByClientIdAsync(int clientId)
        {
            // Use specific query with Includes
            return await _unitOfWork.Repository<Rental>().FindIncludingAsync(r => r.ClientId == clientId, r => r.Vehicle);
        }

        public async Task<IEnumerable<Rental>> GetAllRentalsAsync()
        {
             return await _unitOfWork.Repository<Rental>().GetAllIncludingAsync(r => r.Vehicle, r => r.Client);
        }
    }
}
