using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using CarRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRental.FrontOffice.Controllers
{
    [Authorize]
    public class PdfController : Controller
    {
        private readonly IPdfService _pdfService;
        private readonly IUnitOfWork _unitOfWork;

        public PdfController(IPdfService pdfService, IUnitOfWork unitOfWork)
        {
            _pdfService = pdfService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadContract(int rentalId)
        {
            // Get the rental with related entities
            var rentals = await _unitOfWork.Repository<Rental>()
                .FindIncludingAsync(r => r.Id == rentalId, r => r.Vehicle, r => r.Client);
            
            var rental = rentals.FirstOrDefault();

            if (rental == null)
            {
                return NotFound("Rental not found");
            }

            // Security check: ensure the logged-in user owns this rental
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (rental.ClientId != userId)
            {
                return Forbid();
            }

            // Generate PDF
            var pdfBytes = _pdfService.GenerateContract(rental);

            // Return as file download
            var fileName = $"CarRental_Contract_{rental.Id}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
