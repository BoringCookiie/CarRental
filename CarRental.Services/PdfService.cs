using CarRental.Core.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRental.Services
{
    public interface IPdfService
    {
        byte[] GenerateContract(Rental rental);
    }

    public class PdfService : IPdfService
    {
        private readonly IQrCodeService _qrCodeService;

        public PdfService(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateContract(Rental rental)
        {
            var qrContent = $"Rental:{rental.Id}|Vehicle:{rental.VehicleId}|Client:{rental.ClientId}|Total:{rental.TotalPrice}";
            var qrImage = _qrCodeService.GenerateQrCode(qrContent);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Row(row => 
                        {
                            row.RelativeItem().Column(col => 
                            {
                                col.Item().Text($"Rental Contract #{rental.Id}").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Date: {DateTime.Now:g}").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(100).Image(qrImage);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(x =>
                        {
                            x.Item().Text("Rental Agreement").Bold().FontSize(16);
                            x.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            x.Item().Text($"Vehicle ID: {rental.VehicleId}");
                            x.Item().Text($"Client ID: {rental.ClientId}");
                            x.Item().Text($"Period: {rental.StartDate:d} - {rental.EndDate:d}");
                            x.Item().Text($"Total Price: {rental.TotalPrice:C}").SemiBold();
                            
                            x.Item().PaddingTop(20).Text("Terms and Conditions").Bold();
                            x.Item().Text("1. The renter agrees to return the vehicle in the same condition.");
                            x.Item().Text("2. Late returns will incur additional charges.");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf();
        }
    }
}
