using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace BarcodeLaminationWeb.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        public string? ErrorMessage { get; set; }

        public void OnGet(string? message = null)
        {
            ErrorMessage = message ?? "系统发生未知错误";
        }
    }
}