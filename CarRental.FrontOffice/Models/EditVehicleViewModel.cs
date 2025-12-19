using System.ComponentModel.DataAnnotations;

namespace CarRental.FrontOffice.Models
{
    public class EditVehicleViewModel
    {
        public int Id { get; set; }
        [Required] public string Brand { get; set; }
        [Required] public string Model { get; set; }
        [Required] public string LicensePlate { get; set; }
        public int Year { get; set; }
        public string? Color { get; set; }
        public decimal DailyPrice { get; set; }
        public string? Features { get; set; }
        
        // Handling image deletion/addition is complex. 
        // For this iteration, we allow adding new images only or deleting existing via separate action?
        // Let's keep it simple: Add new images.
        public List<IFormFile>? NewImages { get; set; }
    }
}
