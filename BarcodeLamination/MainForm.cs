using BarcodeLaminationModel.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MistakeProofing.Printing.BarTender;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Format = MistakeProofing.Printing.BarTender.Format;

namespace BarcodeLaminationPrint
{
    public partial class MainForm : Form
    {
        private System.Timers.Timer _pollingTimer;
        private bool _isPolling = false;
        private readonly string _connectionString;
        private string _templatePath; 
        private string _printerName;
        private readonly string _barTenderPath;
        private readonly int _pollingInterval;
        private object _lockObject = new object();
        private MistakeProofing.Printing.BarTender.Application? _btApp;

        private int _printedCount = 0;
        private readonly string _templateDirectory;
        // 打印类型枚举
        private enum PrintType
        {
            FilmCoating,  // 覆膜打印
            Unloading     // 下料打印
        }

        public MainForm()
        {
            InitializeComponent();

            // 从配置文件读取设置
            _templateDirectory = Path.Combine(System.Windows.Forms.Application.StartupPath, "BartenderTemp");
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            _templatePath = ConfigurationManager.AppSettings["TemplatePath"];
            //_printerName = ConfigurationManager.AppSettings["PrinterName"];
            _barTenderPath = ConfigurationManager.AppSettings["BarTenderPath"];
            _printerName = Properties.Settings.Default.Printer1;
            _templatePath = GetCurrentTemplatePath();
            EnsureTemplateDirectory();
            if (!int.TryParse(ConfigurationManager.AppSettings["PollingInterval"], out _pollingInterval))
            {
                _pollingInterval = 5000; // 默认5秒
            }
            try
            {
                _btApp = new MistakeProofing.Printing.BarTender.Application();
            }
            catch (Exception e)
            {
                AddLog($"打印标签软件初始化失败: {e.Message}");
                return;
            }
            // 初始化定时器
            _pollingTimer = new System.Timers.Timer(_pollingInterval);
            _pollingTimer.Elapsed += PollingTimer_Elapsed;

            // 加载上次打印计数
            LoadPrintedCount();

            // 自动启动
            if (chkAutoStart.Checked)
            {
                StartPolling();
                CheckForPrintJobs();
            }
            // 更新界面显示
            UpdatePrinterDisplay();
            UpdateTemplateDisplay();
        }

