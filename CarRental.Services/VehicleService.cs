using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using CarRental.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime start, DateTime end);
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task AddVehicleAsync(Vehicle vehicle);
        Task UpdateVehicleAsync(Vehicle vehicle);
        Task DeleteVehicleAsync(int id);
        Task<IEnumerable<Vehicle>> GetVehiclesByOwnerId(int clientId);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
        {
            return await _unitOfWork.Repository<Vehicle>().GetAllIncludingAsync(v => v.Images);
        }
        
        // Revised Repo usage for Include support (Quick fix: we will need to change IRepository to expose IQueryable or specific methods)
        // Let's rely on specific queries in this service by accessing context? 
        // No, that breaks pattern. 
        // Let's assume for now we use the basic repo and maybe loading is separate or we add 'Include' param to GetAll.
        
        // Let's keep it simple: generic repo returns IEnumerable, so no Includes unless we change it.
        // I'll add 'Include' support to repository interface for better performance.
        
        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime start, DateTime end)
        {
             // Get all vehicles that do NOT have an active rental overlapping with start/end
             // This logic requires checking Rental table.
             // We can fetch all vehicles and filter in memory (not efficient but easy) or query.
             var vehicles = await _unitOfWork.Repository<Vehicle>().GetAllAsync();
             var rentals = await _unitOfWork.Repository<Rental>().GetAllAsync(); // This is bad for perf, but okay for prototype
             
             var availableVehicles = new List<Vehicle>();
             foreach(var v in vehicles)
             {
                 bool isBooked = rentals.Any(r => 
                     r.VehicleId == v.Id && 
                     r.Status != RentalStatus.Cancelled &&
                     (
                        (start >= r.StartDate && start < r.EndDate) ||
                        (end > r.StartDate && end <= r.EndDate) ||
                        (start <= r.StartDate && end >= r.EndDate)
                     )
                 );
                 
                 if(!isBooked && v.Status == VehicleStatus.Available)
                 {
                     availableVehicles.Add(v);
                 }
             }
             return availableVehicles;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Vehicle>().GetByIdAsync(id);
        }

        public async Task AddVehicleAsync(Vehicle vehicle)
        {
            await _unitOfWork.Repository<Vehicle>().AddAsync(vehicle);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            _unitOfWork.Repository<Vehicle>().Update(vehicle);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteVehicleAsync(int id)
        {
            var v = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(id);
            if (v != null)
            {
                _unitOfWork.Repository<Vehicle>().Remove(v);
                await _unitOfWork.CompleteAsync();
            }
        }
        
        public async Task<IEnumerable<Vehicle>> GetVehiclesByOwnerId(int clientId)
        {
            return await _unitOfWork.Repository<Vehicle>()
                .FindIncludingAsync(v => v.ClientId == clientId, v => v.Images);
        }
    }
}
