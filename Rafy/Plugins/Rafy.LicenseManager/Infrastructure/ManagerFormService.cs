﻿/*******************************************************
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
using Rafy.LicenseManager.Entities;
using Rafy.LicenseManager.Properties;
using Rafy.Security;

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
            if(entity.LicenseTarget == LicenseTarget.None)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormGetLicenseEntityAuthenticationTargetWarning, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            var authCode = new AuthorizationCode
            {
                ExpireTime = entity.ExpireTime,
                Mac = entity.MacCode,
                Category = entity.LicenseTarget == LicenseTarget.Development ? 0 : 1
            };
            var licenseCode = SecurityAuthentication.Encrypt(authCode, LicenseManagerResource.PublicKey);

            return licenseCode;
        }

        /// <summary>
        /// 验证参数。
        /// </summary>
        /// <param name="mac"></param>
        /// <param name="expireTime"></param>
        /// <param name="authorizationTarget"></param>
        /// <returns></returns>
        internal static bool ValidateParameters(string mac, DateTime expireTime, LicenseTarget authorizationTarget)
        {
            if(string.IsNullOrWhiteSpace(mac) || !_regex.IsMatch(mac))
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersMACAddress, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if(expireTime < DateTime.Now)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersExpireTime, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if(authorizationTarget == LicenseTarget.None)
            {
                MessageBox.Show(LicenseManagerResource.ManagerFormValidateParametersSelectAuthenticationTarget, LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var repository = RF.Concrete<LicenseEntityRepository>();

            var entity = repository.GetFirstBy(new CommonQueryCriteria(BinaryOperator.And) { new PropertyMatch(LicenseEntity.MacCodeProperty, PropertyOperator.Equal, mac) });

            if(entity != null)
            {
                MessageBox.Show(string.Format(LicenseManagerResource.ManagerFormValidateParametersMACHasUsed, mac), LicenseManagerResource.ManagerFormValidateParametersWarning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 为 <see cref="DataGridView"/> 对象绑定数据源。
        /// </summary>
        /// <param name="dataGridView"></param>
        internal static void BindDataGridView(DataGridView dataGridView)
        {
            var repository = RF.Concrete<LicenseEntityRepository>();
            var list = repository.GetAll();

            var source = list.Select(l =>
            {
                var entity = (LicenseEntity) l;

                return new
                {
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
    }
}
