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
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "条码覆膜标签打印服务";
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
            // 状态标签
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatus.Location = new System.Drawing.Point(20, 20);
            this.lblStatus.Size = new System.Drawing.Size(200, 25);
            this.lblStatus.Text = "服务状态: 已停止";
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei", 10, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblStatus);

            // 自动启动复选框
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkAutoStart.Location = new System.Drawing.Point(250, 20);
            this.chkAutoStart.Size = new System.Drawing.Size(120, 25);
            this.chkAutoStart.Text = "自动启动";
            this.chkAutoStart.Checked = true;
            this.Controls.Add(this.chkAutoStart);

            // 最后检查时间标签
            this.lblLastCheck = new System.Windows.Forms.Label();
            this.lblLastCheck.Location = new System.Drawing.Point(400, 20);
            this.lblLastCheck.Size = new System.Drawing.Size(100, 25);
            this.lblLastCheck.Text = "最后检查:";
            this.lblLastCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(this.lblLastCheck);

            // 最后检查时间文本框
            this.txtLastCheck = new System.Windows.Forms.TextBox();
            this.txtLastCheck.Location = new System.Drawing.Point(510, 20);
            this.txtLastCheck.Size = new System.Drawing.Size(120, 25);
            this.txtLastCheck.ReadOnly = true;
            this.txtLastCheck.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Controls.Add(this.txtLastCheck);

            // 总打印数标签
            this.lblTotalPrinted = new System.Windows.Forms.Label();
            this.lblTotalPrinted.Location = new System.Drawing.Point(650, 20);
            this.lblTotalPrinted.Size = new System.Drawing.Size(80, 25);
            this.lblTotalPrinted.Text = "打印总数:";
            this.lblTotalPrinted.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Controls.Add(this.lblTotalPrinted);

            // 总打印数文本框
            this.txtTotalPrinted = new System.Windows.Forms.TextBox();
            this.txtTotalPrinted.Location = new System.Drawing.Point(740, 20);
            this.txtTotalPrinted.Size = new System.Drawing.Size(40, 25);
            this.txtTotalPrinted.ReadOnly = true;
            this.txtTotalPrinted.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Controls.Add(this.txtTotalPrinted);

            // 启动按钮
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStart.Location = new System.Drawing.Point(20, 60);
            this.btnStart.Size = new System.Drawing.Size(100, 35);
            this.btnStart.Text = "启动(F1)";
            this.btnStart.BackColor = System.Drawing.Color.LightGreen;
            this.btnStart.Click += new System.EventHandler(this.StartButton_Click);
            this.Controls.Add(this.btnStart);

            // 停止按钮
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStop.Location = new System.Drawing.Point(130, 60);
            this.btnStop.Size = new System.Drawing.Size(100, 35);
            this.btnStop.Text = "停止(F2)";
            this.btnStop.BackColor = System.Drawing.Color.LightCoral;
            this.btnStop.Enabled = false;
            this.btnStop.Click += new System.EventHandler(this.StopButton_Click);
            this.Controls.Add(this.btnStop);

            // 测试数据库连接按钮
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnTestConnection.Location = new System.Drawing.Point(240, 60);
            this.btnTestConnection.Size = new System.Drawing.Size(120, 35);
            this.btnTestConnection.Text = "测试数据库连接";
            this.btnTestConnection.Click += new System.EventHandler(this.TestConnectionButton_Click);
            this.Controls.Add(this.btnTestConnection);

            // 测试打印按钮
            this.btnTestPrint = new System.Windows.Forms.Button();
            this.btnTestPrint.Location = new System.Drawing.Point(370, 60);
            this.btnTestPrint.Size = new System.Drawing.Size(100, 35);
            this.btnTestPrint.Text = "测试打印";
            this.btnTestPrint.Click += new System.EventHandler(this.TestPrintButton_Click);
            this.Controls.Add(this.btnTestPrint);

            // 清空日志按钮
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnClearLog.Location = new System.Drawing.Point(480, 60);
            this.btnClearLog.Size = new System.Drawing.Size(100, 35);
            this.btnClearLog.Text = "清空日志(F5)";
            this.btnClearLog.Click += new System.EventHandler(this.ClearLogButton_Click);
            this.Controls.Add(this.btnClearLog);

            // 选择打印机按钮
            this.btnSelectPrinter = new System.Windows.Forms.Button();
            this.btnSelectPrinter.Location = new System.Drawing.Point(590, 60);
            this.btnSelectPrinter.Size = new System.Drawing.Size(100, 35);
            this.btnSelectPrinter.Text = "选择打印机";
            this.btnSelectPrinter.Click += new System.EventHandler(this.btnSelectPrinter_Click);
            this.Controls.Add(this.btnSelectPrinter);

            // 上传模板按钮
            this.btnUploadTemplate = new System.Windows.Forms.Button();
            this.btnUploadTemplate.Location = new System.Drawing.Point(700, 60);
            this.btnUploadTemplate.Size = new System.Drawing.Size(100, 35);
            this.btnUploadTemplate.Text = "上传模板";
            this.btnUploadTemplate.Visible = false;
            this.btnUploadTemplate.Click += new System.EventHandler(this.btnUploadTemplate_Click);
            this.Controls.Add(this.btnUploadTemplate);

            // 当前打印机标签
            this.lblCurrentPrinter = new System.Windows.Forms.Label();
            this.lblCurrentPrinter.Location = new System.Drawing.Point(20, 110);
            this.lblCurrentPrinter.Size = new System.Drawing.Size(300, 25);
            this.lblCurrentPrinter.Text = "当前打印机: " + (Properties.Settings.Default.Printer1 ?? "未设置");
            this.lblCurrentPrinter.Font = new System.Drawing.Font("Microsoft YaHei", 9, System.Drawing.FontStyle.Regular);
            this.Controls.Add(this.lblCurrentPrinter);

            // 当前模板标签
            this.lblCurrentTemplate = new System.Windows.Forms.Label();
            this.lblCurrentTemplate.Location = new System.Drawing.Point(350, 110);
            this.lblCurrentTemplate.Size = new System.Drawing.Size(450, 25);
            this.lblCurrentTemplate.Text = "当前模板: " + (GetCurrentTemplateName() ?? "未设置");
            this.lblCurrentTemplate.Font = new System.Drawing.Font("Microsoft YaHei", 9, System.Drawing.FontStyle.Regular);
            this.Controls.Add(this.lblCurrentTemplate);

            // 日志标签
            this.lblLog = new System.Windows.Forms.Label();
            this.lblLog.Location = new System.Drawing.Point(20, 140);
            this.lblLog.Size = new System.Drawing.Size(200, 25);
            this.lblLog.Text = "操作日志:";
            this.lblLog.Font = new System.Drawing.Font("Microsoft YaHei", 10, System.Drawing.FontStyle.Bold);
            this.Controls.Add(this.lblLog);

            // 日志文本框
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtLog.Location = new System.Drawing.Point(20, 170);
            this.txtLog.Size = new System.Drawing.Size(760, 380);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9);
            this.Controls.Add(this.txtLog);

            // 窗体设置
            this.ClientSize = new System.Drawing.Size(820, 580);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "条码覆膜标签打印服务";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);

            // 调整窗体大小以适应控件
            this.ClientSize = new Size(820, 580);
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
    }
}