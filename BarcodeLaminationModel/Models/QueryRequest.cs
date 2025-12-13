namespace BarcodeLaminationModel.Models
{
    public class QueryRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProductERPCode { get; set; }
        public string? BatchNumber { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
