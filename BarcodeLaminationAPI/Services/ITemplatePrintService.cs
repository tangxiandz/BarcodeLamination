using BarcodeLaminationModel.Models.Print;

namespace BarcodeLaminationAPI.Services
{
    public interface ITemplatePrintService
    {
        Task<PrintResult> PrintWithTemplateAsync(TemplatePrintData printData);
        Task<string> SaveTemplateAsync(PrintTemplate template); 
        Task<string> SaveTemplateAsync(PrintTemplate template, IFormFile templateFile);
        Task<bool> DeleteTemplateAsync(string templateName);
        Task<List<PrintTemplate>> GetTemplatesAsync();
        Task<PrintTemplate?> GetTemplateAsync(string templateName);
        Task<string> PreviewTemplateAsync(TemplatePrintData printData);
        Task<bool> TemplateExistsAsync(string templateName);
    }
}
