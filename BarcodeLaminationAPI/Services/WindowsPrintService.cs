// BarcodeLaminationAPI/Services/WindowsPrintService.cs
using BarcodeLaminationModel.Models.Print;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ZXing; // 添加条形码生成库

namespace BarcodeLaminationAPI.Services
{
    public class WindowsPrintService : IWindowsPrintService
    {
        private readonly ILogger<WindowsPrintService> _logger;

        public WindowsPrintService(ILogger<WindowsPrintService> logger)
        {
            _logger = logger;
        }

        public async Task<PrintResult> PrintLabelAsync(LabelPrintData printData)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var printDocument = new  PrintDocument();

                    if (!string.IsNullOrEmpty(printData.PrinterName))
                    {
                        printDocument.PrinterSettings.PrinterName = printData.PrinterName;
                    }

                    if (!printDocument.PrinterSettings.IsValid)
                    {
                        return new PrintResult { Success = false, Message = "打印机不可用" };
                    }

                    printDocument.PrinterSettings.Copies = (short)printData.Copies;
                    printDocument.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);

                    string printedContent = string.Empty;
                    bool printSuccess = false;

                    printDocument.PrintPage += (sender, e) =>
                    {
                        try
                        {
                            printedContent = GenerateLabelContent(printData);

                            // 使用正确的 Graphics 对象
                            using var font = new Font("宋体", 10);
                            using var brush = new SolidBrush(Color.Black);

                            var bounds = e.MarginBounds;
                            if (bounds.Width == 0) bounds = e.PageBounds;

                            e.Graphics.DrawString(printedContent, font, brush, bounds);
                            printSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "打印页面渲染失败");
                            printSuccess = false;
                        }
                    };

                    printDocument.Print();
                    return new PrintResult
                    {
                        Success = printSuccess,
                        Message = printSuccess ? "打印成功" : "打印失败",
                        //PrintedContent = printedContent
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "打印异常");
                    return new PrintResult { Success = false, Message = $"打印失败: {ex.Message}" };
                }
            });
        }

        private string GenerateLabelContent(LabelPrintData printData)
        {
            var sb = new StringBuilder();

            switch (printData.LabelType)
            {
                case "Unloading":
                    sb.AppendLine("=== 下料标签 ===");
                    sb.AppendLine($"产品ERP号: {printData.Variables.GetValueOrDefault("ProductERPCode")}");
                    sb.AppendLine($"数量: {printData.Variables.GetValueOrDefault("Quantity")}");
                    sb.AppendLine($"批次: {printData.Variables.GetValueOrDefault("BatchNumber")}");
                    sb.AppendLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    break;

                default:
                    foreach (var variable in printData.Variables)
                    {
                        sb.AppendLine($"{variable.Key}: {variable.Value}");
                    }
                    break;
            }

            return sb.ToString();
        }

        public async Task<List<PrinterInfo>> GetAvailablePrintersAsync()
        {
            return await Task.Run(() =>
            {
                var printers = new List<PrinterInfo>();

                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    try
                    {
                        var settings = new PrinterSettings { PrinterName = printerName };
                        printers.Add(new PrinterInfo
                        {
                            Name = printerName,
                            IsDefault = settings.IsDefaultPrinter,
                            IsValid = settings.IsValid
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "获取打印机信息失败: {PrinterName}", printerName);
                        printers.Add(new PrinterInfo
                        {
                            Name = printerName,
                            IsValid = false
                        });
                    }
                }

                return printers;
            });
        }
    }
}