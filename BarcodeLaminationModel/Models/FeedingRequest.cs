
namespace BarcodeLaminationModel.Models
{
    public class FeedingRequest
    {
        public string ProductERPCode { get; set; } = string.Empty;
        public string MoldQRCode { get; set; } = string.Empty;
        public List<string> FilmCoatingQRCodes { get; set; } = new(); // 覆膜二维码列表
        public string PDADeviceId { get; set; } = string.Empty;
        public string? Operator { get; set; }
    }

    public class FeedingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsAllValid { get; set; }
        public FeedingRecord? Record { get; set; }
        public ValidationResult MoldValidation { get; set; } = new();
        public List<FilmCoatingValidationResult> FilmCoatingValidations { get; set; } = new();
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class FilmCoatingValidationResult
    {
        public int Index { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string ERPCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class FabricValidationResult
    {
        public int Index { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
