
namespace BarcodeLaminationModel.Models
{
    public class UnloadingRequest
    {
        public List<int> FeedingRecordIds { get; set; } = new (); // 改为复数形式
        public string PCDeviceId { get; set; } = string.Empty;
        public string? PrintedBy { get; set; }
    }

    public class UnloadingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UnloadingRecord? Record { get; set; }
        public string NewQRCode { get; set; } = string.Empty;
    }
}
