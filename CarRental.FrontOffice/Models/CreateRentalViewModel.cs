using System.ComponentModel.DataAnnotations;

namespace CarRental.FrontOffice.Models
{
    public class CreateRentalViewModel
    {
        [Required(ErrorMessage = "Please select a vehicle")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(2);

        // For displaying in the form
        public string? VehicleName { get; set; }
        public decimal? DailyPrice { get; set; }
    }
}
