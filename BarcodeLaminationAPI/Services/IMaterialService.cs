using BarcodeLaminationModel.Models;

namespace BarcodeLaminationAPI.Services
{
    public interface IMaterialService
    {
        Task<PagedResponse<Material>> GetMaterialsAsync(int pageNumber = 1, int pageSize = 20, string searchTerm = "");
        Task<List<Material>> GetMaterialsAsync();
        Task<Material> GetMaterialByIdAsync(int id);
        Task<Material> GetMaterialByProductERPCodeAsync(string productERPCode);
        Task<Material> GetMaterialByProductERPCodeAsync(string productERPCode,string FabricERPCode);
        Task<Material> GetMaterialByFabricERPCodeAsync(string FabricERPCode);
        Task<Material> GetMaterialByFabricERPCodeAsync(string FabricERPCode,string MoldNumber);
        Task<Material> GetMaterialByMoldNumberAsync(string moldNumber);
        Task<Material> CreateMaterialAsync(MaterialRequest request);
        Task<Material> UpdateMaterialAsync(int id, MaterialRequest request);
        Task<bool> DeleteMaterialAsync(int id);
    }

    public interface IFilmCoatingService
    {
        Task<FilmCoatingResponse> ProcessFilmCoatingAsync(FilmCoatingRequest request);
        Task<PagedResponse<FilmCoatingRecord>> GetFilmCoatingRecordsAsync(QueryRequest request);
    }

    public interface IFeedingService
    {
        Task<FeedingResponse> ProcessFeedingAsync(FeedingRequest request);
        Task<PagedResponse<FeedingRecord>> GetFeedingRecordsAsync(QueryRequest request);
        Task<List<FeedingRecord>> GetValidFeedingRecordsAsync();
        Task<List<FeedingRecordsList>> GetFeedingRecordDetailsAsync(int feedingRecordId); // 新增
    }

    public interface IUnloadingService
    {
        Task<UnloadingResponse> ProcessUnloadingAsync(UnloadingRequest request);
        Task<UnloadingResponse> ProcessClosedAsync(int FeedingRecordId);
        Task<PagedResponse<UnloadingRecord>> GetUnloadingRecordsAsync(QueryRequest request);
    }
}
