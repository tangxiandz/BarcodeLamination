// BarTenderPrintService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BarcodeLaminationAPI.Services;
using BarcodeLaminationModel.Models.Print;
using System.Drawing.Printing;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

public class BarTenderPrintService : IBarTenderPrintService, IDisposable
{
    private readonly ILogger<BarTenderPrintService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private bool _disposed = false;

    public BarTenderPrintService(ILogger<BarTenderPrintService> logger, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<PrintResult> PrintFilmCoatingLabel(FilmCoatingPrintData printData)
    {
        try
        {
            // 1. 获取模板路径
            var templatePath = GetTemplatePath();

            if (!File.Exists(templatePath))
            {
                _logger.LogError($"模板文件不存在: {templatePath}");
                return new PrintResult
                {
                    Success = false,
                    Message = $"模板文件不存在: {templatePath}",
                    PrintTime = DateTime.Now
                };
            }

            // 2. 获取打印机名称
            var printerName = GetDefaultPrinter();

            _logger.LogInformation($"使用打印机: {printerName}");
            _logger.LogInformation($"使用模板: {templatePath}");
            _logger.LogInformation($"打印份数: {printData.Copies}");

            // 3. 创建数据文件
            string dataFilePath = CreateDataFile(printData);
            if (string.IsNullOrEmpty(dataFilePath))
            {
                _logger.LogError("创建数据文件失败");
                return new PrintResult
                {
                    Success = false,
                    Message = "创建数据文件失败",
                    PrintTime = DateTime.Now
                };
            }

            _logger.LogInformation($"创建数据文件: {dataFilePath}");

            // 4. 使用命令行打印
            var result = await PrintWithBarTenderCommandLine(templatePath, printerName, printData);

            // 5. 清理临时文件
            try
            {
                if (File.Exists(dataFilePath))
                {
                    File.Delete(dataFilePath);
                    _logger.LogInformation($"已删除临时数据文件: {dataFilePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "删除临时数据文件时出错");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打印标签时发生异常");
            return new PrintResult
            {
                Success = false,
                Message = $"打印失败: {ex.Message}",
                PrintTime = DateTime.Now
            };
        }
    }

    private string CreateDataFile(FilmCoatingPrintData printData)
    {
        string tempFilePath = Path.GetTempFileName();

        try
        {
            // 创建键值对数据文件
            var dataLines = new List<string>
            {
                "// 打印数据文件 - 用于BarTender模板变量替换",
                "// 格式: 变量名=值",
                "",
                $"ERPCode={printData.ErpCode ?? ""}",
                $"ProductPartDescription={printData.ProductPartDescription ?? ""}",
                $"OriginalQuantity={printData.OriginalQuantity?.ToString() ?? "0"}",
                $"NewQuantity={printData.NewQuantity?.ToString() ?? "0"}",
                $"OriginalBatch={printData.OriginalBatch ?? ""}",
                $"NewBatch={printData.NewBatch ?? ""}",
                $"NewQRCode={printData.NewQRCode ?? ""}",
                $"PrintedBy={printData.PrintedBy ?? ""}",
                $"PrintTime={DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };

            File.WriteAllLines(tempFilePath, dataLines, Encoding.UTF8);

            // 验证文件内容
            var fileContent = File.ReadAllText(tempFilePath);
            _logger.LogInformation($"数据文件内容:\n{fileContent}");

            return tempFilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建数据文件时出错");

            // 清理临时文件
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            return null;
        }
    }

    private async Task<PrintResult> PrintWithBarTenderCommandLine(string templatePath, string printerName, FilmCoatingPrintData printData)
    {
        Process process = null;

        try
        {
            var btPrintExePath = FindBarTenderPrintExe();
            if (string.IsNullOrEmpty(btPrintExePath))
            {
                _logger.LogError("未找到BarTender命令行工具");
                return new PrintResult
                {
                    Success = false,
                    Message = "未找到BarTender命令行工具，请确保BarTender已正确安装",
                    PrintTime = DateTime.Now
                };
            }

            _logger.LogInformation($"找到BarTender命令行工具: {btPrintExePath}");

            if (!File.Exists(templatePath))
            {
                _logger.LogError($"模板文件不存在: {templatePath}");
                return new PrintResult
                {
                    Success = false,
                    Message = $"模板文件不存在: {templatePath}",
                    PrintTime = DateTime.Now
                };
            }

            // 检查打印机
            bool printerExists = false;
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Equals(printerName, StringComparison.OrdinalIgnoreCase))
                {
                    printerExists = true;
                    _logger.LogInformation($"打印机 '{printerName}' 存在");
                    break;
                }
            }

            if (!printerExists)
            {
                _logger.LogWarning($"打印机 '{printerName}' 不存在于已安装打印机列表中");
                var defaultPrinterSettings = new PrinterSettings();
                printerName = defaultPrinterSettings.PrinterName;
                _logger.LogWarning($"将使用默认打印机: {printerName}");
            }

            // 方法1：通过/D参数传递变量值
            var arguments = BuildCorrectCommandLineArguments(templatePath, printerName, printData);

            _logger.LogInformation($"完整命令: {btPrintExePath} {arguments}");

            process = new Process();
            process.StartInfo.FileName = btPrintExePath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(btPrintExePath);
            process.StartInfo.StandardOutputEncoding = Encoding.Default;
            process.StartInfo.StandardErrorEncoding = Encoding.Default;

            _logger.LogInformation("启动BarTender打印进程...");
            process.Start();

            // 异步读取输出
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            bool exited = process.WaitForExit(30000);

            if (!exited)
            {
                process.Kill();
                _logger.LogError("BarTender进程执行超时");
                return new PrintResult
                {
                    Success = false,
                    Message = "打印超时",
                    PrintTime = DateTime.Now
                };
            }

            var output = await outputTask;
            var error = await errorTask;

            _logger.LogInformation($"进程退出代码: {process.ExitCode}");

            if (!string.IsNullOrEmpty(output))
                _logger.LogInformation($"BarTender输出: {output}");

            if (!string.IsNullOrEmpty(error))
                _logger.LogError($"BarTender错误: {error}");

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("✅ BarTender打印命令执行成功");
                return new PrintResult
                {
                    Success = true,
                    Message = "打印命令执行成功",
                    PrinterName = printerName,
                    Copies = printData.Copies,
                    PrintedCopies = printData.Copies,
                    PrintTime = DateTime.Now
                };
            }
            else
            {
                _logger.LogError($"❌ BarTender打印失败，退出代码: {process.ExitCode}");
                return new PrintResult
                {
                    Success = false,
                    Message = $"BarTender打印失败，退出代码: {process.ExitCode}",
                    PrinterName = printerName,
                    Copies = printData.Copies,
                    PrintedCopies = 0,
                    PrintTime = DateTime.Now
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调用BarTender命令行时发生异常");
            return new PrintResult
            {
                Success = false,
                Message = $"打印异常: {ex.Message}",
                PrintTime = DateTime.Now
            };
        }
        finally
        {
            process?.Dispose();
        }
    }

    private string BuildCorrectCommandLineArguments(string templatePath, string printerName, FilmCoatingPrintData printData)
    {
        var args = new StringBuilder();
        printerName = printerName.Trim('"');

        // 基本参数
        args.Append($"/AF=\"{templatePath}\" ");
        args.Append($"/PRN=\"{printerName}\" ");

        // 添加自动调整页面设置
        args.Append($"/AutoResize ");

        // 通过/SetNamedSubString参数传递变量值
        AddNamedSubStringArgument(args, "ERPCode", printData.ErpCode);
        AddNamedSubStringArgument(args, "ProductPartDescription", printData.ProductPartDescription);
        AddNamedSubStringArgument(args, "OriginalQuantity", printData.OriginalQuantity?.ToString() ?? "0");
        AddNamedSubStringArgument(args, "NewQuantity", printData.NewQuantity?.ToString() ?? "0");
        AddNamedSubStringArgument(args, "OriginalBatch", printData.OriginalBatch);
        AddNamedSubStringArgument(args, "NewBatch", printData.NewBatch);
        AddNamedSubStringArgument(args, "NewQRCode", printData.NewQRCode);
        AddNamedSubStringArgument(args, "PrintedBy", printData.PrintedBy);
        AddNamedSubStringArgument(args, "PrintTime", DateTime.Now.ToString("yyyy-MM-dd:ss"));

        // 打印份数
        if (printData.Copies > 1)
        {
            args.Append($"/Copies={printData.Copies} ");
        }

        args.Append("/X");  // 打印后退出

        return args.ToString().Trim();
    }
    private void AddNamedSubStringArgument(StringBuilder args, string name, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = "";
        }

        // 转义值中的引号
        value = EscapeForCommandLine(value);

        // 使用/SetNamedSubString "名称" "值" 格式
        args.Append($"/D \"{name}\" \"{value}\" ");
    }

    private string EscapeForCommandLine(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        // 转义命令行中的特殊字符
        return input.Replace("\"", "\\\"");
    }
    private string EscapeVariable(string value)
    {
        // 对特殊字符进行转义
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");  // 双引号转义
    }

    private string FindBarTenderPrintExe()
    {
        // 常见的BarTender安装路径
        var searchPaths = new[]
        {
            @"C:\Program Files\Seagull\BarTender Suite\bartend.exe",
            @"C:\Program Files (x86)\Seagull\BarTender Suite\bartend.exe",
            @"C:\Program Files\Seagull\BarTender 2022\bartend.exe",
            @"C:\Program Files\Seagull\BarTender 2021\bartend.exe",
            @"C:\Program Files\Seagull\BarTender 2020\bartend.exe",
            @"C:\Program Files\Seagull\BarTender 2019\bartend.exe",
            @"D:\Program Files\Seagull\BarTender Suite\bartend.exe"
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // 在PATH环境变量中查找
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(';');
        if (paths != null)
        {
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, "bartend.exe");
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    private string BuildCommandLineArguments(string templatePath, string dataFilePath, string printerName, int copies)
    {
        // 使用BarTender命令行参数
        var args = new StringBuilder();

        // 移除打印机名称中的多余引号
        printerName = printerName.Trim('"');

        // 构建命令参数
        args.Append($"/AF=\"{templatePath}\" ");      // 模板文件
        args.Append($"/F=\"{dataFilePath}\" ");        // 数据文件
        args.Append($"/PRN=\"{printerName}\" ");       // 打印机

        if (copies > 1)
        {
            args.Append($"/P={copies} ");               // 打印份数
        }

        args.Append("/X");  // 打印后退出

        return args.ToString().Trim();
    }

    private string GetTemplatePath()
    {
        var relativePath = _configuration["BarTender:TemplatePath"] ?? "LabelTemplates/FilmCoating.btw";
        var appRoot = _environment.ContentRootPath;
        var fullPath = Path.Combine(appRoot, relativePath);

        return fullPath;
    }

    public async Task TestConnection()
    {
        try
        {
            // 测试BarTender命令行工具
            var btPrintExePath = FindBarTenderPrintExe();

            if (string.IsNullOrEmpty(btPrintExePath))
            {
                throw new Exception("未找到BarTender命令行工具");
            }

            if (!File.Exists(btPrintExePath))
            {
                throw new Exception($"BarTender命令行工具不存在: {btPrintExePath}");
            }

            _logger.LogInformation("BarTender命令行工具连接测试成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BarTender连接测试失败");
            throw;
        }
    }

    private string GetDefaultPrinter()
    {
        try
        {
            var defaultPrinter = _configuration["BarTender:DefaultPrinter"];
            if (!string.IsNullOrEmpty(defaultPrinter))
                return defaultPrinter;

            // 尝试获取系统默认打印机
            var printerSettings = new PrinterSettings();
            return printerSettings.PrinterName;
        }
        catch
        {
            return "Canon G5080 series";
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 清理资源
            }
            _disposed = true;
        }
    }
}