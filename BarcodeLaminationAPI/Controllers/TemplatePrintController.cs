// Controllers/TemplatePrintController.cs
using Microsoft.AspNetCore.Mvc;
using BarcodeLaminationModel.Models.Print;
using BarcodeLaminationAPI.Services;

namespace BarcodeLaminationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TemplatePrintController : ControllerBase
    {
        private readonly ITemplatePrintService _templatePrintService;
        private readonly ILogger<TemplatePrintController> _logger;

        public TemplatePrintController(ITemplatePrintService templatePrintService,
            ILogger<TemplatePrintController> logger)
        {
            _templatePrintService = templatePrintService;
            _logger = logger;
        }

        [HttpPost("print")]
        public async Task<ActionResult<PrintResult>> PrintWithTemplate([FromBody] TemplatePrintData printData)
        {
            try
            {
                var result = await _templatePrintService.PrintWithTemplateAsync(printData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打印请求失败");
                return BadRequest(new PrintResult { Success = false, Message = ex.Message });
            }
        }

        // 支持文件上传的模板保存方法
        [HttpPost("templates")]
        public async Task<ActionResult<string>> UploadTemplate([FromForm] TemplateUploadRequest request)
        {
            try
            {
                var template = new PrintTemplate
                {
                    Name = request.Name,
                    Description = request.Description,
                    Content = request.Content,
                    TemplateType = request.TemplateType
                };

                // 使用带文件参数的重载方法
                var result = await _templatePrintService.SaveTemplateAsync(template, request.TemplateFile);
                return Ok(new { TemplateName = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上传模板失败");
                return BadRequest(ex.Message);
            }
        }

        // 仅保存模板信息（不带文件）
        [HttpPost("templates/simple")]
        public async Task<ActionResult<string>> SaveTemplate([FromBody] PrintTemplate template)
        {
            try
            {
                var result = await _templatePrintService.SaveTemplateAsync(template);
                return Ok(new { TemplateName = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板失败");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("templates")]
        public async Task<ActionResult<List<PrintTemplate>>> GetTemplates()
        {
            var templates = await _templatePrintService.GetTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("templates/{name}")]
        public async Task<ActionResult<PrintTemplate>> GetTemplate(string name)
        {
            var template = await _templatePrintService.GetTemplateAsync(name);
            if (template == null)
                return NotFound();

            return Ok(template);
        }

        [HttpDelete("templates/{name}")]
        public async Task<ActionResult> DeleteTemplate(string name)
        {
            var result = await _templatePrintService.DeleteTemplateAsync(name);
            return result ? Ok() : NotFound();
        }

        [HttpPost("preview")]
        public async Task<ActionResult<string>> PreviewTemplate([FromBody] TemplatePrintData printData)
        {
            try
            {
                var content = await _templatePrintService.PreviewTemplateAsync(printData);
                return Ok(content);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}