/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 15:55
 * 
*******************************************************/

using Rafy.RBAC.RoleManagement;
using System;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 数据权限开关
    /// </summary>
    public class DataPermissionFacade
    {
        /// <summary>
        /// 启用资源的数据权限
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static IDisposable EnableDataPermission(Resource resource)
        {
           return DataPermissionInterceptor.FilterResource.UseScopeValue(resource);
        }

        /// <summary>
        /// 禁用数据权限
        /// </summary>
        /// <returns></returns>
        public static IDisposable DisableDataPermission()
        {
            return DataPermissionInterceptor.FilterResource.UseScopeValue(null);
        }
    }
}
