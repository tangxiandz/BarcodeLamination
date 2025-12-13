using System.Text;
using System.Text.Json;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationWeb.Services
{
    public interface IApiService
    {
        // 物料管理方法
        Task<PagedResponse<Material>> GetMaterialsAsync(int pageNumber = 1, int pageSize = 20, string searchTerm = "");
        Task<bool> AddMaterialAsync(MaterialRequest material);
        Task<bool> UpdateMaterialAsync(int id, MaterialRequest material);
        Task<bool> DeleteMaterialAsync(int id);
        Task<Material?> GetMaterialByIdAsync(int id);

        // 记录查询方法
        Task<PagedResponse<FilmCoatingRecord>> GetFilmCoatingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, string? batchNumber = null, int pageNumber = 1, int pageSize = 20);
        Task<PagedResponse<FeedingRecord>> GetFeedingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, int pageNumber = 1, int pageSize = 20);
        Task<List<FeedingRecord>> GetValidFeedingRecordsAsync();
        Task<PagedResponse<UnloadingRecord>> GetUnloadingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, string? batchNumber = null, int pageNumber = 1, int pageSize = 20);

        // 下料处理方法
        Task<UnloadingResponse?> ProcessUnloadingAsync(UnloadingRequest request);
        // 下料完成方法
        Task<UnloadingResponse?> ProcessClosedAsync(int feedingRecordId);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // 物料管理方法实现
        public async Task<PagedResponse<Material>> GetMaterialsAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = "")
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}",
                    $"searchTerm={searchTerm}"
                };


                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"materials{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PagedResponse<Material>>(content, _jsonOptions)
                        ?? new PagedResponse<Material>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取物料列表失败: {ex.Message}");
            }
            return new PagedResponse<Material>();
        }

        public async Task<bool> AddMaterialAsync(MaterialRequest material)
        {
            try
            {
                var json = JsonSerializer.Serialize(material, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("materials", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加物料失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateMaterialAsync(int id, MaterialRequest material)
        {
            try
            {
                var json = JsonSerializer.Serialize(material, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"materials/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新物料失败: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"materials/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除物料失败: {ex.Message}");
                return false;
            }
        }

        public async Task<Material?> GetMaterialByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"materials/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Material>(content, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取物料详情失败: {ex.Message}");
            }
            return null;
        }

        // 覆膜记录查询
        public async Task<PagedResponse<FilmCoatingRecord>> GetFilmCoatingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, string? batchNumber = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(productERPCode))
                    queryParams.Add($"productERPCode={Uri.EscapeDataString(productERPCode)}");
                if (!string.IsNullOrEmpty(batchNumber))
                    queryParams.Add($"batchNumber={Uri.EscapeDataString(batchNumber)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"filmcoating/records{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PagedResponse<FilmCoatingRecord>>(content, _jsonOptions)
                        ?? new PagedResponse<FilmCoatingRecord>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取覆膜记录失败: {ex.Message}");
            }
            return new PagedResponse<FilmCoatingRecord>();
        }

        // 上料记录查询
        public async Task<PagedResponse<FeedingRecord>> GetFeedingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(productERPCode))
                    queryParams.Add($"productERPCode={Uri.EscapeDataString(productERPCode)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"feeding/records{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PagedResponse<FeedingRecord>>(content, _jsonOptions)
                        ?? new PagedResponse<FeedingRecord>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取上料记录失败: {ex.Message}");
            }
            return new PagedResponse<FeedingRecord>();
        }

        // 有效上料记录
        public async Task<List<FeedingRecord>> GetValidFeedingRecordsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("feeding/valid-records");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<FeedingRecord>>(content, _jsonOptions)
                        ?? new List<FeedingRecord>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取有效上料记录失败: {ex.Message}");
            }
            return new List<FeedingRecord>();
        }

        // 下料记录查询
        public async Task<PagedResponse<UnloadingRecord>> GetUnloadingRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, string? productERPCode = null, string? batchNumber = null, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (startDate.HasValue)
                    queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
                if (endDate.HasValue)
                    queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(productERPCode))
                    queryParams.Add($"productERPCode={Uri.EscapeDataString(productERPCode)}");
                if (!string.IsNullOrEmpty(batchNumber))
                    queryParams.Add($"batchNumber={Uri.EscapeDataString(batchNumber)}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"unloading/records{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PagedResponse<UnloadingRecord>>(content, _jsonOptions)
                        ?? new PagedResponse<UnloadingRecord>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取下料记录失败: {ex.Message}");
            }
            return new PagedResponse<UnloadingRecord>();
        }

        // 下料处理
        public async Task<UnloadingResponse?> ProcessUnloadingAsync(UnloadingRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("unloading/process", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UnloadingResponse>(responseContent, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下料处理失败: {ex.Message}");
            }
            return null;
        }

        // 新增下料完成方法实现
        public async Task<UnloadingResponse?> ProcessClosedAsync(int feedingRecordId)
        {
            try
            {
                // 构建请求体
                var requestBody = new { FeedingRecordId = feedingRecordId };
                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 调用下料完成接口
                var response = await _httpClient.PostAsync("unloading/complete", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UnloadingResponse>(responseContent, _jsonOptions);
                }
                else
                {
                    // 记录错误信息
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"下料完成失败: {response.StatusCode}, {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"下料完成处理失败: {ex.Message}");
            }
            return null;
        }
    }

    // 下料响应类
    public class UnloadingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UnloadingRecord? Record { get; set; }
        public string NewQRCode { get; set; } = string.Empty;
    }
}