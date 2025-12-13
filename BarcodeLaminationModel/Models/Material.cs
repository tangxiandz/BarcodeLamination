namespace BarcodeLaminationModel.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string ProductERPCode { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public string MoldNumber { get; set; } = string.Empty;
        public int PackingQuantity { get; set; } = 1;
        public int FabricRollCount { get; set; } = 3;
        public string FabricERPCode { get; set; } = string.Empty;
        public string FabricPartDescription { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class MaterialRequest
    {
        public string ProductERPCode { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public string MoldNumber { get; set; } = string.Empty;
        public int PackingQuantity { get; set; } = 1;
        public int FabricRollCount { get; set; } = 3;
        public string FabricERPCode { get; set; } = string.Empty;
        public string FabricPartDescription { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}