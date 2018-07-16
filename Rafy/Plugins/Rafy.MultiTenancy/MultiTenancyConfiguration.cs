/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160406 10:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;

namespace Rafy.MultiTenancy
{
    /// <summary>
    /// 多租户的配置类型。
    /// </summary>
    public class MultiTenancyConfiguration
    {
        private List<Type> _enabledTypes = new List<Type>();

        /// <summary>
        /// 已经启用多租户的实体的列表。
        /// </summary>
        internal List<Type> EnabledTypes
        {
            get
            {
                return _enabledTypes;
            }
        }

        /// <summary>
        /// 返回是否已经为某个实体类型启用了多租户隔离。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public bool IsMultiTenancyEnabled(Type entityType)
        {
            return _enabledTypes.Contains(entityType);
        }

        /// <summary>
        /// 为指定的实体类型启用多租户数据隔离。
        /// </summary>
        /// <param name="entities"></param>
        public void EnableMultiTenancy(params Type[] entities)
        {
            EnabledTypes.AddRange(entities);
        }
    }
}
