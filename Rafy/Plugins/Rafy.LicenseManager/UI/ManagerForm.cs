using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Rafy.Domain;
using Rafy.LicenseManager.Entities;
using Rafy.LicenseManager.Infrastructure;
using Rafy.LicenseManager.Properties;
using Rafy.Security;

namespace Rafy.LicenseManager.UI
{
    public partial class ManagerForm : Form
    {
        private ToolTip _toolTip;

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
                    ((DescriptionAttribute)m.GetCustomAttributes().First(c => c.GetType() == typeof(DescriptionAttribute))).Description
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
            var mac = this.tbMac.Text.Trim();
            var expireTime = this.dtpExpireTime.Value;
            LicenseTarget authorizationTarget;

            if (!Enum.TryParse(this.cbxAuthorizationTarget.SelectedValue.ToString(), out authorizationTarget))
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if(!ManagerFormService.ValidateParameters(mac, expireTime, authorizationTarget))
                return null;

            var license = new LicenseEntity
            {
                MacCode = mac,
                ExpireTime = expireTime,
                LicenseTarget = authorizationTarget,
                PersistenceStatus = PersistenceStatus.New
            };
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
            var repository = RF.Concrete<LicenseEntityRepository>();
            repository.Save(entity);

            this.tbLicenseCode.Text = entity.LicenseCode;
            this.tbLicenseCode.Focus();
            this.tbLicenseCode.Select(0, entity.LicenseCode.Length);
            Clipboard.SetText(entity.LicenseCode);

            this.lblLog.Text = LicenseManagerResource.ManagerFormSaveLicenseSuccess;
        }

        private void _TbMac_MouseEnter(object sender, EventArgs e)
        {
            this._toolTip.Show("MAC 地址格式：XX-XX-XX-XX-XX-XX", (Control) sender);
        }

        private void _TbMac_MouseLeave(object sender, EventArgs e)
        {
            this._toolTip.Hide((IWin32Window) sender);
        }
    }
}
