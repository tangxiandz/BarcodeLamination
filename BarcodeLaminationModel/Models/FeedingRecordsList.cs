namespace BarcodeLaminationModel.Models
{
    public class FeedingRecordsList
    {
        public int Id { get; set; }
        public int FeedingRecordId { get; set; }
        public string FilmCoatingQRCode { get; set; } = string.Empty;
        public string ERPCode { get; set; } = string.Empty; // 从二维码中解析的ERP号码
        public int Quantity { get; set; } // 从二维码中解析的数量
        public string BatchNumber { get; set; } = string.Empty; // 批次（格式：yyyyMMdd）
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        // 导航属性
        public FeedingRecord? FeedingRecord { get; set; }
    }
}