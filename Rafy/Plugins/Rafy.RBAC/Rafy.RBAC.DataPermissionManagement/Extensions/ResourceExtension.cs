/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161220 11:49
 * 
*******************************************************/


using System;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 
    /// </summary>
    [CompiledPropertyDeclarer]
    public static class ResourceExtension
    {
        #region string ResourceEntityType (资源领域实体类型)

        /// <summary>
        /// 资源领域实体类型 扩展属性。
        /// </summary>
        public static readonly Property<string> ResourceEntityTypeProperty =
            P<Resource>.RegisterExtension<string>("ResourceEntityType", typeof(ResourceExtension));
        /// <summary>
        /// 获取 资源领域实体类型 属性的值。
        /// </summary>
        /// <param name="me">要获取扩展属性值的对象。</param>
        public static string GeResourceEntityType(this Resource me)
        {
            return me.GetProperty(ResourceEntityTypeProperty);
        }
        /// <summary>
        /// 设置 资源领域实体类型 属性的值。
        /// </summary>
        /// <param name="me">要设置扩展属性值的对象。</param>
        /// <param name="value">设置的值。</param>
        public static void SetResourceEntityType(this Resource me, string value)
        {
            me.SetProperty(ResourceEntityTypeProperty, value);
        }

        #endregion

        #region bool IsSupportDataPermission (是否支持数据权限)

        /// <summary>
        /// 是否支持数据权限 扩展属性。
        /// </summary>
        public static readonly Property<bool> IsSupportDataPermissionProperty =
            P<Resource>.RegisterExtension<bool>("IsSupportDataPermission", typeof(ResourceExtension));
        /// <summary>
        /// 获取 是否支持数据权限 属性的值。
        /// </summary>
        /// <param name="me">要获取扩展属性值的对象。</param>
        public static bool GetIsSupportDataPermission(this Resource me)
        {
            return me.GetProperty(IsSupportDataPermissionProperty);
        }
        /// <summary>
        /// 设置 是否支持数据权限 属性的值。
        /// </summary>
        /// <param name="me">要设置扩展属性值的对象。</param>
        /// <param name="value">设置的值。</param>
        public static void SetIsSupportDataPermission(this Resource me, bool value)
        {
            me.SetProperty(IsSupportDataPermissionProperty, value);
        }

        #endregion
    }
}
