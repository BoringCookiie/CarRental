using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using CarRental.FrontOffice.Models;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CarRental.FrontOffice.Controllers
{
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly IUnitOfWork _unitOfWork; // Needed for VehicleTypes
        private readonly IWebHostEnvironment _environment;

        public VehicleController(IVehicleService vehicleService, IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _vehicleService = vehicleService;
            _unitOfWork = unitOfWork;
            _environment = environment;
        }

        // Public Search with filtering
        public async Task<IActionResult> Index(string? search, int? typeId)
        {
            var allVehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleTypes = await _unitOfWork.Repository<VehicleType>().GetAllAsync();
            
            // Filter by brand (search)
            if (!string.IsNullOrWhiteSpace(search))
            {
                allVehicles = allVehicles.Where(v => 
                    v.Brand.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            
            // Filter by vehicle type
            if (typeId.HasValue && typeId > 0)
            {
                allVehicles = allVehicles.Where(v => v.VehicleTypeId == typeId.Value);
            }
            
            ViewBag.VehicleTypes = vehicleTypes;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentTypeId = typeId;
            
            return View(allVehicles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return View(vehicle);
        }
    }
}