        /// <summary>
        /// 确保模板目录存在
        /// </summary>
        private void EnsureTemplateDirectory()
        {
            try
            {
                if (!Directory.Exists(_templateDirectory))
                {
                    Directory.CreateDirectory(_templateDirectory);
                    AddLog($"✅ 创建模板目录: {_templateDirectory}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 创建模板目录失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 获取当前模板路径
        /// </summary>
        private string GetCurrentTemplatePath()
        {
            var templateFile = Properties.Settings.Default.TemplateFile;
            if (!string.IsNullOrEmpty(templateFile))
            {
                string fullPath = Path.Combine(_templateDirectory, templateFile);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // 如果没有设置或文件不存在，尝试查找目录中的第一个模板文件
            if (Directory.Exists(_templateDirectory))
            {
                var templateFiles = Directory.GetFiles(_templateDirectory, "*.btw");
                if (templateFiles.Length > 0)
                {
                    Properties.Settings.Default.TemplateFile = Path.GetFileName(templateFiles[0]);
                    Properties.Settings.Default.Save();
                    return templateFiles[0];
                }
            }

            return null;
        }
        /// <summary>
        /// 获取当前打印类型
        /// </summary>
        private PrintType GetCurrentPrintType()
        {
            return rdoFilmCoating.Checked ? PrintType.FilmCoating : PrintType.Unloading;
        }
        /// <summary>
        /// 获取当前选择的打印机名称
        /// </summary>
        /// <summary>
        /// 获取当前模板名称
        /// </summary>
        private string GetCurrentTemplateName()
        {
            if (string.IsNullOrEmpty(_templatePath) || !File.Exists(_templatePath))
                return "未设置";

            return Path.GetFileName(_templatePath);
        }
        /// <summary>
        /// 选择打印机按钮点击事件
        /// </summary>
        private void btnSelectPrinter_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDialog printDialog = new PrintDialog())
                {
                    // 设置默认打印机
                    if (!string.IsNullOrEmpty(_printerName))
                    {
                        printDialog.PrinterSettings.PrinterName = _printerName;
                    }

                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        _printerName = printDialog.PrinterSettings.PrinterName;
                        Properties.Settings.Default.Printer1 = _printerName;
                        Properties.Settings.Default.Save();

                        UpdatePrinterDisplay();
                        AddLog($"✅ 打印机已设置为: {_printerName}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 选择打印机失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 上传模板按钮点击事件
        /// </summary>
        private void btnUploadTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "BarTender 模板文件 (*.btw)|*.btw|所有文件 (*.*)|*.*";
                    openFileDialog.Title = "选择 BarTender 模板文件";
                    openFileDialog.Multiselect = false;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string sourceFile = openFileDialog.FileName;
                        string fileName = Path.GetFileName(sourceFile);
                        string destFile = Path.Combine(_templateDirectory, fileName);

                        // 检查文件扩展名
                        if (!Path.GetExtension(fileName).Equals(".btw", StringComparison.OrdinalIgnoreCase))
                        {
                            AddLog("⚠️ 警告: 选择的文件可能不是 BarTender 模板文件 (.btw)");
                        }

                        // 如果目标文件已存在，询问是否覆盖
                        if (File.Exists(destFile))
                        {
                            var result = MessageBox.Show($"模板文件 {fileName} 已存在，是否覆盖？",
                                "确认覆盖", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result != DialogResult.Yes)
                                return;
                        }

                        // 复制文件
                        File.Copy(sourceFile, destFile, true);

                        // 更新设置
                        _templatePath = destFile;
                        Properties.Settings.Default.TemplateFile = fileName;
                        Properties.Settings.Default.Save();

                        UpdateTemplateDisplay();
                        AddLog($"✅ 模板上传成功: {fileName}");

                        // 如果 BarTender 应用正在运行，重新加载模板
                        if (_btApp != null)
                        {
                            try
                            {
                                // 关闭所有已打开的格式
                                foreach (var format in _btApp.Formats)
                                {
                                    format.Close(SaveOptionConstants.DoNotSaveChanges);
                                }
                                AddLog("✅ 已重新加载模板");
                            }
                            catch (Exception ex)
                            {
                                AddLog($"⚠️ 重新加载模板时出现警告: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 上传模板失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 打印类型选择变更事件
        /// </summary>
        private void PrintType_CheckedChanged(object sender, EventArgs e)
        {
            if ((RadioButton)sender != null && ((RadioButton)sender).Checked)
            {
                UpdatePrinterDisplay();
                UpdateTemplateDisplay();
                AddLog($"✅ 切换到{(GetCurrentPrintType() == PrintType.FilmCoating ? "覆膜打印" : "下料打印")}模式");
            }
        }


        /// <summary>
        /// 更新打印机显示
        /// </summary>
        private void UpdatePrinterDisplay()
        {
            if (lblCurrentPrinter.InvokeRequired)
            {
                lblCurrentPrinter.Invoke(new Action(UpdatePrinterDisplay));
                return;
            }

            lblCurrentPrinter.Text = $"当前打印机: {_printerName ?? "未设置"}";
        }

        /// <summary>
        /// 更新模板显示
        /// </summary>
        private void UpdateTemplateDisplay()
        {
            if (lblCurrentTemplate.InvokeRequired)
            {
                lblCurrentTemplate.Invoke(new Action(UpdateTemplateDisplay));
                return;
            }

            lblCurrentTemplate.Text = $"当前模板: {GetCurrentTemplateName()}";
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    StartButton_Click(sender, e);
                    break;
                case Keys.F2:
                    StopButton_Click(sender, e);
                    break;
                case Keys.F5:
                    ClearLogButton_Click(sender, e);
                    break;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartPolling();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopPolling();
        }

        private void TestConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                AddLog("正在测试数据库连接...");

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT COUNT(*) FROM FilmCoatingRecords", connection);
                    int count = (int)command.ExecuteScalar();

                    AddLog($"✅ 数据库连接成功，表中有 {count} 条记录");
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 数据库连接失败: {ex.Message}");
            }
        }

        private void TestPrintButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 检查模板是否存在
                string templatePath = GetCurrentTemplatePath();
                if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
                {
                    AddLog("❌ 请先上传模板文件");
                    return;
                }

                // 检查打印机是否设置
                string printerName = _printerName;
                if (string.IsNullOrEmpty(printerName))
                {
                    AddLog("❌ 请先选择打印机");
                    return;
                }

                AddLog($"开始{(GetCurrentPrintType() == PrintType.FilmCoating ? "覆膜打印" : "下料打印")}测试...");

                if (GetCurrentPrintType() == PrintType.FilmCoating)
                {
                    // 创建一个测试记录
                    var testRecord = new FilmCoatingRecord
                    {
                        Id = 999,
                        NewERPCode = "TEST001",
                        ProductPartDescription = "测试产品描述",
                        Quantity = 100,
                        BatchNumber = "BATCH001",
                        PDADeviceId = "TestDevice"
                    };

                    Task.Run(() => ProcessPrintJob(testRecord));
                }
                else
                {
                    // 创建下料打印测试记录
                    var testRecord = new UnloadingRecord
                    {
                        Id = 999,
                        ProductERPCode = "TEST002",
                        ProductPartDescription = "测试下料产品",
                        Quantity = 200,
                        BatchNumber = "BATCH002"
                    };

                    Task.Run(() => ProcessPrintJob2(testRecord));
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 测试打印失败: {ex.Message}");
            }
        }

        private void ClearLogButton_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            AddLog("日志已清空");
        }

        private void StartPolling()
        {
            if (!_isPolling)
            {
                _pollingTimer.Start();
                _isPolling = true;

                lblStatus.Text = "服务状态: 运行中";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                AddLog("✅ 打印监控服务已启动");
                AddLog($"轮询间隔: {_pollingInterval / 1000} 秒");
            }
        }

        private void StopPolling()
        {
            if (_isPolling)
            {
                _pollingTimer.Stop();
                _isPolling = false;

                lblStatus.Text = "服务状态: 已停止";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnStart.Enabled = true;
                btnStop.Enabled = false;

                AddLog("⏸️ 打印监控服务已停止");
            }
        }

        private void PollingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                try
                {
                    UpdateLastCheckTime();
                    CheckForPrintJobs();
                }
                catch (Exception ex)
                {
                    AddLog($"❌ 检查打印任务时出错: {ex.Message}");
                }
            }
        }

        private void UpdateLastCheckTime()
        {
            if (txtLastCheck.InvokeRequired)
            {
                txtLastCheck.Invoke((MethodInvoker)delegate
                {
                    txtLastCheck.Text = DateTime.Now.ToString("HH:mm:ss");
                });
            }
            else
            {
                txtLastCheck.Text = DateTime.Now.ToString("HH:mm:ss");
            }
        }
        private void CheckForPrintJobs()
        {
            try
            {
                // 获取当前打印类型
                PrintType currentPrintType = GetCurrentPrintType();

                // 检查必要的设置
                string templatePath = GetCurrentTemplatePath();
                if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
                {
                    AddLog("⚠️ 模板文件未设置，无法处理打印任务");
                    return;
                }

                string printerName = _printerName;
                if (string.IsNullOrEmpty(printerName))
                {
                    AddLog("⚠️ 打印机未设置，无法处理打印任务");
                    return;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    if (currentPrintType == PrintType.FilmCoating)
                    {
                        // 查询待打印的覆膜记录
                        // 查询待打印的下料记录
                        var query = @"
                        SELECT TOP 1 * 
                        FROM FilmCoatingRecords 
                        WHERE Status = '待打印' 
                        ORDER BY CreatedTime ASC";

                        using (var command = new SqlCommand(query, connection))
                        {
                            // 先读取第一条记录
                            FilmCoatingRecord unloadingRecord = null;
                            int? unloadingRecordId = null;
                            string productERPCode = null;

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    unloadingRecordId = reader.GetInt32(reader.GetOrdinal("Id"));
                                    productERPCode = reader.GetString(reader.GetOrdinal("NewERPCode"));

                                    unloadingRecord = new FilmCoatingRecord
                                    {
                                        Id = unloadingRecordId.Value,
                                        ProductERPCode = productERPCode,
                                        ProductPartDescription = reader.GetString(reader.GetOrdinal("ProductPartDescription")),
                                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                        BatchNumber = reader.GetString(reader.GetOrdinal("BatchNumber")),
                                        PrintTime = reader.GetDateTime(reader.GetOrdinal("PrintTime")),
                                        CreatedTime = reader.GetDateTime(reader.GetOrdinal("CreatedTime")),
                                        Status = reader.GetString(reader.GetOrdinal("Status"))
                                    };

                                    AddLog($"📄 发现待打印下料记录 ID: {unloadingRecord.Id}, ERP: {unloadingRecord.ProductERPCode}");
                                }
                            } // 第一个DataReader在此处关闭
                            if (unloadingRecord == null || string.IsNullOrEmpty(productERPCode))
                            {
                                return;
                            }
                            Task.Run(() => ProcessPrintJob(unloadingRecord));
                        }
                    }
                    else
                    {
                        // 查询待打印的下料记录
                        // 查询待打印的记录
                        var query = @"
                        SELECT TOP 1 * 
                        FROM UnloadingRecords 
                        WHERE PrintStatus = 0 
                        ORDER BY CreatedTime ASC";

                        using (var command = new SqlCommand(query, connection))
                        {
                            // 先读取第一条记录
                            UnloadingRecord unloadingRecord = null;
                            int? unloadingRecordId = null;
                            string productERPCode = null;

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    unloadingRecordId = reader.GetInt32(reader.GetOrdinal("Id"));
                                    productERPCode = reader.GetString(reader.GetOrdinal("ProductERPCode"));

                                    unloadingRecord = new UnloadingRecord
                                    {
                                        Id = unloadingRecordId.Value,
                                        ProductERPCode = productERPCode,
                                        ProductPartDescription = reader.GetString(reader.GetOrdinal("ProductPartDescription")),
                                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                        BatchNumber = reader.GetString(reader.GetOrdinal("BatchNumber")),
                                        PrintTime = reader.GetDateTime(reader.GetOrdinal("PrintTime")),
                                        CreatedTime = reader.GetDateTime(reader.GetOrdinal("CreatedTime")),
                                        PrintStatus = reader.GetInt32(reader.GetOrdinal("PrintStatus"))
                                    };

                                   // AddLog($"📄 发现待打印{recordType}记录 ID: {unloadingRecord.Id}, ERP: {unloadingRecord.ProductERPCode}");
                                }
                            } // DataReader在此处关闭
                            
                            // 如果没有找到记录，直接返回
                            if (unloadingRecord == null || string.IsNullOrEmpty(productERPCode))
                            {
                                return;
                            }

                            // 查询材料信息
                            bool hasMaterialInfo = TryGetMaterialInfo(connection, productERPCode, unloadingRecord);

                            if (hasMaterialInfo)
                            {
                                AddLog($"📦 找到材料信息，更新包装数量: {unloadingRecord.Quantity}");
                            }
                            else
                            {
                                AddLog($"⚠️ 未找到材料信息，使用原始数量: {unloadingRecord.Quantity}");
                            }

                            // 处理打印任务
                            Task.Run(() => ProcessPrintJob2(unloadingRecord));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 数据库查询失败: {ex.Message}");
            }
        }

        private bool TryGetMaterialInfo(SqlConnection connection, string productERPCode, UnloadingRecord unloadingRecord)
        {
            try
            {
                // 使用参数化查询避免SQL注入
                var materialQuery = @"
            SELECT TOP 1 * 
            FROM Materials 
            WHERE ProductERPCode = @ProductERPCode
            ORDER BY CreateTime ASC";

                using (var materialCommand = new SqlCommand(materialQuery, connection))
                {
                    materialCommand.Parameters.AddWithValue("@ProductERPCode", productERPCode);

                    using (var materialReader = materialCommand.ExecuteReader())
                    {
                        if (materialReader.Read())
                        {
                            // 如果PackingQuantity列存在，则更新数量
                            int packingQuantityOrdinal = materialReader.GetOrdinal("PackingQuantity");
                            if (!materialReader.IsDBNull(packingQuantityOrdinal))
                            {
                                unloadingRecord.Quantity = materialReader.GetInt32(packingQuantityOrdinal);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"⚠️ 查询材料信息失败: {ex.Message}");
            }

            return false;
        }
        private async Task ProcessPrintJob(FilmCoatingRecord printRecord) 
        {
            // 检查模板和打印机设置
            if (string.IsNullOrEmpty(_templatePath) || !File.Exists(_templatePath))
            {
                AddLog("❌ 模板文件未设置或不存在");
                return;
            }

            if (string.IsNullOrEmpty(_printerName))
            {
                AddLog("❌ 打印机未设置");
                return;
            }


            Format? format = null;
            Template? template = null;

            try
            {
                AddLog($"🔄 开始处理打印任务 ID: {printRecord.Id}");

                // 1. 读取配置好的模版文件
                AddLog("📋 步骤1: 读取模版文件");
                _btApp ??= new MistakeProofing.Printing.BarTender.Application();
                format = _btApp.Formats.Open(_templatePath);

                // 2. 创建模板对象并识别属性
                AddLog("🔍 步骤2: 识别模版属性值");
                template = Template.Create(format, null);

                // 3. 使用模板绑定方式填充数据
                AddLog("📝 步骤3: 使用模板绑定填充数据");
                List<customproductattributes> customproductattributeslist=  new List<customproductattributes>();
                customproductattributeslist.Add(new customproductattributes() { ID=1,ProductId=1,Name= "ERPCode",Value= "NewERPCode",});
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "ProductPartDescription", Value = "ProductPartDescription", });
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "Quantity", Value = "Quantity", });
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "BatchNumberCode", Value = "BatchNumberCode", });
                var label = new LabelP(format, template.DataBindings, customproductattributeslist);
                label.DataSource = printRecord;

                // 4. 设置打印参数
                AddLog("🏷️ 步骤4: 设置打印参数");
                format.Printer = _printerName;
                format.IdenticalCopiesOfLabel = 1;
                format.NumberSerializedLabels = 1;

                // 5. 执行打印
                AddLog("🖨️ 步骤5: 执行打印");
                label.Print(1, 1); // 打印1份，序列化标签数为1

                AddLog($"✅ 打印任务完成: ID {printRecord.Id}, ERP: {printRecord.NewERPCode}");

                // 更新状态
                UpdatePrintStatus(printRecord.Id, "已打印");
                UpdatePrintedCount(1);
            }
            catch (Exception ex)
            {
                AddLog($"❌ 打印处理失败: {ex.Message}");
                UpdatePrintStatus(printRecord.Id, "打印失败");
            }
            finally
            {
                // 清理资源
                format?.Close(SaveOptionConstants.DoNotSaveChanges);
               // template?.Dispose();
            }
        }
        private async Task ProcessPrintJob2(UnloadingRecord unloadingRecord)
        {
            // 检查模板和打印机设置
            if (string.IsNullOrEmpty(_templatePath) || !File.Exists(_templatePath))
            {
                AddLog("❌ 模板文件未设置或不存在");
                return;
            }

            if (string.IsNullOrEmpty(_printerName))
            {
                AddLog("❌ 打印机未设置");
                return;
            }


            Format? format = null;
            Template? template = null;

            try
            {
                AddLog($"🔄 开始处理下料打印任务 ID: {unloadingRecord.Id}");

                // 1. 读取配置好的模版文件
                AddLog("📋 步骤1: 读取模版文件");
                _btApp ??= new MistakeProofing.Printing.BarTender.Application();
                format = _btApp.Formats.Open(_templatePath);

                // 2. 创建模板对象并识别属性
                AddLog("🔍 步骤2: 识别模版属性值");
                template = Template.Create(format, null);

                // 3. 使用模板绑定方式填充数据
                AddLog("📝 步骤3: 使用模板绑定填充数据");
                List<customproductattributes> customproductattributeslist = new List<customproductattributes>();
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "ProductERPCode", Value = "NewERPCode", });
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "ProductPartDescription", Value = "ProductPartDescription", });
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "Quantity", Value = "Quantity", });
                customproductattributeslist.Add(new customproductattributes() { ID = 1, ProductId = 1, Name = "BatchNumberCode", Value = "BatchNumberCode", });
                var label = new LabelP(format, template.DataBindings, customproductattributeslist);
                var printRecord = new FilmCoatingRecord
                {
                    Id = unloadingRecord.Id,
                    OriginalERPCode = unloadingRecord.ProductERPCode,
                    NewERPCode = unloadingRecord.ProductERPCode,
                    ProductERPCode = unloadingRecord.ProductERPCode,
                    Quantity = unloadingRecord.Quantity,
                    BatchNumber = unloadingRecord.BatchNumber,
                    ProductPartDescription = unloadingRecord.ProductPartDescription,
                    PrintTime = unloadingRecord.PrintTime,
                    PDADeviceId = "0",
                    CreatedTime = DateTime.Now,
                    Status = ""
                };

                label.DataSource = printRecord;

                // 4. 设置打印参数
                AddLog("🏷️ 步骤4: 设置打印参数");
                format.Printer = _printerName;
                format.IdenticalCopiesOfLabel = 1;
                format.NumberSerializedLabels = 1;

                // 5. 执行打印
                AddLog("🖨️ 步骤5: 执行打印");
                label.Print(1, 1); // 打印1份，序列化标签数为1

                AddLog($"✅ 打印任务完成: ID {unloadingRecord.Id}, ERP: {unloadingRecord.ProductERPCode}");

                // 更新状态
                UpdateUnLoadingPrintStatus(unloadingRecord.Id,1);
                UpdatePrintedCount(1);
            }
            catch (Exception ex)
            {
                AddLog($"❌ 打印处理失败: {ex.Message}");
                UpdateUnLoadingPrintStatus(unloadingRecord.Id,0);
            }
            finally
            {
                // 清理资源
                format?.Close(SaveOptionConstants.DoNotSaveChanges);
                // template?.Dispose();
            }
        }
        /// <summary>
        /// 命令打印
        /// </summary>
        /// <param name="printRecord"></param>
        /// <returns></returns>
        #region
        //private async Task ProcessPrintJob(FilmCoatingRecord printRecord)
        //{
        //    try
        //    {
        //        AddLog($"开始处理打印任务 ID: {printRecord.Id}");

        //        // 创建数据文件
        //        var dataFilePath = CreateDataFile(printRecord);

        //        if (!string.IsNullOrEmpty(dataFilePath) && File.Exists(dataFilePath))
        //        {
        //            // 执行打印
        //            bool printResult = await PrintLabel(printRecord, dataFilePath);

        //            if (printResult)
        //            {
        //                // 更新数据库状态
        //                UpdatePrintStatus(printRecord.Id, "已打印");

        //                AddLog($"✅ 打印成功: ID {printRecord.Id}, ERP: {printRecord.NewERPCode}");

        //                // 更新打印计数
        //                UpdatePrintedCount(1);
        //            }
        //            else
        //            {
        //                UpdatePrintStatus(printRecord.Id, "打印失败");

        //                AddLog($"❌ 打印失败: ID {printRecord.Id}, ERP: {printRecord.NewERPCode}");
        //            }

        //            // 清理临时文件
        //            try
        //            {
        //                if (File.Exists(dataFilePath))
        //                {
        //                    File.Delete(dataFilePath);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                AddLog($"⚠️ 清理临时文件失败: {ex.Message}");
        //            }
        //        }
        //        else
        //        {
        //            AddLog($"❌ 创建数据文件失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddLog($"❌ 处理打印任务时出错: {ex.Message}");

        //        // 更新状态为失败
        //        UpdatePrintStatus(printRecord.Id, "处理失败");
        //    }
        //}
        #endregion

        /// <summary>
        /// Excel 格式
        /// </summary>
        /// <param name="printRecord"></param>
        /// <returns></returns>
        #region
        //private string CreateDataFile(FilmCoatingRecord printRecord)
        //{
        //    try
        //    {
        //        // 创建一个临时Excel文件
        //        string excelPath = Path.Combine(
        //            Path.GetDirectoryName(_templatePath),
        //            "tempdata11.xlsx");

        //        AddLog($"📁 创建Excel文件: {excelPath}");

        //        // 如果文件已存在，先删除
        //        if (File.Exists(excelPath))
        //        {
        //            File.Delete(excelPath);
        //        }

        //        // 创建Excel工作簿
        //        using (var workbook = new XLWorkbook())
        //        {
        //            // 添加工作表，名称可能是"Sheet1"（常见默认名称）
        //            var worksheet = workbook.Worksheets.Add("Sheet1");

        //            // 写入表头 - 必须与模板中配置的数据库字段名完全匹配
        //            // 根据之前的错误信息，模板中可能有以下字段：
        //            worksheet.Cell("A1").Value = "erp号";           // 列A: erp号
        //            worksheet.Cell("B1").Value = "产品描述";       // 列B: 产品描述
        //            worksheet.Cell("C1").Value = "数量";           // 列C: 数量
        //            worksheet.Cell("D1").Value = "批次";           // 列D: 批次
        //            worksheet.Cell("E1").Value = "供应商";         // 列E: 供应商
        //            worksheet.Cell("F1").Value = "打印人";         // 列F: 打印人
        //            worksheet.Cell("G1").Value = "打印时间";       // 列G: 打印时间
        //            worksheet.Cell("H1").Value = "二维码";         // 列H: 二维码

        //            // 写入数据（第2行）
        //            worksheet.Cell("A2").Value = printRecord.NewERPCode ?? "TEST001";
        //            worksheet.Cell("B2").Value = printRecord.ProductPartDescription ?? "测试产品描述";
        //            worksheet.Cell("C2").Value = printRecord.Quantity;
        //            worksheet.Cell("D2").Value = printRecord.BatchNumber ?? "BATCH001";
        //            worksheet.Cell("E2").Value = "默认供应商";
        //            worksheet.Cell("F2").Value = printRecord.PDADeviceId ?? "System";
        //            worksheet.Cell("G2").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //            worksheet.Cell("H2").Value = GenerateQRCode(printRecord);

        //            // 保存Excel文件
        //            workbook.SaveAs(excelPath);
        //        }

        //        AddLog($"✅ Excel文件创建成功");
        //        AddLog($"📄 Excel数据行内容:");
        //        AddLog($"  A1(erp号): {printRecord.NewERPCode}");
        //        AddLog($"  B1(产品描述): {printRecord.ProductPartDescription}");
        //        AddLog($"  C1(数量): {printRecord.Quantity}");
        //        AddLog($"  D1(批次): {printRecord.BatchNumber}");
        //        AddLog($"  E1(供应商): 默认供应商");
        //        AddLog($"  F1(打印人): {printRecord.PDADeviceId}");
        //        AddLog($"  G1(打印时间): {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        //        AddLog($"  H1(二维码): {GenerateQRCode(printRecord)}");

        //        return excelPath;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddLog($"❌ 创建Excel文件时出错: {ex.Message}");

        //        // 如果创建Excel失败，尝试回退到CSV
        //        AddLog("🔄 尝试回退到CSV格式...");
        //        return CreateCsvDataFile(printRecord);
        //    }
        //}
        //private string CreateCsvDataFile(FilmCoatingRecord printRecord)
        //{
        //    try
        //    {
        //        string csvPath = Path.GetTempFileName() + ".csv";
        //        var csvContent = new StringBuilder();

        //        // 使用与Excel相同的字段名
        //        csvContent.AppendLine("erp号,产品描述,数量,批次,供应商,打印人,打印时间,二维码");
        //        csvContent.AppendLine($"\"{printRecord.NewERPCode}\",\"{printRecord.ProductPartDescription}\",\"{printRecord.Quantity}\",\"{printRecord.BatchNumber}\",\"默认供应商\",\"{printRecord.PDADeviceId}\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",\"{GenerateQRCode(printRecord)}\"");

        //        File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);

        //        AddLog($"📁 回退到CSV文件: {csvPath}");
        //        AddLog($"📄 CSV内容:\n{csvContent}");

        //        return csvPath;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddLog($"❌ 创建CSV文件时出错: {ex.Message}");
        //        return null;
        //    }
        //}
        //private string BuildCommandLineArguments(string dataFilePath)
        //{
        //    var args = new StringBuilder();

        //    // 基本参数
        //    args.Append($"/AF=\"{_templatePath}\" ");

        //    // 根据文件类型使用正确的参数
        //    var extension = Path.GetExtension(dataFilePath).ToLower();

        //    if (extension == ".xlsx" || extension == ".xls")
        //    {
        //        // Excel文件
        //        args.Append($"/F=\"{dataFilePath}\" ");
        //    }
        //    else
        //    {
        //        // CSV或其他数据文件
        //        args.Append($"/D=\"{dataFilePath}\" ");
        //    }

        //    args.Append($"/PRN=\"{_printerName}\" ");
        //    args.Append($"/C=1 ");
        //    args.Append($"/P ");
        //    args.Append($"/X");

        //    return args.ToString().Trim();
        //}
        #endregion
        /// <summary>
        /// temp 文件格式
        /// </summary>
        /// <param name="printRecord"></param>
        /// <returns></returns>
        #region
        private string CreateDataFile(FilmCoatingRecord printRecord)
        {
            try
            {
                string tempFilePath = Path.GetTempFileName();
                var csvContent = new StringBuilder();

                // 修正表头 - 使用中文字段名
                // 根据错误信息，模板期望"供应商"字段
                csvContent.AppendLine("ERPCode,ProductPartDescription,Quantity,BatchNumberCode,PrintedBy,PrintTime,供应商名称,供应商");

                // 数据行
                var dataLine = $"\"{EscapeCsv(printRecord.NewERPCode ?? "")}\"," +
                              $"\"{EscapeCsv(printRecord.ProductPartDescription ?? "")}\"," +
                              $"\"{EscapeCsv(printRecord.Quantity.ToString() ?? "0")}\"," +
                              $"\"{EscapeCsv(printRecord.BatchNumber ?? "")}\"," +
                              $"\"{EscapeCsv(GenerateQRCode(printRecord))}\"," +
                              $"\"{EscapeCsv(printRecord.PDADeviceId ?? "System")}\"," +
                              $"\"{EscapeCsv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}\"," +
                               $"\"{EscapeCsv("供应商名称")}\"," +
                              $"\"{EscapeCsv("默认供应商")}\"";  // 提供供应商值

                csvContent.AppendLine(dataLine);
                File.WriteAllText(tempFilePath, csvContent.ToString(), Encoding.Unicode);

                // 添加调试日志
                AddLog($"📁 CSV内容:\n{csvContent.ToString()}");

                return tempFilePath;
            }
            catch (Exception ex)
            {
                AddLog($"❌ 创建数据文件时出错: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// temp 格式
        /// </summary>
        /// <param name="dataFilePath"></param>
        /// <returns></returns>
        private string BuildCommandLineArguments(string dataFilePath)
        {
            var args = new StringBuilder();

            // 只使用支持的参数
            args.Append($"/F=\"{_templatePath}\" ");
            args.Append($"/D=\"{dataFilePath}\" ");
            args.Append($"/PRN=\"{_printerName}\" ");
            args.Append($"/C=1 ");
            args.Append($"/P ");
            args.Append($"/Minimized ");  // 只保留这个最小化参数
            args.Append($"/X");

            return args.ToString().Trim();
        }
        #endregion

        private string GenerateQRCode(FilmCoatingRecord printRecord)
        {
            // 生成二维码内容
            return $"ERP:{printRecord.NewERPCode}|BATCH:{printRecord.BatchNumber}|QTY:{printRecord.Quantity}";
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // 转义CSV中的特殊字符
            if (input.Contains("\""))
            {
                return input.Replace("\"", "\"\"");
            }

            return input;
        }

        private async Task<bool> PrintLabel(FilmCoatingRecord printRecord, string dataFilePath)
        {
            try
            {
                // 检查BarTender路径
                if (!File.Exists(_barTenderPath))
                {
                    AddLog($"❌ BarTender路径不存在: {_barTenderPath}");
                    return false;
                }

                // 检查模板文件
                if (!File.Exists(_templatePath))
                {
                    AddLog($"❌ 模板文件不存在: {_templatePath}");
                    return false;
                }

                // 构建命令行参数
                var arguments = BuildCommandLineArguments(dataFilePath);

                AddLog($"🖨️ 执行打印命令: {_barTenderPath} {arguments}");

                // 执行打印命令
                var processInfo = new ProcessStartInfo
                {
                    FileName = _barTenderPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    // 读取输出
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    // 等待进程结束

                    await Task.Run(() => process.WaitForExit(30000)); // 30秒超时

                    if (!string.IsNullOrEmpty(output))
                    {
                        AddLog($"📄 BarTender输出: {output}");
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        AddLog($"⚠️ BarTender错误: {error}");
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 执行打印命令时出错: {ex.Message}");
                return false;
            }
        }
        
        
        private void UpdatePrintStatus(int recordId, string status)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var updateQuery = @"
                        UPDATE FilmCoatingRecords 
                        SET Status = @Status, 
                            PrintTime = @PrintTime
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@PrintTime", DateTime.Now);
                        command.Parameters.AddWithValue("@Id", recordId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 更新数据库状态失败: {ex.Message}");
            }
        }

        private void UpdateUnLoadingPrintStatus(int recordId, int status)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var updateQuery = @"
                        UPDATE UnloadingRecords 
                        SET PrintStatus = @Status, 
                            PrintTime = @PrintTime
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@PrintTime", DateTime.Now);
                        command.Parameters.AddWithValue("@Id", recordId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"❌ 更新数据库状态失败: {ex.Message}");
            }
        }

        private void LoadPrintedCount()
        {
            try
            {
                if (File.Exists("printcount.txt"))
                {
                    string countText = File.ReadAllText("printcount.txt");
                    if (int.TryParse(countText, out _printedCount))
                    {
                        UpdatePrintedCountDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"⚠️ 加载打印计数失败: {ex.Message}");
            }
        }

        private void UpdatePrintedCount(int increment = 0)
        {
            _printedCount += increment;
            UpdatePrintedCountDisplay();
            SavePrintedCount();
        }

        private void UpdatePrintedCountDisplay()
        {
            if (txtTotalPrinted.InvokeRequired)
            {
                txtTotalPrinted.Invoke((MethodInvoker)delegate
                {
                    txtTotalPrinted.Text = _printedCount.ToString();
                });
            }
            else
            {
                txtTotalPrinted.Text = _printedCount.ToString();
            }
        }

        private void SavePrintedCount()
        {
            try
            {
                File.WriteAllText("printcount.txt", _printedCount.ToString());
            }
            catch (Exception ex)
            {
                AddLog($"⚠️ 保存打印计数失败: {ex.Message}");
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate
                {
                    AddLogInternal(message);
                });
            }
            else
            {
                AddLogInternal(message);
            }
        }

        private void AddLogInternal(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
            txtLog.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 停止定时器
            if (_isPolling)
            {
                _pollingTimer.Stop();
            }

            // 保存打印计数
            SavePrintedCount();

            base.OnFormClosing(e);
        }
    }
}