/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141222
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141222 16:22
 * 
*******************************************************/

using System;
using Rafy.Domain;

namespace Rafy.MultiTenancy
{
    /// <summary>
    /// 实体的多租户属性扩展类型。
    /// </summary>
    public static class TenantAwareEntityExtension
    {
        #region TenantId

        /// <summary>
        /// 表明某个实体的审核状态。
        /// </summary>
        public const string TenantIdProperty = "TenantId";
        /// <summary>
        /// 获取某个实体的审核状态。
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static long GetTenantId(this Entity me)
        {
            var mp = me.PropertiesContainer.GetCompiledProperties().Find(TenantIdProperty);
            return Convert.ToInt64(me.GetProperty(mp));
        }
        /// <summary>
        /// 设置某个实体的审核状态。
        /// </summary>
        /// <param name="me">Me.</param>
        /// <param name="value">The value.</param>
        public static void SetTenantId(this Entity me, long value)
        {
            var mp = me.PropertiesContainer.GetCompiledProperties().Find(TenantIdProperty);
            me.SetProperty(mp, value);
        }

        #endregion
    }
}