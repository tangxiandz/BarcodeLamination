using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class PrintResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PrinterName { get; set; } = string.Empty;
        public int Copies { get; set; }
        public int PrintedCopies { get; set; }
        public DateTime PrintTime { get; set; }
    }
}  
