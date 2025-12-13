using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BarcodeLaminationWeb.Services;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationWeb.Pages
{
    public class MaterialsModel : PageModel
    {
        private readonly IApiService _apiService;

        public PagedResponse<Material> MaterialsResponse { get; set; } = new PagedResponse<Material>();

        [BindProperty]
        public MaterialRequest NewMaterial { get; set; } = new MaterialRequest();

        [BindProperty]
        public MaterialRequest EditMaterial { get; set; } = new MaterialRequest();

        [BindProperty]
        public int EditMaterialId { get; set; }

        [BindProperty]
        public int DeleteMaterialId { get; set; }

        public string Message { get; set; } = string.Empty;
        public bool ShowSuccess { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public MaterialsModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task OnGetAsync()
        {
            MaterialsResponse = await _apiService.GetMaterialsAsync(CurrentPage, PageSize, SearchTerm);
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            try
            {
                NewMaterial.CreatedBy = "PC管理员";
                var success = await _apiService.AddMaterialAsync(NewMaterial);

                if (success)
                {
                    Message = "物料添加成功！";
                    ShowSuccess = true;
                    NewMaterial = new MaterialRequest();
                }
                else
                {
                    Message = "物料添加失败，请重试！";
                    ShowSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"添加物料时出错：{ex.Message}";
                ShowSuccess = false;
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                currentPage = CurrentPage,
                pageSize = PageSize
            });
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            try
            {
                var success = await _apiService.UpdateMaterialAsync(EditMaterialId, EditMaterial);

                if (success)
                {
                    Message = "物料修改成功！";
                    ShowSuccess = true;
                }
                else
                {
                    Message = "物料修改失败，请重试！";
                    ShowSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"修改物料时出错：{ex.Message}";
                ShowSuccess = false;
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                currentPage = CurrentPage,
                pageSize = PageSize
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var success = await _apiService.DeleteMaterialAsync(id);

                if (success)
                {
                    Message = "物料删除成功！";
                    ShowSuccess = true;
                }
                else
                {
                    Message = "物料删除失败，请重试！";
                    ShowSuccess = false;
                }
            }
            catch (Exception ex)
            {
                Message = $"删除物料时出错：{ex.Message}";
                ShowSuccess = false;
            }

            return RedirectToPage(new
            {
                searchTerm = SearchTerm,
                currentPage = CurrentPage,
                pageSize = PageSize
            });
        }

        public async Task<JsonResult> OnGetMaterialDetailsAsync(int id)
        {
            var material = await _apiService.GetMaterialByIdAsync(id);
            return new JsonResult(material);
        }
    }
}