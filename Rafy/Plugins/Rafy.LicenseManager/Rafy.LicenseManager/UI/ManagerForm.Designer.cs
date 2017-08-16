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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGetPrivateKey = new System.Windows.Forms.TabPage();
            this.btnGenerator = new System.Windows.Forms.Button();
            this.tbPublicKey = new System.Windows.Forms.TextBox();
            this.tbPrivateKey = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tabAuthorization = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tbLicenseCode = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblLog = new System.Windows.Forms.Label();
            this.btnGeneratorAuthorizationCode = new System.Windows.Forms.Button();
            this.cbxAuthorizationTarget = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpExpireTime = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMac = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabAuthorizationView = new System.Windows.Forms.TabPage();
            this.dgvLicenseView = new System.Windows.Forms.DataGridView();
            this.dgvContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmPrivateKey = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmPublicKey = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmLicenceCode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmExpressData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.MacCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrivateKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PublicKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LicenseCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExpireTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LicenseTarget = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabGetPrivateKey.SuspendLayout();
            this.tabAuthorization.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabAuthorizationView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLicenseView)).BeginInit();
            this.dgvContextMenu.SuspendLayout();
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
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(114, 26);
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
            this.tabControl1.Controls.Add(this.tabGetPrivateKey);
            this.tabControl1.Controls.Add(this.tabAuthorization);
            this.tabControl1.Controls.Add(this.tabAuthorizationView);
            this.tabControl1.Location = new System.Drawing.Point(13, 32);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(857, 509);
            this.tabControl1.TabIndex = 1;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this._TabControl1_Selected);
            // 
            // tabGetPrivateKey
            // 
            this.tabGetPrivateKey.AllowDrop = true;
            this.tabGetPrivateKey.Controls.Add(this.btnGenerator);
            this.tabGetPrivateKey.Controls.Add(this.tbPublicKey);
            this.tabGetPrivateKey.Controls.Add(this.tbPrivateKey);
            this.tabGetPrivateKey.Controls.Add(this.label6);
            this.tabGetPrivateKey.Controls.Add(this.label5);
            this.tabGetPrivateKey.Location = new System.Drawing.Point(4, 25);
            this.tabGetPrivateKey.Name = "tabGetPrivateKey";
            this.tabGetPrivateKey.Padding = new System.Windows.Forms.Padding(3);
            this.tabGetPrivateKey.Size = new System.Drawing.Size(849, 480);
            this.tabGetPrivateKey.TabIndex = 2;
            this.tabGetPrivateKey.Text = "获取公钥私钥";
            this.tabGetPrivateKey.UseVisualStyleBackColor = true;
            // 
            // btnGenerator
            // 
            this.btnGenerator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerator.Location = new System.Drawing.Point(368, 429);
            this.btnGenerator.Name = "btnGenerator";
            this.btnGenerator.Size = new System.Drawing.Size(121, 34);
            this.btnGenerator.TabIndex = 4;
            this.btnGenerator.Text = "获取公钥私钥";
            this.btnGenerator.UseVisualStyleBackColor = true;
            this.btnGenerator.Click += new System.EventHandler(this._BtnGenerator_Click);
            // 
            // tbPublicKey
            // 
            this.tbPublicKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPublicKey.Location = new System.Drawing.Point(78, 226);
            this.tbPublicKey.Multiline = true;
            this.tbPublicKey.Name = "tbPublicKey";
            this.tbPublicKey.Size = new System.Drawing.Size(746, 181);
            this.tbPublicKey.TabIndex = 3;
            // 
            // tbPrivateKey
            // 
            this.tbPrivateKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPrivateKey.Location = new System.Drawing.Point(78, 21);
            this.tbPrivateKey.Multiline = true;
            this.tbPrivateKey.Name = "tbPrivateKey";
            this.tbPrivateKey.Size = new System.Drawing.Size(746, 181);
            this.tbPrivateKey.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 228);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 15);
            this.label6.TabIndex = 1;
            this.label6.Text = "公钥：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "私钥：";
            // 
            // tabAuthorization
            // 
            this.tabAuthorization.Controls.Add(this.groupBox2);
            this.tabAuthorization.Controls.Add(this.groupBox1);
            this.tabAuthorization.Location = new System.Drawing.Point(4, 25);
            this.tabAuthorization.Name = "tabAuthorization";
            this.tabAuthorization.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuthorization.Size = new System.Drawing.Size(849, 480);
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
            this.groupBox2.Size = new System.Drawing.Size(321, 419);
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
            this.tbLicenseCode.Size = new System.Drawing.Size(296, 369);
            this.tbLicenseCode.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblLog);
            this.groupBox1.Controls.Add(this.btnGeneratorAuthorizationCode);
            this.groupBox1.Controls.Add(this.cbxAuthorizationTarget);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dtpExpireTime);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbMac);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(22, 34);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(455, 419);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "输入";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(124, 30);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(288, 120);
            this.textBox1.TabIndex = 9;
            this.textBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this._TextBox1_MouseClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "输入私钥：";
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblLog.Location = new System.Drawing.Point(121, 385);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(0, 15);
            this.lblLog.TabIndex = 7;
            // 
            // btnGeneratorAuthorizationCode
            // 
            this.btnGeneratorAuthorizationCode.Location = new System.Drawing.Point(124, 329);
            this.btnGeneratorAuthorizationCode.Name = "btnGeneratorAuthorizationCode";
            this.btnGeneratorAuthorizationCode.Size = new System.Drawing.Size(120, 30);
            this.btnGeneratorAuthorizationCode.TabIndex = 6;
            this.btnGeneratorAuthorizationCode.Text = "生成授权码";
            this.btnGeneratorAuthorizationCode.UseVisualStyleBackColor = true;
            this.btnGeneratorAuthorizationCode.Click += new System.EventHandler(this._BtnGeneratorAuthorizationCode_Click);
            // 
            // cbxAuthorizationTarget
            // 
            this.cbxAuthorizationTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxAuthorizationTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxAuthorizationTarget.FormattingEnabled = true;
            this.cbxAuthorizationTarget.Location = new System.Drawing.Point(124, 275);
            this.cbxAuthorizationTarget.Name = "cbxAuthorizationTarget";
            this.cbxAuthorizationTarget.Size = new System.Drawing.Size(288, 23);
            this.cbxAuthorizationTarget.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 279);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "授权目标：";
            // 
            // dtpExpireTime
            // 
            this.dtpExpireTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dtpExpireTime.CustomFormat = "yyyy-MM-dd";
            this.dtpExpireTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpExpireTime.Location = new System.Drawing.Point(124, 222);
            this.dtpExpireTime.MinDate = new System.DateTime(2016, 1, 1, 0, 0, 0, 0);
            this.dtpExpireTime.Name = "dtpExpireTime";
            this.dtpExpireTime.Size = new System.Drawing.Size(288, 25);
            this.dtpExpireTime.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 228);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "过期时间：";
            // 
            // tbMac
            // 
            this.tbMac.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMac.Location = new System.Drawing.Point(124, 172);
            this.tbMac.Name = "tbMac";
            this.tbMac.Size = new System.Drawing.Size(288, 25);
            this.tbMac.TabIndex = 1;
            this.tbMac.MouseEnter += new System.EventHandler(this._TbMac_MouseEnter);
            this.tbMac.MouseLeave += new System.EventHandler(this._TbMac_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 177);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "校验码：";
            // 
            // tabAuthorizationView
            // 
            this.tabAuthorizationView.Controls.Add(this.dgvLicenseView);
            this.tabAuthorizationView.Location = new System.Drawing.Point(4, 25);
            this.tabAuthorizationView.Name = "tabAuthorizationView";
            this.tabAuthorizationView.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuthorizationView.Size = new System.Drawing.Size(849, 480);
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
            this.dgvLicenseView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MacCode,
            this.Id,
            this.PrivateKey,
            this.PublicKey,
            this.LicenseCode,
            this.ExpireTime,
            this.LicenseTarget,
            this.CreateTime});
            this.dgvLicenseView.Location = new System.Drawing.Point(6, 6);
            this.dgvLicenseView.Name = "dgvLicenseView";
            this.dgvLicenseView.RowTemplate.Height = 27;
            this.dgvLicenseView.Size = new System.Drawing.Size(837, 468);
            this.dgvLicenseView.TabIndex = 0;
            // 
            // dgvContextMenu
            // 
            this.dgvContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.dgvContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmPrivateKey,
            this.tsmPublicKey,
            this.tsmLicenceCode,
            this.toolStripSeparator1,
            this.tsmExpressData,
            this.toolStripSeparator2,
            this.tsmDelete});
            this.dgvContextMenu.Name = "dgvContextMenu";
            this.dgvContextMenu.Size = new System.Drawing.Size(169, 136);
            this.dgvContextMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this._DgvContextMenu_ItemClicked);
            // 
            // tsmPrivateKey
            // 
            this.tsmPrivateKey.Name = "tsmPrivateKey";
            this.tsmPrivateKey.Size = new System.Drawing.Size(168, 24);
            this.tsmPrivateKey.Text = "复制私钥";
            // 
            // tsmPublicKey
            // 
            this.tsmPublicKey.Name = "tsmPublicKey";
            this.tsmPublicKey.Size = new System.Drawing.Size(168, 24);
            this.tsmPublicKey.Text = "复制公钥";
            // 
            // tsmLicenceCode
            // 
            this.tsmLicenceCode.Name = "tsmLicenceCode";
            this.tsmLicenceCode.Size = new System.Drawing.Size(168, 24);
            this.tsmLicenceCode.Text = "复制授权码";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // tsmExpressData
            // 
            this.tsmExpressData.Name = "tsmExpressData";
            this.tsmExpressData.Size = new System.Drawing.Size(168, 24);
            this.tsmExpressData.Text = "获取明文数据";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // tsmDelete
            // 
            this.tsmDelete.Name = "tsmDelete";
            this.tsmDelete.Size = new System.Drawing.Size(168, 24);
            this.tsmDelete.Text = "删除";
            // 
            // MacCode
            // 
            this.MacCode.DataPropertyName = "MacCode";
            this.MacCode.HeaderText = "校验码";
            this.MacCode.Name = "MacCode";
            this.MacCode.ReadOnly = true;
            this.MacCode.Width = 120;
            // 
            // Id
            // 
            this.Id.DataPropertyName = "Id";
            this.Id.HeaderText = "Id";
            this.Id.Name = "Id";
            this.Id.Visible = false;
            // 
            // PrivateKey
            // 
            this.PrivateKey.DataPropertyName = "PrivateKey";
            this.PrivateKey.HeaderText = "私钥";
            this.PrivateKey.Name = "PrivateKey";
            // 
            // PublicKey
            // 
            this.PublicKey.DataPropertyName = "PublicKey";
            this.PublicKey.HeaderText = "公钥";
            this.PublicKey.Name = "PublicKey";
            // 
            // LicenseCode
            // 
            this.LicenseCode.DataPropertyName = "LicenseCode";
            this.LicenseCode.HeaderText = "授权码";
            this.LicenseCode.Name = "LicenseCode";
            this.LicenseCode.ReadOnly = true;
            // 
            // ExpireTime
            // 
            this.ExpireTime.DataPropertyName = "ExpireTime";
            this.ExpireTime.HeaderText = "授权过期时间";
            this.ExpireTime.Name = "ExpireTime";
            this.ExpireTime.ReadOnly = true;
            // 
            // LicenseTarget
            // 
            this.LicenseTarget.DataPropertyName = "LicenseTarget";
            this.LicenseTarget.HeaderText = "授权目标";
            this.LicenseTarget.Name = "LicenseTarget";
            this.LicenseTarget.ReadOnly = true;
            // 
            // CreateTime
            // 
            this.CreateTime.DataPropertyName = "CreateTime";
            this.CreateTime.HeaderText = "创建时间";
            this.CreateTime.Name = "CreateTime";
            this.CreateTime.ReadOnly = true;
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 553);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ManagerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LicenseManager";
            this.Load += new System.EventHandler(this._ManagerForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabGetPrivateKey.ResumeLayout(false);
            this.tabGetPrivateKey.PerformLayout();
            this.tabAuthorization.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabAuthorizationView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLicenseView)).EndInit();
            this.dgvContextMenu.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabGetPrivateKey;
        private System.Windows.Forms.Button btnGenerator;
        private System.Windows.Forms.TextBox tbPublicKey;
        private System.Windows.Forms.TextBox tbPrivateKey;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ContextMenuStrip dgvContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmPrivateKey;
        private System.Windows.Forms.ToolStripMenuItem tsmPublicKey;
        private System.Windows.Forms.ToolStripMenuItem tsmLicenceCode;
        private System.Windows.Forms.ToolStripMenuItem tsmExpressData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem tsmDelete;
        private System.Windows.Forms.DataGridViewTextBoxColumn MacCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrivateKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn PublicKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn LicenseCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ExpireTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn LicenseTarget;
        private System.Windows.Forms.DataGridViewTextBoxColumn CreateTime;
    }
}