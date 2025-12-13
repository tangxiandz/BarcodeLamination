using BarcodeLaminationModel.Models.Print;

namespace BarcodeLaminationAPI.Services
{
    public interface IBarTenderPrintService
    {
        Task<PrintResult> PrintFilmCoatingLabel(FilmCoatingPrintData printData);
        Task TestConnection();
    }
}
