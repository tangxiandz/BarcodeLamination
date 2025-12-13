

using BarcodeLaminationModel.Models.Print;

namespace BarcodeLaminationModel.Models
{
    public class FilmCoatingRequest
    {
        public string OriginalQRCode { get; set; } = string.Empty;
        public string ErpCode { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string PDADeviceId { get; set; } = string.Empty;
        public string? PrintedBy { get; set; }
    }

    public class FilmCoatingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NewQRCode { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public FilmCoatingRecord? Record { get; set; }
        public PrintResult? PrintResult { get; set; }
    }
}
