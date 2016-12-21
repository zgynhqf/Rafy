/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161220 19:45
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 角色数据权限扩展
    /// </summary>
    [CompiledPropertyDeclarer]
    public static class RoleExtension
    {
        #region DataPermissionList DataPermissionList (角色扩展数据权限集合)

        /// <summary>
        /// 数据权限集合 扩展属性。
        /// </summary>
        public static ListProperty<DataPermissionList> DataPermissionListProperty =
            P<Role>.RegisterListExtension<DataPermissionList>("DataPermissionList", typeof(RoleExtension));
        /// <summary>
        /// 获取 数据权限集合 属性的值。
        /// </summary>
        /// <param name="me">要获取扩展属性值的对象。</param>
        public static DataPermissionList GetDataPermissionList(this Role me)
        {
            return me.GetLazyList(DataPermissionListProperty) as DataPermissionList;
        }

        #endregion
    }
}

