namespace BarcodeLaminationModel.Models
{
    public class FilmCoatingRecord
    {
        public int Id { get; set; }
        public string OriginalERPCode { get; set; } = string.Empty;
        public string NewERPCode { get; set; } = string.Empty;
        public string ProductERPCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public DateTime PrintTime { get; set; } = DateTime.Now;
        public string PrintedBy { get; set; } = string.Empty;
        public string PDADeviceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}