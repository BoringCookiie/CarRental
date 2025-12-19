using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CarRental.Services
{
    public interface IQrCodeService
    {
        byte[] GenerateQrCode(string content);
    }

    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCode(string content)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
