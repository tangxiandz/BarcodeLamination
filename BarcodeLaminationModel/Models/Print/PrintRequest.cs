using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class PrintRequest
    {
        public string PrinterName { get; set; } = string.Empty;
        public string LabelType { get; set; } = "Unloading";
        public int Copies { get; set; } = 1;
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
