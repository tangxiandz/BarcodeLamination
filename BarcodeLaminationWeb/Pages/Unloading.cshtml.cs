using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BarcodeLaminationWeb.Services;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationWeb.Pages
{
    public class UnloadingModel : PageModel
    {
        private readonly IApiService _apiService;

        [BindProperty]
        public List<int> SelectedFeedingRecordIds { get; set; } = new List<int>();

        [BindProperty]
        public int CompleteFeedingRecordId { get; set; }

        public List<FeedingRecord> ValidFeedingRecords { get; set; } = new List<FeedingRecord>();
        public List<UnloadingRecord> LastUnloadingRecords { get; set; } = new List<UnloadingRecord>();
        public string Message { get; set; } = string.Empty;
        public bool ShowSuccess { get; set; }

        // 物料列表和选中物料
        public List<Material> Materials { get; set; } = new List<Material>();
        [BindProperty(SupportsGet = true)]
        public string SelectedMaterialCode { get; set; } = string.Empty;

        // 存储物料编码到装箱数量的映射
        public Dictionary<string, int> MaterialPackingQuantities { get; set; } = new Dictionary<string, int>();

        public UnloadingModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task OnGetAsync()
        {
            await LoadValidFeedingRecords();
            await LoadMaterials();
        }

        // 下料打印处理方法
        public async Task<IActionResult> OnPostAsync()
        {
            await LoadValidFeedingRecords();

            if (SelectedFeedingRecordIds == null || SelectedFeedingRecordIds.Count == 0)
            {
                Message = "请至少选择一条上料记录";
                ShowSuccess = false;
                return Page();
            }

            // 只能选择一条记录
            if (SelectedFeedingRecordIds.Count > 1)
            {
                Message = "打印操作只能选择一条记录";
                ShowSuccess = false;
                return Page();
            }

            try
            {
                // 设置固定值
                var request = new UnloadingRequest
                {
                    FeedingRecordIds = SelectedFeedingRecordIds,
                    PCDeviceId = "PC001", // 固定值
                    PrintedBy = "PC操作员" // 固定值
                };

                var result = await _apiService.ProcessUnloadingAsync(request);
                if (result?.Success == true)
                {
                    Message = result.Message;
                    ShowSuccess = true;

                    // 清空选择
                    SelectedFeedingRecordIds.Clear();

                    // 重新加载有效记录
                    await LoadValidFeedingRecords();
                }
                else
                {
                    Message = result?.Message ?? "下料失败，请重试";
                    ShowSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"下料处理错误：{ex.Message}";
                ShowSuccess = false;
            }

            return Page();
        }

        // 下料完毕处理方法
        public async Task<IActionResult> OnPostCompleteAsync()
        {
            await LoadValidFeedingRecords();

            if (CompleteFeedingRecordId == 0)
            {
                Message = "请选择一条上料记录";
                ShowSuccess = false;
                return Page();
            }

            try
            {
                // 调用下料完成接口
                var result = await _apiService.ProcessClosedAsync(CompleteFeedingRecordId);
                if (result?.Success == true)
                {
                    Message = result.Message;
                    ShowSuccess = true;

                    // 清空选择
                    CompleteFeedingRecordId = 0;

                    // 重新加载有效记录
                    await LoadValidFeedingRecords();
                }
                else
                {
                    Message = result?.Message ?? "下料完成操作失败，请重试";
                    ShowSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"下料完成操作错误：{ex.Message}";
                ShowSuccess = false;
            }

            return Page();
        }

        private async Task LoadValidFeedingRecords()
        {
            try
            {
                ValidFeedingRecords = await _apiService.GetValidFeedingRecordsAsync();
                // 如果选择了物料，则筛选记录
                if (!string.IsNullOrEmpty(SelectedMaterialCode))
                {
                    ValidFeedingRecords = ValidFeedingRecords
                        .Where(r => r.ProductERPCode == SelectedMaterialCode)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Message = $"加载上料记录失败：{ex.Message}";
                ShowSuccess = false;
            }
        }

        private async Task LoadMaterials()
        {
            try
            {
                // 获取物料列表
                var materialResponse = await _apiService.GetMaterialsAsync(1, 1000);
                Materials = materialResponse.Data ?? new List<Material>();

                // 创建物料编码到装箱数量的映射
                MaterialPackingQuantities = Materials.ToDictionary(
                    m => m.FabricERPCode,
                    m => m.PackingQuantity
                );
            }
            catch (Exception ex)
            {
                Message = $"加载物料列表失败：{ex.Message}";
                ShowSuccess = false;
            }
        }
    }
}