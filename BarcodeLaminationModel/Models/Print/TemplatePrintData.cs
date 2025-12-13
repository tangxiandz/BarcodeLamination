using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class TemplatePrintData : LabelPrintData  // 继承 LabelPrintData
    {
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateVariables { get; set; } = new Dictionary<string, string>();
    }
}
