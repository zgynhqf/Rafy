using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rafy.Domain;
using Rafy.LicenseManager.Entities;
using Rafy.LicenseManager.Infrastructure;

namespace Rafy.LicenseManager.UI
{
    public partial class ManagerForm : Form
    {
        public ManagerForm()
        {
            InitializeComponent();
        }

        private void _ManagerForm_Load(object sender, EventArgs e)
        {
            this._BindLicenseTargetComboBox();
        }

        private void _BindLicenseTargetComboBox()
        {
            var custoemrs = typeof(LicenseTarget)
                .GetMembers(BindingFlags.Public | BindingFlags.Static)
                .Select(m =>new
                {
                    Description = ((DescriptionAttribute)m.GetCustomAttributes().First(c => c.GetType() == typeof(DescriptionAttribute))).Description,
                    Name = m.Name
                }).ToList();

            this.cbxAuthorizationTarget.DisplayMember = "Description";
            this.cbxAuthorizationTarget.ValueMember = "Name";
            this.cbxAuthorizationTarget.DataSource = custoemrs;
        }

        private void _BtnGeneratorAuthorizationCode_Click(object sender, EventArgs e)
        {
            var mac = this.tbMac.Text.Trim();
            var expireTime = this.dtpExpireTime.Value;
            var authorizationTarget = this.cbxAuthorizationTarget.SelectedValue.ToString();

            if(!this._ValidateParameters(mac, expireTime, authorizationTarget)) return;

            var license = new LicenseEntity
            {
                MacCode = mac,
                ExpireTime = expireTime,
                LicenseTarget = LicenseTarget.None,
                PersistenceStatus = PersistenceStatus.New
            };

            var repository = RF.Concrete<LicenseEntityRepository>();
            repository.Save(license);
        }

        private void _Exit_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private bool _ValidateParameters(string mac, DateTime expireTime, string authorTarget)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                MessageBox.Show("MAC 地址不能为空。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (expireTime < DateTime.Now)
            {
                MessageBox.Show("过期时间不能小于当前时间。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(authorTarget) || authorTarget.Equals("None", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageBox.Show("授权目标不能为空。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var repository = RF.Concrete<LicenseEntityRepository>();

            var entity = repository.GetFirstBy(new CommonQueryCriteria(BinaryOperator.And) {new PropertyMatch(LicenseEntity.MacCodeProperty, PropertyOperator.Equal, mac)});

            if (entity != null)
            {
                MessageBox.Show($"当前 MAC 地址：{mac}，已经被授权。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
