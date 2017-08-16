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
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rafy.Domain;
using Rafy.LicenseManager.Encryption;
using Rafy.LicenseManager.Entities;
using Rafy.LicenseManager.Properties;

namespace Rafy.LicenseManager.Infrastructure
{
    internal class ManagerFormService
    {
        private static readonly Regex _regex = new Regex(@"^(?in)([\da-fA-F]{2}(-|$)){6}$");

        /// <summary>
        /// 生成 License Code。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static string GeneratorLicenseCode(LicenseEntity entity)
        {
            if (entity.LicenseTarget == LicenseTarget.None)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            var authCode = new AuthorizationCode
            {
                ExpireTime = entity.ExpireTime,
                CheckCode = entity.MacCode,
                Category = entity.LicenseTarget == LicenseTarget.Development ? 0 : 1
            };
            var licenseCode = SecurityAuthentication.Encrypt(authCode, entity.PrivateKey);

            return licenseCode;
        }

        /// <summary>
        /// 验证参数。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        internal static bool ValidateParameters(LicenseEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.PrivateKey) || string.IsNullOrWhiteSpace(entity.PublicKey))
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormServiceValidateParametersNeedPrivateKeyAndPublicKey, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(entity.MacCode) /*|| !_regex.IsMatch(entity.MacCode)*/)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersMACAddress, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (entity.ExpireTime < DateTime.Now)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersExpireTime, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (entity.LicenseTarget == LicenseTarget.None)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersSelectAuthenticationTarget, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        internal static void BindContextMenu(DataGridView dataGridView, ContextMenuStrip contextMenuTrip)
        {
            dataGridView.CellMouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                if (e.RowIndex < 0) return;

                var dgv = sender as DataGridView;
                if (dgv == null) return;

                if (!dgv.Rows[e.RowIndex].Selected)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                }

                if (dgv.SelectedRows.Count == 1)
                {
                    dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }

                contextMenuTrip.Show(Control.MousePosition.X, Control.MousePosition.Y);
            };
        }

        /// <summary>
        /// 为 <see cref="DataGridView"/> 对象绑定数据源。
        /// </summary>
        /// <param name="dataGridView"></param>
        internal static void BindDataGridView(DataGridView dataGridView)
        {
            var repository = RF.ResolveInstance<LicenseEntityRepository>();
            var list = repository.GetAll();

            var source = list.Select(l =>
            {
                var entity = (LicenseEntity) l;

                return new
                {
                    entity.Id,
                    entity.PrivateKey,
                    entity.PublicKey,
                    entity.LicenseTarget,
                    entity.ExpireTime,
                    entity.MacCode,
                    entity.LicenseCode,
                    entity.CreateTime
                };
            }).ToList();

            dataGridView.Invoke(new Action(() =>
            {
                dataGridView.DataSource = source;
            }));
        }

        /// <summary>
        /// 获取一对 RAS 公钥与私钥。
        /// </summary>
        /// <returns></returns>
        internal static string[] GeneratorKeys()
        {
            var keys = RSACryptoService.GenerateKeys();

            return keys;
        }

        internal static void DeleteRecord(string id)
        {
            var repository = RF.ResolveInstance<LicenseEntityRepository>();

            var entity = repository.GetById(id);

            if (entity == null) return;

            entity.PersistenceStatus = PersistenceStatus.Deleted;

            repository.Save(entity);
        }
    }
}
