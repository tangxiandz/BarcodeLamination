using BarcodeLaminationModel.Models.Print;

namespace BarcodeLaminationAPI.Services
{
    public interface IWindowsPrintService
    {
        Task<PrintResult> PrintLabelAsync(LabelPrintData printData);
        Task<List<PrinterInfo>> GetAvailablePrintersAsync();
    }
}
