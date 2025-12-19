using CarRental.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CarRental.FrontOffice.Models
{
    public class CreateVehicleViewModel
    {
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public string LicensePlate { get; set; }
        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }
        public string Color { get; set; }
        [Required]
        [Range(0, 10000)]
        public decimal DailyPrice { get; set; }
        
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }
        
        [Display(Name = "Features (comma separated)")]
        public string Features { get; set; }
        
        [Display(Name = "Upload Images")]
        public List<IFormFile> Images { get; set; }
    }
}
