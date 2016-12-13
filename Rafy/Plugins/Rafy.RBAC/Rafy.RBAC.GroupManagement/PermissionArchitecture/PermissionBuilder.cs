/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161209
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161209 17:58
 * 
*******************************************************/

using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.RBAC.GroupManagement.PermissionArchitecture
{
    /// <summary>
    /// 权限系统对象生成器
    /// </summary>
    public abstract class PermissionBuilder
    {
        #region 私有的字段

        private PermissionEntry _permissionEntry;
        private GroupRepository _groupRepository;
        private UserRepository _userRepository;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数初始化默认实现的权限使用对象
        /// </summary>
        protected PermissionBuilder():this(new DefaultPermissionEntry())
        { }

        /// <summary>
        /// 重载构造函数，初始化权限使用对象
        /// </summary>
        /// <param name="permissionEntry">权限使用对象</param>
        protected PermissionBuilder(PermissionEntry permissionEntry)
        {
            if (permissionEntry == null)
            {
                _permissionEntry = new DefaultPermissionEntry();
            }
            else
            {
                _permissionEntry = permissionEntry;
            }
            if (_groupRepository == null)
            {
                _groupRepository = RepositoryFacade.ResolveInstance<GroupRepository>();
            }
            if (_userRepository == null)
            {
                _userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
            }
        }

        #endregion

        #region 获取数据库操作对象

        /// <summary>
        /// 获取组的仓库对象
        /// </summary>
        protected GroupRepository GroupRepository
        {
            get { return this._groupRepository; }
        }

        /// <summary>
        /// 获取用户的仓库对象
        /// </summary>
        protected UserRepository UserRepository
        {
            get { return this._userRepository; }
        }

        #endregion

        #region 权限的核心方法，可以重写实现个人逻辑

        /// <summary>
        /// 获取当前组的所有菜单资源的权限
        /// </summary>
        /// <param name="groupID">用户组的主键</param>
        /// <returns>返回获取到的资源数组列表</returns>
        protected abstract IList<Resource> GenerateResourcePermission(int groupID);

        /// <summary>
        /// 获取当前组的所有菜单资源所对应的操作权限
        /// </summary>
        /// <param name="groupID">用户组的主键</param>
        /// <returns>返回获取到的当前组每个资源所对应的操作权限的字典集合</returns>
        protected abstract IDictionary<long, IList<ResourceOperation>> GenerateOperationPermission(int groupID);

        /// <summary>
        /// 初始化用户的详情信息
        /// </summary>
        /// <returns>返回初始化后的用户实例对象</returns>
        protected abstract User InitUser();

        /// <summary>
        /// 初始化当前用户的组信息
        /// </summary>
        /// <param name="groupID">当前组的主键</param>
        /// <returns>返回初始化后的组的实例对象</returns>
        protected abstract Group InitGroup(int groupID);

        #endregion

        #region 当前的模板方法

        /// <summary>
        /// 根据当前登录的用户所属的组的主键获取登录用户当前组所对应的权限对象
        /// </summary>
        /// <param name="groupID">当前组的主键</param>
        /// <returns>返回权限接口对象实例</returns>
        public PermissionEntry ResolvePermission(int groupID)
        {
            _permissionEntry.Resources=GenerateResourcePermission(groupID);
            _permissionEntry.Operations=GenerateOperationPermission(groupID);
            _permissionEntry.User = InitUser();
            _permissionEntry.Group = InitGroup(groupID);
            return _permissionEntry;
        }

        #endregion
    }
}