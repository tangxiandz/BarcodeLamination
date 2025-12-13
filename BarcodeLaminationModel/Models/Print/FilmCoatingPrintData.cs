using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class FilmCoatingPrintData
    {
        public string ErpCode { get; set; } = string.Empty;
        public string ProductPartDescription { get; set; } = string.Empty;
        public int? OriginalQuantity { get; set; }
        public int? NewQuantity { get; set; }
        public string OriginalBatch { get; set; } = string.Empty;
        public string NewBatch { get; set; } = string.Empty;
        public string NewQRCode { get; set; } = string.Empty;
        public string PrintedBy { get; set; } = string.Empty;
        public string PrinterName { get; set; } = string.Empty;
        public int Copies { get; set; } = 1;
        public bool GenerateQRCode { get; set; } = true;
    }
}
