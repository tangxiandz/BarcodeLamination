using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class LabelPrintData
    {
        public string PrinterName { get; set; } = string.Empty;
        public int Copies { get; set; } = 1;
        public string LabelType { get; set; } = "Default";
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }
}
