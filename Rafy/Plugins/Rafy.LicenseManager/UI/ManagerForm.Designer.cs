namespace Rafy.LicenseManager.UI
{
    partial class ManagerForm
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
            if(disposing && (components != null))
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabAuthorization = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbLicenseCode = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGeneratorAuthorizationCode = new System.Windows.Forms.Button();
            this.cbxAuthorizationTarget = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpExpireTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMac = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabAuthorizationView = new System.Windows.Forms.TabPage();
            this.dgvLicenseView = new System.Windows.Forms.DataGridView();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabAuthorization.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabAuthorizationView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLicenseView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.帮助ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(882, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.退出ToolStripMenuItem});
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.文件ToolStripMenuItem.Text = "文件";
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.退出ToolStripMenuItem.Text = "退出";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this._Exit_ToolStripMenuItem_Click);
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            this.帮助ToolStripMenuItem.Size = new System.Drawing.Size(51, 24);
            this.帮助ToolStripMenuItem.Text = "帮助";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabAuthorization);
            this.tabControl1.Controls.Add(this.tabAuthorizationView);
            this.tabControl1.Location = new System.Drawing.Point(13, 32);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(857, 409);
            this.tabControl1.TabIndex = 1;
            // 
            // tabAuthorization
            // 
            this.tabAuthorization.Controls.Add(this.groupBox2);
            this.tabAuthorization.Controls.Add(this.groupBox1);
            this.tabAuthorization.Location = new System.Drawing.Point(4, 25);
            this.tabAuthorization.Name = "tabAuthorization";
            this.tabAuthorization.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuthorization.Size = new System.Drawing.Size(849, 380);
            this.tabAuthorization.TabIndex = 0;
            this.tabAuthorization.Text = "授权";
            this.tabAuthorization.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.tbLicenseCode);
            this.groupBox2.Location = new System.Drawing.Point(503, 34);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(321, 319);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "授权码";
            // 
            // tbLicenseCode
            // 
            this.tbLicenseCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLicenseCode.Location = new System.Drawing.Point(19, 31);
            this.tbLicenseCode.Multiline = true;
            this.tbLicenseCode.Name = "tbLicenseCode";
            this.tbLicenseCode.ReadOnly = true;
            this.tbLicenseCode.Size = new System.Drawing.Size(296, 269);
            this.tbLicenseCode.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.btnGeneratorAuthorizationCode);
            this.groupBox1.Controls.Add(this.cbxAuthorizationTarget);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dtpExpireTime);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbMac);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(22, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(455, 319);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入";
            // 
            // btnGeneratorAuthorizationCode
            // 
            this.btnGeneratorAuthorizationCode.Location = new System.Drawing.Point(124, 214);
            this.btnGeneratorAuthorizationCode.Name = "btnGeneratorAuthorizationCode";
            this.btnGeneratorAuthorizationCode.Size = new System.Drawing.Size(100, 30);
            this.btnGeneratorAuthorizationCode.TabIndex = 6;
            this.btnGeneratorAuthorizationCode.Text = "生成授权";
            this.btnGeneratorAuthorizationCode.UseVisualStyleBackColor = true;
            this.btnGeneratorAuthorizationCode.Click += new System.EventHandler(this._BtnGeneratorAuthorizationCode_Click);
            // 
            // cbxAuthorizationTarget
            // 
            this.cbxAuthorizationTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxAuthorizationTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAuthorizationTarget.FormattingEnabled = true;
            this.cbxAuthorizationTarget.Location = new System.Drawing.Point(124, 143);
            this.cbxAuthorizationTarget.Name = "cbxAuthorizationTarget";
            this.cbxAuthorizationTarget.Size = new System.Drawing.Size(288, 23);
            this.cbxAuthorizationTarget.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "授权目标：";
            // 
            // dtpExpireTime
            // 
            this.dtpExpireTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpExpireTime.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dtpExpireTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpExpireTime.Location = new System.Drawing.Point(124, 81);
            this.dtpExpireTime.MinDate = new System.DateTime(2016, 1, 1, 0, 0, 0, 0);
            this.dtpExpireTime.Name = "dtpExpireTime";
            this.dtpExpireTime.Size = new System.Drawing.Size(288, 25);
            this.dtpExpireTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "过期时间：";
            // 
            // tbMac
            // 
            this.tbMac.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMac.Location = new System.Drawing.Point(124, 31);
            this.tbMac.Name = "tbMac";
            this.tbMac.Size = new System.Drawing.Size(288, 25);
            this.tbMac.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "MAC 地址：";
            // 
            // tabAuthorizationView
            // 
            this.tabAuthorizationView.Controls.Add(this.dgvLicenseView);
            this.tabAuthorizationView.Location = new System.Drawing.Point(4, 25);
            this.tabAuthorizationView.Name = "tabAuthorizationView";
            this.tabAuthorizationView.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuthorizationView.Size = new System.Drawing.Size(849, 380);
            this.tabAuthorizationView.TabIndex = 1;
            this.tabAuthorizationView.Text = "授权信息查询";
            this.tabAuthorizationView.UseVisualStyleBackColor = true;
            // 
            // dgvLicenseView
            // 
            this.dgvLicenseView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLicenseView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLicenseView.Location = new System.Drawing.Point(6, 6);
            this.dgvLicenseView.Name = "dgvLicenseView";
            this.dgvLicenseView.RowTemplate.Height = 27;
            this.dgvLicenseView.Size = new System.Drawing.Size(837, 368);
            this.dgvLicenseView.TabIndex = 0;
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 453);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManagerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ManagerForm";
            this.Load += new System.EventHandler(this._ManagerForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabAuthorization.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabAuthorizationView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLicenseView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabAuthorization;
        private System.Windows.Forms.TabPage tabAuthorizationView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbMac;
        private System.Windows.Forms.DateTimePicker dtpExpireTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxAuthorizationTarget;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnGeneratorAuthorizationCode;
        private System.Windows.Forms.TextBox tbLicenseCode;
        private System.Windows.Forms.DataGridView dgvLicenseView;
    }
}