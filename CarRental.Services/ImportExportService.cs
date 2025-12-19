using CarRental.Core.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace CarRental.Services
{
    public interface IImportExportService
    {
        byte[] ExportVehiclesToCsv(IEnumerable<Vehicle> vehicles);
    }

    public class ImportExportService : IImportExportService
    {
        public byte[] ExportVehiclesToCsv(IEnumerable<Vehicle> vehicles)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Map specific columns if needed, or auto-map
                csv.WriteRecords(vehicles.Select(v => new 
                {
                    v.Id,
                    v.Brand,
                    v.Model,
                    v.LicensePlate,
                    v.Year,
                    v.DailyPrice,
                    v.Status,
                    TypeName = v.VehicleType?.Name ?? "N/A"
                }));
                writer.Flush();
                return memoryStream.ToArray();
            }
        }
    }
}
