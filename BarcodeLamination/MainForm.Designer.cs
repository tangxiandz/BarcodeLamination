namespace BarcodeLaminationPrint
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 650);  // 增加窗体大小
            this.Text = "条码覆膜/下料标签打印服务";  // 修改标题
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // 创建控件并设置位置和大小
            CreateControls();

            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
        }

        private void CreateControls()
        {
            int yPos = 20;  // 垂直位置起始点
            int xPos = 20;  // 水平位置起始点
            int controlWidth = 200;  // 控件宽度
            int buttonWidth = 120;   // 按钮宽度
            int buttonHeight = 35;   // 按钮高度
            int labelHeight = 25;    // 标签高度

            // 状态标签
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatus.Location = new System.Drawing.Point(xPos, yPos);
            this.lblStatus.Size = new System.Drawing.Size(controlWidth, labelHeight);
            this.lblStatus.Text = "服务状态: 已停止";
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei", 10, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblStatus);

            // 自动启动复选框
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkAutoStart.Location = new System.Drawing.Point(xPos + 220, yPos);
            this.chkAutoStart.Size = new System.Drawing.Size(120, labelHeight);
            this.chkAutoStart.Text = "自动启动";
            this.chkAutoStart.Checked = true;
            this.Controls.Add(this.chkAutoStart);

            // 最后检查时间标签
            this.lblLastCheck = new System.Windows.Forms.Label();
            this.lblLastCheck.Location = new System.Drawing.Point(xPos + 360, yPos);
            this.lblLastCheck.Size = new System.Drawing.Size(100, labelHeight);
            this.lblLastCheck.Text = "最后检查:";
            this.lblLastCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(this.lblLastCheck);

            // 最后检查时间文本框
            this.txtLastCheck = new System.Windows.Forms.TextBox();
            this.txtLastCheck.Location = new System.Drawing.Point(xPos + 470, yPos);
            this.txtLastCheck.Size = new System.Drawing.Size(120, labelHeight);
            this.txtLastCheck.ReadOnly = true;
            this.txtLastCheck.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Controls.Add(this.txtLastCheck);

            // 总打印数标签
            this.lblTotalPrinted = new System.Windows.Forms.Label();
            this.lblTotalPrinted.Location = new System.Drawing.Point(xPos + 610, yPos);
            this.lblTotalPrinted.Size = new System.Drawing.Size(80, labelHeight);
            this.lblTotalPrinted.Text = "打印总数:";
            this.lblTotalPrinted.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(this.lblTotalPrinted);

            // 总打印数文本框
            this.txtTotalPrinted = new System.Windows.Forms.TextBox();
            this.txtTotalPrinted.Location = new System.Drawing.Point(xPos + 700, yPos);
            this.txtTotalPrinted.Size = new System.Drawing.Size(60, labelHeight);
            this.txtTotalPrinted.ReadOnly = true;
            this.txtTotalPrinted.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Controls.Add(this.txtTotalPrinted);

            yPos += 40;  // 下移一行

            // 第一行按钮
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStart.Location = new System.Drawing.Point(xPos, yPos);
            this.btnStart.Size = new System.Drawing.Size(buttonWidth, buttonHeight);
            this.btnStart.Text = "启动(F1)";
            this.btnStart.BackColor = System.Drawing.Color.LightGreen;
            this.btnStart.Click += new System.EventHandler(this.StartButton_Click);
            this.Controls.Add(this.btnStart);

            this.btnStop = new System.Windows.Forms.Button();
            this.btnStop.Location = new System.Drawing.Point(xPos + buttonWidth + 10, yPos);
            this.btnStop.Size = new System.Drawing.Size(buttonWidth, buttonHeight);
            this.btnStop.Text = "停止(F2)";
            this.btnStop.BackColor = System.Drawing.Color.LightCoral;
            this.btnStop.Enabled = false;
            this.btnStop.Click += new System.EventHandler(this.StopButton_Click);
            this.Controls.Add(this.btnStop);

            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnTestConnection.Location = new System.Drawing.Point(xPos + (buttonWidth + 10) * 2, yPos);
            this.btnTestConnection.Size = new System.Drawing.Size(140, buttonHeight);
            this.btnTestConnection.Text = "测试数据库连接";
            this.btnTestConnection.Click += new System.EventHandler(this.TestConnectionButton_Click);
            this.Controls.Add(this.btnTestConnection);

            this.btnTestPrint = new System.Windows.Forms.Button();
            this.btnTestPrint.Location = new System.Drawing.Point(xPos + (buttonWidth + 10) * 2 + 150, yPos);
            this.btnTestPrint.Size = new System.Drawing.Size(buttonWidth, buttonHeight);
            this.btnTestPrint.Text = "测试打印";
            this.btnTestPrint.Click += new System.EventHandler(this.TestPrintButton_Click);
            this.Controls.Add(this.btnTestPrint);

            yPos += 45;  // 下移一行

            // 第二行按钮
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnClearLog.Location = new System.Drawing.Point(xPos, yPos);
            this.btnClearLog.Size = new System.Drawing.Size(140, buttonHeight);
            this.btnClearLog.Text = "清空日志(F5)";
            this.btnClearLog.Click += new System.EventHandler(this.ClearLogButton_Click);
            this.Controls.Add(this.btnClearLog);

            this.btnSelectPrinter = new System.Windows.Forms.Button();
            this.btnSelectPrinter.Location = new System.Drawing.Point(xPos + 150, yPos);
            this.btnSelectPrinter.Size = new System.Drawing.Size(140, buttonHeight);
            this.btnSelectPrinter.Text = "选择打印机";
            this.btnSelectPrinter.Click += new System.EventHandler(this.btnSelectPrinter_Click);
            this.Controls.Add(this.btnSelectPrinter);

            this.btnUploadTemplate = new System.Windows.Forms.Button();
            this.btnUploadTemplate.Location = new System.Drawing.Point(xPos + 300, yPos);
            this.btnUploadTemplate.Size = new System.Drawing.Size(140, buttonHeight);
            this.btnUploadTemplate.Text = "上传模板";
            this.btnUploadTemplate.Click += new System.EventHandler(this.btnUploadTemplate_Click);
            this.Controls.Add(this.btnUploadTemplate);

            yPos += 45;  // 下移一行

            // 打印类型分组框
            this.groupBoxPrintType = new System.Windows.Forms.GroupBox();
            this.groupBoxPrintType.Location = new System.Drawing.Point(xPos, yPos);
            this.groupBoxPrintType.Size = new System.Drawing.Size(300, 60);
            this.groupBoxPrintType.Text = "打印类型";
            this.Controls.Add(this.groupBoxPrintType);

            // 覆膜打印单选按钮
            this.rdoFilmCoating = new System.Windows.Forms.RadioButton();
            this.rdoFilmCoating.Location = new System.Drawing.Point(15, 25);
            this.rdoFilmCoating.Size = new System.Drawing.Size(120, 25);
            this.rdoFilmCoating.Text = "覆膜打印";
            this.rdoFilmCoating.Checked = true;
            this.rdoFilmCoating.CheckedChanged += new System.EventHandler(this.PrintType_CheckedChanged);
            this.groupBoxPrintType.Controls.Add(this.rdoFilmCoating);

            // 下料打印单选按钮
            this.rdoUnloading = new System.Windows.Forms.RadioButton();
            this.rdoUnloading.Location = new System.Drawing.Point(150, 25);
            this.rdoUnloading.Size = new System.Drawing.Size(120, 25);
            this.rdoUnloading.Text = "下料打印";
            this.rdoUnloading.CheckedChanged += new System.EventHandler(this.PrintType_CheckedChanged);
            this.groupBoxPrintType.Controls.Add(this.rdoUnloading);

            yPos += 70;  // 下移一行

            // 当前打印机标签
            this.lblCurrentPrinter = new System.Windows.Forms.Label();
            this.lblCurrentPrinter.Location = new System.Drawing.Point(xPos, yPos);
            this.lblCurrentPrinter.Size = new System.Drawing.Size(400, labelHeight);
            this.lblCurrentPrinter.Text = "当前打印机: 未设置";
            this.lblCurrentPrinter.Font = new System.Drawing.Font("Microsoft YaHei", 9, System.Drawing.FontStyle.Regular);
            this.Controls.Add(this.lblCurrentPrinter);

            yPos += 30;  // 下移一行

            // 当前模板标签
            this.lblCurrentTemplate = new System.Windows.Forms.Label();
            this.lblCurrentTemplate.Location = new System.Drawing.Point(xPos, yPos);
            this.lblCurrentTemplate.Size = new System.Drawing.Size(400, labelHeight);
            this.lblCurrentTemplate.Text = "当前模板: 未设置";
            this.lblCurrentTemplate.Font = new System.Drawing.Font("Microsoft YaHei", 9, System.Drawing.FontStyle.Regular);
            this.Controls.Add(this.lblCurrentTemplate);

            yPos += 40;  // 下移一行

            // 日志标签
            this.lblLog = new System.Windows.Forms.Label();
            this.lblLog.Location = new System.Drawing.Point(xPos, yPos);
            this.lblLog.Size = new System.Drawing.Size(200, labelHeight);
            this.lblLog.Text = "操作日志:";
            this.lblLog.Font = new System.Drawing.Font("Microsoft YaHei", 10, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblLog);

            yPos += 30;  // 下移一行

            // 日志文本框
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtLog.Location = new System.Drawing.Point(xPos, yPos);
            this.txtLog.Size = new System.Drawing.Size(860, 380);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9);
            this.Controls.Add(this.txtLog);
        }

        #endregion

        // 控件声明
        private Label lblStatus;
        private CheckBox chkAutoStart;
        private Label lblLastCheck;
        private TextBox txtLastCheck;
        private Label lblTotalPrinted;
        private TextBox txtTotalPrinted;
        private Button btnStart;
        private Button btnStop;
        private Button btnTestConnection;
        private Button btnTestPrint;
        private Button btnClearLog;
        private Label lblLog;
        private TextBox txtLog;
        private System.Windows.Forms.Button btnSelectPrinter;
        private System.Windows.Forms.Button btnUploadTemplate;
        private System.Windows.Forms.Label lblCurrentPrinter;
        private System.Windows.Forms.Label lblCurrentTemplate;
        // 新增的打印类型控件
        private RadioButton rdoFilmCoating;
        private RadioButton rdoUnloading;
        private GroupBox groupBoxPrintType;
    }
}