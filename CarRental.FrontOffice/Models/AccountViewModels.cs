using System.ComponentModel.DataAnnotations;

namespace CarRental.FrontOffice.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    public class EditProfileViewModel
    {
        public int Id { get; set; }
        
        [Required, Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required, Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required, EmailAddress]
        public string Email { get; set; }
        
        [Phone, Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Driver License")]
        public string? DriverLicenseNumber { get; set; }
        
        public string? Address { get; set; }
    }
}
