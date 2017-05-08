/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20160921 10:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rafy.Domain;
using Rafy.LicenseManager.Encryption;
using Rafy.LicenseManager.Entities;
using Rafy.LicenseManager.Infrastructure;
using Rafy.LicenseManager.Properties;

namespace Rafy.LicenseManager.UI
{
    public partial class ManagerForm : Form
    {
        private ToolTip _toolTip;
        private string[] _keys;

        private readonly Dictionary<string, string> _commandDictionary = new Dictionary<string, string>
        {
            {"复制私钥", "PrivateKey"},
            {"复制公钥", "PublicKey"},
            {"复制授权码", "LicenseCode"}
        };
        private readonly Dictionary<string, Action<string>> _delegateDictionary=new Dictionary<string, Action<string>>
        {
            {"删除", ManagerFormService.DeleteRecord}
        };

        public ManagerForm()
        {
            InitializeComponent();
        }

        private void _ManagerForm_Load(object sender, EventArgs e)
        {
            this._BindLicenseTargetComboBox();

            var now = DateTime.Now.AddMonths(6);
            this.dtpExpireTime.Value = new DateTime(now.Year, now.Month, now.Day);

            this._toolTip = new ToolTip
            {
                ToolTipTitle = "提示",
                AutoPopDelay = 0
            };

            ManagerFormService.BindContextMenu(this.dgvLicenseView, this.dgvContextMenu);
        }

        /// <summary>
        /// 为 <see cref="ComboBox"/> 绑定数据源。
        /// </summary>
        private void _BindLicenseTargetComboBox()
        {
            var custoemrs = typeof(LicenseTarget)
                .GetMembers(BindingFlags.Public | BindingFlags.Static)
                .Select(m =>new
                {
                    m.Name,
                    ((DescriptionAttribute)m.GetCustomAttributes(typeof(DescriptionAttribute),false).First(c => c.GetType() == typeof(DescriptionAttribute))).Description
                }).ToList();

            this.cbxAuthorizationTarget.DisplayMember = "Description";
            this.cbxAuthorizationTarget.ValueMember = "Name";
            this.cbxAuthorizationTarget.DataSource = custoemrs;
        }

        private void _BtnGeneratorAuthorizationCode_Click(object sender, EventArgs e)
        {
            this.lblLog.Text = string.Empty;

            var license = this._GetLicenseEntity();

            if (license == null)
                return;

            this._SaveLicense(license);
        }

        private void _Exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 获取一个 <see cref="LicenseEntity"/> 对象的实例。
        /// </summary>
        /// <returns></returns>
        private LicenseEntity _GetLicenseEntity()
        {
            this._keys = new[] {this.tbPrivateKey.Text.Trim(), this.tbPublicKey.Text.Trim()};
            var mac = this.tbMac.Text.Trim();
            var expireTime = this.dtpExpireTime.Value;
            LicenseTarget authorizationTarget;


            if (!Enum.TryParse(this.cbxAuthorizationTarget.SelectedValue.ToString(), out authorizationTarget))
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            var license = new LicenseEntity {
                PrivateKey = this._keys[0],
                PublicKey = this._keys[1],
                MacCode = mac,
                ExpireTime = expireTime,
                LicenseTarget = authorizationTarget,
                CreateTime = DateTime.Now,
                PersistenceStatus = PersistenceStatus.New
            };

            if(!ManagerFormService.ValidateParameters(license))
                return null;

            var licenseCode = ManagerFormService.GeneratorLicenseCode(license);

            if(string.IsNullOrWhiteSpace(licenseCode))
                return null;

            license.LicenseCode = licenseCode;

            return license;
        }

        /// <summary>
        /// 保存 License 信息。
        /// </summary>
        /// <param name="entity"></param>
        private void _SaveLicense(LicenseEntity entity)
        {
            var repository = RF.ResolveInstance<LicenseEntityRepository>();
            repository.Save(entity);

            this.tbLicenseCode.Text = entity.LicenseCode;
            this.tbLicenseCode.Focus();
            this.tbLicenseCode.Select(0, entity.LicenseCode.Length);

            try
            {
                Clipboard.SetText(entity.LicenseCode);
                this.lblLog.Text = LicenseManagerResource.ManagerFormSaveLicenseSuccess;
            }
            catch (ExternalException)
            {
                MessageBox.Show(@"自动复制到剪贴版失败，请手动复制。", LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void _TbMac_MouseEnter(object sender, EventArgs e)
        {
            this._toolTip.Show("MAC 地址格式：XX-XX-XX-XX-XX-XX", (Control) sender);
        }

        private void _TbMac_MouseLeave(object sender, EventArgs e)
        {
            this._toolTip.Hide((IWin32Window) sender);
        }

        private void _TabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage.Text == LicenseManagerResource.ManagerFormTabControl1SelectedQueryAuthentication)
            {
                Task.Factory.StartNew(() =>
                {
                    ManagerFormService.BindDataGridView(this.dgvLicenseView);
                });
            }
        }

        private void _BtnGenerator_Click(object sender, EventArgs e)
        {
            this._keys = ManagerFormService.GeneratorKeys();

            this.tbPrivateKey.Text = this._keys[0];
            this.tbPublicKey.Text = this._keys[1];
        }

        private void _TextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if(this._keys == null) return;
            
            this.textBox1.Text = this._keys[0];
        }

        private void _DgvContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.dgvContextMenu.Hide();
            if(this.dgvLicenseView.SelectedRows.Count < 1) return;

            var row = this.dgvLicenseView.SelectedRows[0];
            var commandText = e.ClickedItem.Text;
            string dataPropertyName;
            string expressData;

            if (this._commandDictionary.TryGetValue(commandText, out dataPropertyName))
            {
                expressData = row.Cells[dataPropertyName].Value.ToString();
            }
            else
            {
                if (this._delegateDictionary.ContainsKey(e.ClickedItem.Text))
                {
                    this._delegateDictionary[e.ClickedItem.Text](row.Cells["Id"].Value.ToString());
                    MessageBox.Show(@"删除成功", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ManagerFormService.BindDataGridView(this.dgvLicenseView);
                    return;
                }

                var publicKey = row.Cells["PublicKey"].Value.ToString();
                var licenseCode = row.Cells["LicenseCode"].Value.ToString();
                expressData = RSACryptoService.DecryptString(licenseCode, publicKey);
            }

            try
            {
                Clipboard.SetText(expressData);
                MessageBox.Show(LicenseManagerResource.ManagerFormdgvContextMenuPaste);
            }
            catch (ExternalException)
            {
                MessageBox.Show(@"复制到剪贴版失败，请重试。", LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
    }
}
