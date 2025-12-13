// Services/TemplatePrintService.cs
using BarcodeLaminationModel.Models.Print;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace BarcodeLaminationAPI.Services
{
    public class TemplatePrintService : ITemplatePrintService
    {
        private readonly ILogger<TemplatePrintService> _logger;
        private readonly IWindowsPrintService _windowsPrintService;
        private readonly string _templateBasePath;
        private readonly string _templateFilesPath;

        public TemplatePrintService(ILogger<TemplatePrintService> logger,
            IWindowsPrintService windowsPrintService,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _windowsPrintService = windowsPrintService;
            _templateBasePath = Path.Combine(environment.ContentRootPath, "Templates");
            _templateFilesPath = Path.Combine(environment.WebRootPath, "template-files");

            Directory.CreateDirectory(_templateBasePath);
            Directory.CreateDirectory(_templateFilesPath);
        }

        // 原有的保存方法（不带文件）
        public async Task<string> SaveTemplateAsync(PrintTemplate template)
        {
            return await SaveTemplateAsync(template, null);
        }

        // 新的重载方法（带文件）
        public async Task<string> SaveTemplateAsync(PrintTemplate template, IFormFile templateFile)
        {
            try
            {
                if (string.IsNullOrEmpty(template.Name))
                {
                    throw new ArgumentException("模板名称不能为空");
                }

                // 如果有上传的文件，处理文件内容
                if (templateFile != null && templateFile.Length > 0)
                {
                    // 保存文件到服务器
                    string fileExtension = Path.GetExtension(templateFile.FileName);
                    string fileName = $"{template.Name}{fileExtension}";
                    string filePath = Path.Combine(_templateFilesPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await templateFile.CopyToAsync(stream);
                    }

                    // 如果是文本文件，读取内容到模板
                    if (fileExtension.ToLower() == ".txt" || fileExtension.ToLower() == ".btw")
                    {
                        template.Content = await File.ReadAllTextAsync(filePath);
                    }

                    template.TemplateType = GetTemplateTypeFromExtension(fileExtension);
                }

                // 保存模板信息到JSON文件
                string templatePath = Path.Combine(_templateBasePath, $"{template.Name}.json");

                template.UpdatedTime = DateTime.Now;
                if (template.CreatedTime == default)
                {
                    template.CreatedTime = DateTime.Now;
                }

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(template,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync(templatePath, jsonContent);
                _logger.LogInformation("模板保存成功: {TemplateName}", template.Name);
                return template.Name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板失败");
                throw;
            }
        }

        private string GetTemplateTypeFromExtension(string extension)
        {
            return extension.ToLower() switch
            {
                ".btw" => "BTW",
                ".xml" => "XML",
                ".json" => "JSON",
                _ => "TEXT"
            };
        }

        // 其他方法保持不变...
        public async Task<PrintResult> PrintWithTemplateAsync(TemplatePrintData printData)
        {
            try
            {
                var template = await GetTemplateAsync(printData.TemplateName);
                if (template == null)
                {
                    return new PrintResult { Success = false, Message = $"模板 '{printData.TemplateName}' 不存在" };
                }

                string processedContent = ProcessTemplateVariables(template.Content, printData);
                return await PrintTextTemplate(processedContent, printData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模板打印失败");
                return new PrintResult { Success = false, Message = $"模板打印失败: {ex.Message}" };
            }
        }

        private string ProcessTemplateVariables(string templateContent, TemplatePrintData printData)
        {
            var allVariables = new Dictionary<string, string>();

            if (printData.TemplateVariables != null)
            {
                foreach (var variable in printData.TemplateVariables)
                    allVariables[variable.Key] = variable.Value;
            }

            if (printData.Variables != null)
            {
                foreach (var variable in printData.Variables)
                    allVariables[variable.Key] = variable.Value;
            }

            allVariables["CurrentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            allVariables["CurrentTime"] = DateTime.Now.ToString("HH:mm:ss");
            allVariables["CurrentDateTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string result = templateContent;
            foreach (var variable in allVariables)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? "");
                result = result.Replace($"[[{variable.Key}]]", variable.Value ?? "");
                result = result.Replace($"%{variable.Key}%", variable.Value ?? "");
            }

            return result;
        }

        private async Task<PrintResult> PrintTextTemplate(string content, TemplatePrintData printData)
        {
            var textPrintData = new LabelPrintData
            {
                PrinterName = printData.PrinterName,
                Copies = printData.Copies,
                LabelType = "TemplateText",
                Variables = new Dictionary<string, string> { ["Content"] = content }
            };

            return await _windowsPrintService.PrintLabelAsync(textPrintData);
        }

        public async Task<bool> DeleteTemplateAsync(string templateName)
        {
            try
            {
                string templatePath = Path.Combine(_templateBasePath, $"{templateName}.json");
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateName}", templateName);
                return false;
            }
        }

        public async Task<List<PrintTemplate>> GetTemplatesAsync()
        {
            var templates = new List<PrintTemplate>();

            if (!Directory.Exists(_templateBasePath))
                return templates;

            foreach (string filePath in Directory.GetFiles(_templateBasePath, "*.json"))
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath);
                    var template = System.Text.Json.JsonSerializer.Deserialize<PrintTemplate>(content);
                    if (template != null && template.IsActive)
                    {
                        templates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "加载模板文件失败: {FilePath}", filePath);
                }
            }

            return templates.OrderBy(t => t.Name).ToList();
        }

        public async Task<PrintTemplate?> GetTemplateAsync(string templateName)
        {
            try
            {
                string templatePath = Path.Combine(_templateBasePath, $"{templateName}.json");
                if (File.Exists(templatePath))
                {
                    string content = await File.ReadAllTextAsync(templatePath);
                    return System.Text.Json.JsonSerializer.Deserialize<PrintTemplate>(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板失败: {TemplateName}", templateName);
                return null;
            }
        }

        public async Task<string> PreviewTemplateAsync(TemplatePrintData printData)
        {
            var template = await GetTemplateAsync(printData.TemplateName);
            if (template == null)
            {
                throw new FileNotFoundException($"模板 '{printData.TemplateName}' 不存在");
            }

            return ProcessTemplateVariables(template.Content, printData);
        }

        public async Task<bool> TemplateExistsAsync(string templateName)
        {
            string templatePath = Path.Combine(_templateBasePath, $"{templateName}.json");
            return File.Exists(templatePath);
        }
    }
}