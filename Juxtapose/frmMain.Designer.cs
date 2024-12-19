namespace Juxtapose
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            panel1 = new Panel();
            Extentions = new Label();
            txtExtensions = new TextBox();
            label4 = new Label();
            txtSVN = new TextBox();
            label3 = new Label();
            txtWinmerge = new TextBox();
            btnLoadSVN = new Button();
            label5 = new Label();
            txtSVNRoot = new TextBox();
            txtRight = new TextBox();
            txtLeft = new TextBox();
            treeRight = new TreeView();
            treeLeft = new TreeView();
            chkAutoScroll = new CheckBox();
            btnAnalyze = new Button();
            progressBar1 = new ProgressBar();
            gridView = new DataGridView();
            txtLog = new TextBox();
            toolTip1 = new ToolTip(components);
            btnBackup = new Button();
            btnImport = new Button();
            chkAdded = new CheckBox();
            chkDeleted = new CheckBox();
            chkModified = new CheckBox();
            chkIdentical = new CheckBox();
            chkMoved = new CheckBox();
            btnFilter = new Button();
            drpBase = new ComboBox();
            chkSVNUpdate = new CheckBox();
            drpUser = new ComboBox();
            imageList1 = new ImageList(components);
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox1 = new GroupBox();
            tabPage2 = new TabPage();
            label1 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridView).BeginInit();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(Extentions);
            panel1.Controls.Add(txtExtensions);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(txtSVN);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txtWinmerge);
            panel1.Location = new Point(1399, 6);
            panel1.Name = "panel1";
            panel1.Size = new Size(484, 344);
            panel1.TabIndex = 0;
            // 
            // Extentions
            // 
            Extentions.AutoSize = true;
            Extentions.Location = new Point(21, 109);
            Extentions.Name = "Extentions";
            Extentions.Size = new Size(58, 15);
            Extentions.TabIndex = 11;
            Extentions.Text = "File Types";
            // 
            // txtExtensions
            // 
            txtExtensions.Location = new Point(97, 106);
            txtExtensions.Name = "txtExtensions";
            txtExtensions.Size = new Size(376, 23);
            txtExtensions.TabIndex = 4;
            txtExtensions.Text = "java";
            toolTip1.SetToolTip(txtExtensions, "Specify comma separated list of file extenstions to use. i.e. java,ts");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(21, 63);
            label4.Name = "label4";
            label4.Size = new Size(71, 15);
            label4.TabIndex = 9;
            label4.Text = "TortoiseSVN";
            // 
            // txtSVN
            // 
            txtSVN.Location = new Point(97, 57);
            txtSVN.Name = "txtSVN";
            txtSVN.Size = new Size(376, 23);
            txtSVN.TabIndex = 3;
            txtSVN.Text = "C:\\Program Files\\TortoiseSVN";
            toolTip1.SetToolTip(txtSVN, "Path to TortoiseSVN. i.e. C:\\Program Files\\TortoiseSVN");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(21, 17);
            label3.Name = "label3";
            label3.Size = new Size(62, 15);
            label3.TabIndex = 7;
            label3.Text = "WinMerge";
            // 
            // txtWinmerge
            // 
            txtWinmerge.Location = new Point(97, 14);
            txtWinmerge.Name = "txtWinmerge";
            txtWinmerge.Size = new Size(376, 23);
            txtWinmerge.TabIndex = 2;
            txtWinmerge.Text = "C:\\Program Files\\WinMerge";
            toolTip1.SetToolTip(txtWinmerge, "Path to WinMerge directory. i.e. C:\\Program Files\\WinMerge");
            // 
            // btnLoadSVN
            // 
            btnLoadSVN.Location = new Point(328, 21);
            btnLoadSVN.Name = "btnLoadSVN";
            btnLoadSVN.Size = new Size(127, 37);
            btnLoadSVN.TabIndex = 15;
            btnLoadSVN.Text = "Load";
            toolTip1.SetToolTip(btnLoadSVN, "Load SVN hierarchy");
            btnLoadSVN.UseVisualStyleBackColor = true;
            btnLoadSVN.Click += btnLoadSVN_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 32);
            label5.Name = "label5";
            label5.Size = new Size(57, 15);
            label5.TabIndex = 14;
            label5.Text = "SVN Root";
            // 
            // txtSVNRoot
            // 
            txtSVNRoot.Location = new Point(99, 29);
            txtSVNRoot.Name = "txtSVNRoot";
            txtSVNRoot.Size = new Size(223, 23);
            txtSVNRoot.TabIndex = 13;
            txtSVNRoot.Text = "svn://svnserver";
            toolTip1.SetToolTip(txtSVNRoot, "SVN root path. i.e. svn://svnserver");
            // 
            // txtRight
            // 
            txtRight.Location = new Point(692, 321);
            txtRight.Name = "txtRight";
            txtRight.Size = new Size(692, 23);
            txtRight.TabIndex = 1;
            toolTip1.SetToolTip(txtRight, "Path of the right branch to analyze");
            // 
            // txtLeft
            // 
            txtLeft.Location = new Point(6, 321);
            txtLeft.Name = "txtLeft";
            txtLeft.Size = new Size(657, 23);
            txtLeft.TabIndex = 0;
            toolTip1.SetToolTip(txtLeft, "Path of the left branch to analyze");
            txtLeft.TextChanged += txtLeft_TextChanged;
            // 
            // treeRight
            // 
            treeRight.Location = new Point(692, 70);
            treeRight.Name = "treeRight";
            treeRight.Size = new Size(692, 245);
            treeRight.TabIndex = 16;
            toolTip1.SetToolTip(treeRight, "Select right (new) version");
            treeRight.AfterSelect += treeRight_AfterSelect;
            // 
            // treeLeft
            // 
            treeLeft.Location = new Point(6, 70);
            treeLeft.Name = "treeLeft";
            treeLeft.Size = new Size(657, 245);
            treeLeft.TabIndex = 15;
            toolTip1.SetToolTip(treeLeft, "Select left (old) version");
            treeLeft.AfterSelect += treeLeft_AfterSelect;
            // 
            // chkAutoScroll
            // 
            chkAutoScroll.AutoSize = true;
            chkAutoScroll.Checked = true;
            chkAutoScroll.CheckState = CheckState.Checked;
            chkAutoScroll.Location = new Point(1800, 18);
            chkAutoScroll.Name = "chkAutoScroll";
            chkAutoScroll.Size = new Size(83, 19);
            chkAutoScroll.TabIndex = 12;
            chkAutoScroll.Text = "Auto scroll";
            toolTip1.SetToolTip(chkAutoScroll, "Auto scroll the grid view to bottom upon adding a new line");
            chkAutoScroll.UseVisualStyleBackColor = true;
            // 
            // btnAnalyze
            // 
            btnAnalyze.Location = new Point(527, 362);
            btnAnalyze.Name = "btnAnalyze";
            btnAnalyze.Size = new Size(139, 37);
            btnAnalyze.TabIndex = 5;
            btnAnalyze.Text = "Analyze";
            toolTip1.SetToolTip(btnAnalyze, "Start analysis process");
            btnAnalyze.UseVisualStyleBackColor = true;
            btnAnalyze.Click += btnAnalyze_Click_1;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(695, 362);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(1188, 37);
            progressBar1.TabIndex = 5;
            toolTip1.SetToolTip(progressBar1, "Analysis prgress");
            // 
            // gridView
            // 
            gridView.AllowUserToAddRows = false;
            gridView.AllowUserToDeleteRows = false;
            gridView.AllowUserToResizeRows = false;
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridView.BackgroundColor = Color.FromArgb(64, 64, 64);
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridView.Location = new Point(6, 51);
            gridView.Name = "gridView";
            gridView.ReadOnly = true;
            gridView.RowHeadersVisible = false;
            gridView.RowTemplate.Height = 25;
            gridView.Size = new Size(1877, 900);
            gridView.TabIndex = 2;
            gridView.Tag = "Detail";
            gridView.CellDoubleClick += gridView_CellDoubleClick;
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.FromArgb(64, 64, 64);
            txtLog.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            txtLog.ForeColor = Color.Lime;
            txtLog.Location = new Point(-4, 408);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Both;
            txtLog.Size = new Size(1887, 634);
            txtLog.TabIndex = 3;
            toolTip1.SetToolTip(txtLog, "Log");
            // 
            // toolTip1
            // 
            toolTip1.ToolTipIcon = ToolTipIcon.Info;
            toolTip1.ToolTipTitle = "Juxtapose";
            // 
            // btnBackup
            // 
            btnBackup.Location = new Point(1667, 8);
            btnBackup.Name = "btnBackup";
            btnBackup.Size = new Size(127, 37);
            btnBackup.TabIndex = 16;
            btnBackup.Text = "Backup...";
            toolTip1.SetToolTip(btnBackup, "Backup difference  analysis to a TSV file");
            btnBackup.UseVisualStyleBackColor = true;
            btnBackup.Click += btnBackup_Click;
            // 
            // btnImport
            // 
            btnImport.Location = new Point(1534, 8);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(127, 37);
            btnImport.TabIndex = 17;
            btnImport.Text = "Restore...";
            toolTip1.SetToolTip(btnImport, "Restore difference  analysis from an existing TSV file");
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // chkAdded
            // 
            chkAdded.AutoSize = true;
            chkAdded.Checked = true;
            chkAdded.CheckState = CheckState.Checked;
            chkAdded.Location = new Point(6, 18);
            chkAdded.Name = "chkAdded";
            chkAdded.Size = new Size(64, 19);
            chkAdded.TabIndex = 18;
            chkAdded.Text = "ADDED";
            toolTip1.SetToolTip(chkAdded, "Include records of status 'ADDED'");
            chkAdded.UseVisualStyleBackColor = true;
            // 
            // chkDeleted
            // 
            chkDeleted.AutoSize = true;
            chkDeleted.Checked = true;
            chkDeleted.CheckState = CheckState.Checked;
            chkDeleted.Location = new Point(89, 18);
            chkDeleted.Name = "chkDeleted";
            chkDeleted.Size = new Size(73, 19);
            chkDeleted.TabIndex = 19;
            chkDeleted.Text = "DELETED";
            toolTip1.SetToolTip(chkDeleted, "Include records of status 'DELETED'");
            chkDeleted.UseVisualStyleBackColor = true;
            // 
            // chkModified
            // 
            chkModified.AutoSize = true;
            chkModified.Checked = true;
            chkModified.CheckState = CheckState.Checked;
            chkModified.Location = new Point(180, 18);
            chkModified.Name = "chkModified";
            chkModified.Size = new Size(80, 19);
            chkModified.TabIndex = 20;
            chkModified.Text = "MODIFIED";
            toolTip1.SetToolTip(chkModified, "Include records of status 'MODIFIED'");
            chkModified.UseVisualStyleBackColor = true;
            // 
            // chkIdentical
            // 
            chkIdentical.AutoSize = true;
            chkIdentical.Checked = true;
            chkIdentical.CheckState = CheckState.Checked;
            chkIdentical.Location = new Point(278, 18);
            chkIdentical.Name = "chkIdentical";
            chkIdentical.Size = new Size(84, 19);
            chkIdentical.TabIndex = 21;
            chkIdentical.Text = "IDENTICAL";
            toolTip1.SetToolTip(chkIdentical, "Include records of status 'IDENTICAL'");
            chkIdentical.UseVisualStyleBackColor = true;
            // 
            // chkMoved
            // 
            chkMoved.AutoSize = true;
            chkMoved.Checked = true;
            chkMoved.CheckState = CheckState.Checked;
            chkMoved.Location = new Point(380, 18);
            chkMoved.Name = "chkMoved";
            chkMoved.Size = new Size(67, 19);
            chkMoved.TabIndex = 22;
            chkMoved.Text = "MOVED";
            toolTip1.SetToolTip(chkMoved, "Include records of status 'MOVED'");
            chkMoved.UseVisualStyleBackColor = true;
            // 
            // btnFilter
            // 
            btnFilter.Location = new Point(696, 6);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(127, 37);
            btnFilter.TabIndex = 24;
            btnFilter.Text = "Apply Filter";
            toolTip1.SetToolTip(btnFilter, "Apply the filter according to the selected values");
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // drpBase
            // 
            drpBase.FormattingEnabled = true;
            drpBase.Items.AddRange(new object[] { "src", "mas/src", "mas/MlineParameters", "mas/ReportMlineTemplates", "ionic-client", "images" });
            drpBase.Location = new Point(126, 370);
            drpBase.Name = "drpBase";
            drpBase.Size = new Size(395, 23);
            drpBase.TabIndex = 20;
            toolTip1.SetToolTip(drpBase, "Select base directory for analysis");
            // 
            // chkSVNUpdate
            // 
            chkSVNUpdate.AutoSize = true;
            chkSVNUpdate.Checked = true;
            chkSVNUpdate.CheckState = CheckState.Checked;
            chkSVNUpdate.Location = new Point(19, 374);
            chkSVNUpdate.Name = "chkSVNUpdate";
            chkSVNUpdate.Size = new Size(89, 19);
            chkSVNUpdate.TabIndex = 19;
            chkSVNUpdate.Text = "SVN Update";
            toolTip1.SetToolTip(chkSVNUpdate, "Perform SVN checkout or update");
            chkSVNUpdate.UseVisualStyleBackColor = true;
            // 
            // drpUser
            // 
            drpUser.FormattingEnabled = true;
            drpUser.Items.AddRange(new object[] { "All Users" });
            drpUser.Location = new Point(522, 16);
            drpUser.Name = "drpUser";
            drpUser.Size = new Size(151, 23);
            drpUser.TabIndex = 23;
            toolTip1.SetToolTip(drpUser, "Select the author to filter");
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth8Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "folder-64.png");
            imageList1.Images.SetKeyName(1, "compare-80.png");
            imageList1.Images.SetKeyName(2, "compare-git-64.png");
            imageList1.Images.SetKeyName(3, "WinMergeLogo.png");
            imageList1.Images.SetKeyName(4, "export-64.png");
            imageList1.Images.SetKeyName(5, "excel-64.png");
            imageList1.Images.SetKeyName(6, "table-64.png");
            imageList1.Images.SetKeyName(7, "html-64.png");
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(5, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1898, 985);
            tabControl1.TabIndex = 6;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(drpBase);
            tabPage1.Controls.Add(chkSVNUpdate);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(btnAnalyze);
            tabPage1.Controls.Add(panel1);
            tabPage1.Controls.Add(progressBar1);
            tabPage1.Controls.Add(txtLog);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1890, 957);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Configuration";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtSVNRoot);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(btnLoadSVN);
            groupBox1.Controls.Add(treeLeft);
            groupBox1.Controls.Add(treeRight);
            groupBox1.Controls.Add(txtLeft);
            groupBox1.Controls.Add(txtRight);
            groupBox1.Location = new Point(3, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1390, 350);
            groupBox1.TabIndex = 17;
            groupBox1.TabStop = false;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(btnFilter);
            tabPage2.Controls.Add(drpUser);
            tabPage2.Controls.Add(chkMoved);
            tabPage2.Controls.Add(chkIdentical);
            tabPage2.Controls.Add(chkModified);
            tabPage2.Controls.Add(chkDeleted);
            tabPage2.Controls.Add(chkAdded);
            tabPage2.Controls.Add(btnImport);
            tabPage2.Controls.Add(btnBackup);
            tabPage2.Controls.Add(gridView);
            tabPage2.Controls.Add(chkAutoScroll);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1890, 957);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Anayzer";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(472, 22);
            label1.Name = "label1";
            label1.Size = new Size(44, 15);
            label1.TabIndex = 25;
            label1.Text = "Author";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1904, 1001);
            Controls.Add(tabControl1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Juxtapose";
            Load += frmMain_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridView).EndInit();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TextBox txtRight;
        private TextBox txtLeft;
        private Button btnAnalyze;
        private DataGridView gridView2;
        private TextBox txtLog;
        private ProgressBar progressBar1;
        private Label label3;
        private TextBox txtWinmerge;
        private Label label4;
        private TextBox txtSVN;
        private Label Extentions;
        private TextBox txtExtensions;
        private ToolTip toolTip1;
        private DataGridView gridView;
        private ImageList imageList1;
        private CheckBox chkAutoScroll;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label label5;
        private TextBox txtSVNRoot;
        private TreeView treeRight;
        private TreeView treeLeft;
        private GroupBox groupBox1;
        private Button btnLoadSVN;
        private CheckBox chkSVNUpdate;
        private ComboBox drpBase;
        private Button btnImport;
        private Button btnBackup;
        private CheckBox chkDeleted;
        private CheckBox chkAdded;
        private CheckBox chkModified;
        private CheckBox chkIdentical;
        private CheckBox chkMoved;
        private ComboBox drpUser;
        private Button btnFilter;
        private Label label1;
    }
}
