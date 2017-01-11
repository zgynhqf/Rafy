/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170110 13:43
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.UserRoleManagement;

namespace Rafy.Accounts
{
    /// <summary>
    /// 用户属性扩展
    /// </summary>
    [CompiledPropertyDeclarer]
    public static class UserExtension
    {
        #region UserRoleList UserRoleList (用户扩展用户角色集合)

        /// <summary>
        /// 数据权限集合 扩展属性。
        /// </summary>
        public static ListProperty<UserRoleList> UserRoleListProperty =
            P<Role>.RegisterListExtension<UserRoleList>("UserRoleList", typeof(UserExtension));
        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static UserRoleList GetUserRoleList(this User me)
        {
            return me.GetLazyList(UserRoleListProperty) as UserRoleList;
        }

        #endregion
    }
}
