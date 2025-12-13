using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class PrintTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TemplateType { get; set; } = "TEXT"; // TEXT, BTW, XML
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 150;
    }
}
