using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationModel.Models.Print
{
    public class TemplateUploadRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? TemplateFile { get; set; }
        public string Content { get; set; } = string.Empty;
        public string TemplateType { get; set; } = "TEXT";
    }
}
