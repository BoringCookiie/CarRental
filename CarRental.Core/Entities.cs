using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Core.Enums
{
    public enum UserRole { Admin, Employee }
    public enum VehicleStatus { Available, Rented, Maintenance, Reserved }
    public enum RentalStatus { Pending, Confirmed, Active, Completed, Cancelled }
    public enum PaymentStatus { Pending, Paid, Refunded }
    public enum MaintenanceType { Routine, Repair, Inspection }
}

namespace CarRental.Core.Entities
{
    using CarRental.Core.Enums;

    public class User
    {
        public int Id { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Client
    {
        public int Id { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? DriverLicenseNumber { get; set; }
        public string? Address { get; set; }
        public string? PasswordHash { get; set; } // For FrontOffice login
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>(); // Vehicles owned/posted by the client
    }

    public class VehicleType
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } // SUV, Sedan, etc.
        public string? Description { get; set; }
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }

    public class Vehicle
    {
        public int Id { get; set; }
        [Required] public string Brand { get; set; }
        [Required] public string Model { get; set; }
        [Required] public string LicensePlate { get; set; }
        public int Year { get; set; }
        public string? Color { get; set; }
        public string? Features { get; set; } // JSON or CSV string
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public decimal DailyPrice { get; set; }
        
        public int VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; }

        public int? ClientId { get; set; } // Owner of the vehicle (if posted by client)
        public Client? Client { get; set; }

        public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();
    }

    public class VehicleImage
    {
        public int Id { get; set; }
        [Required] public string ImageUrl { get; set; } // Or Path
        
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }

    public class Rental
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Pending;
        public string? ContractPdfPath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Method { get; set; } // Card, Cash, etc.

        public int RentalId { get; set; }
        public Rental Rental { get; set; }
    }

    public class Maintenance
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public MaintenanceType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Cost { get; set; }
        public bool IsCompleted { get; set; }

        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
    }

    public class Log
    {
        public int Id { get; set; }
        public string Action { get; set; } // "Login", "CreateRental"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Username { get; set; } // Optional user identifier
    }
}
