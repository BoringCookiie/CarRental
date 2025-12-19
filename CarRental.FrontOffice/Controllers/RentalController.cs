using CarRental.Core.Entities;
using CarRental.FrontOffice.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.FrontOffice.Controllers
{
    [Authorize]
    public class RentalController : Controller
    {
        private readonly IRentalService _rentalService;
        private readonly IVehicleService _vehicleService;
        private readonly IPdfService _pdfService;
        
        public RentalController(IRentalService rentalService, IVehicleService vehicleService, IPdfService pdfService)
        {
            _rentalService = rentalService;
            _vehicleService = vehicleService;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> MyRentals()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var rentals = await _rentalService.GetRentalsByClientIdAsync(userId);
            return View(rentals);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? vehicleId)
        {
            var model = new CreateRentalViewModel
            {
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2)
            };

            if (vehicleId.HasValue)
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(vehicleId.Value);
                if (vehicle != null)
                {
                    model.VehicleId = vehicle.Id;
                    model.VehicleName = $"{vehicle.Brand} {vehicle.Model}";
                    model.DailyPrice = vehicle.DailyPrice;
                }
            }

            // Pass available vehicles for dropdown
            var vehicles = await _vehicleService.GetAvailableVehiclesAsync(model.StartDate, model.EndDate);
            ViewBag.Vehicles = vehicles;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRentalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync(model.StartDate, model.EndDate);
                ViewBag.Vehicles = vehicles;
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var rental = await _rentalService.CreateRentalAsync(userId, model.VehicleId, model.StartDate, model.EndDate);
                
                TempData["Success"] = "Rental request submitted successfully! Check your email for confirmation.";
                return RedirectToAction(nameof(MyRentals));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var vehicles = await _vehicleService.GetAvailableVehiclesAsync(model.StartDate, model.EndDate);
                ViewBag.Vehicles = vehicles;
                return View(model);
            }
        }
    }
}

