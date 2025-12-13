using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class PrinterInfo
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsValid { get; set; }
        public bool CanDuplex { get; set; }
        public bool IsPlotter { get; set; }
        public string Status { get; set; } = "Unknown";
    }
}
