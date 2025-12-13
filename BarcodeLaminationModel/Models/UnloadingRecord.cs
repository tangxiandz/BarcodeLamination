namespace BarcodeLaminationModel.Models
{
    public class UnloadingRecord
    {
        public int Id { get; set; }
        public string ProductERPCode { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public string ProductPartDescription2 { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int PrintStatus { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime PrintTime { get; set; } = DateTime.Now;
        public string PrintedBy { get; set; } = string.Empty;
        public string PCDeviceId { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}