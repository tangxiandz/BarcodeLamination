using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BarcodeLaminationWeb.Services;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationWeb.Pages
{
    public class RecordsModel : PageModel
    {
        private readonly IApiService _apiService;

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RecordType { get; set; } = "filmcoating";

        [BindProperty(SupportsGet = true)]
        public string? ProductERPCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? BatchNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public PagedResponse<FilmCoatingRecord> FilmCoatingRecords { get; set; } = new PagedResponse<FilmCoatingRecord>();
        public PagedResponse<FeedingRecord> FeedingRecords { get; set; } = new PagedResponse<FeedingRecord>();
        public PagedResponse<UnloadingRecord> UnloadingRecords { get; set; } = new PagedResponse<UnloadingRecord>();

        public RecordsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task OnGetAsync()
        {
            switch (RecordType.ToLower())
            {
                case "filmcoating":
                    FilmCoatingRecords = await _apiService.GetFilmCoatingRecordsAsync(
                        StartDate, EndDate, ProductERPCode, BatchNumber, CurrentPage, PageSize);
                    break;
                case "feeding":
                    FeedingRecords = await _apiService.GetFeedingRecordsAsync(
                        StartDate, EndDate, ProductERPCode, CurrentPage, PageSize);
                    break;
                case "unloading":
                    UnloadingRecords = await _apiService.GetUnloadingRecordsAsync(
                        StartDate, EndDate, ProductERPCode, BatchNumber, CurrentPage, PageSize);
                    break;
            }
        }
    }
}