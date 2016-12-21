/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20161220 15:58
 * 
*******************************************************/

using Rafy.Accounts;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 根据用户查找角色
    /// 支持user-role 和group-role 
    /// </summary>
    public interface IUserRoleFinder
    {
        /// <summary>
        /// 查询用户的角色列表
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        RoleList FindByUser(User user);
    }
}