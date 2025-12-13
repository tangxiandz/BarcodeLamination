namespace BarcodeLaminationModel.Models
{
    public class FeedingRecord
    {
        public int Id { get; set; }
        public string ProductERPCode { get; set; } = string.Empty;
        public string MoldNumber { get; set; } = string.Empty;
        public string FilmCoatingQRCode { get; set; } = string.Empty;
        public int FilmCoatingCount { get; set; } // 覆膜扫描个数
        public int FilmCoatingQuantity { get; set; } // 覆膜扫描累计数量
        public DateTime FeedingTime { get; set; } = DateTime.Now;
        public string Operator { get; set; } = string.Empty;
        public string PDADeviceId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public int? IsClosed { get; set; } // 是否装箱
        public int? BoxId { get; set; } // 装箱编号，UnloadingRecords.ID
    }
}