using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.RBAC.RoleManagement;
using System.Data;
using Rafy.Accounts;
using Rafy.Domain;

namespace Rafy.RBAC.GroupManagement.PermissionArchitecture
{
    /// <summary>
    /// 权限生成器的默认实现类
    /// </summary>
    public sealed class DefaultPermissionBuilder : PermissionBuilder
    {
        /// <summary>
        /// 获取当前组的所有菜单资源的权限
        /// </summary>
        /// <param name="groupID">用户组的主键</param>
        /// <returns>返回获取到的资源数组列表</returns>
        protected override IList<Resource> GenerateResourcePermission(int groupID)
        {
            ResourceList resourceList = this.GroupRepository.GetResourcePermissionByGroupID(groupID);
            IList<Resource> resource = resourceList.Concrete().ToList();
            return resource;
        }

        /// <summary>
        /// 获取当前组的所有菜单资源所对应的操作权限
        /// </summary>
        /// <param name="groupID">用户组的主键</param>
        /// <returns>返回获取到的当前组每个资源所对应的操作权限的字典集合</returns>
        protected override IDictionary<long, IList<ResourceOperation>> GenerateOperationPermission(int groupID)
        {
            IDictionary<long, IList<ResourceOperation>> operations = new Dictionary<long, IList<ResourceOperation>>();
            List<ResourceOperation> resourceOperationList = this.GroupRepository.GetResourceOperationPermissionByGroupID(groupID).Concrete().ToList();
            foreach(var item in resourceOperationList)
            {
                IList<ResourceOperation> list = null;
                if(operations.TryGetValue(item.ResourceId,out list))
                {
                    list.Add(item);
                }
                else
                {
                    list = new List<ResourceOperation>();
                    list.Add(item);
                    operations.Add(item.ResourceId, list);
                }
            }
            return operations;
        }

        /// <summary>
        /// 初始化用户的详情信息
        /// </summary>
        /// <returns>返回初始化后的用户实例对象</returns>
        protected override User InitUser()
        {
            return AccountContext.CurrentUser;
        }

        /// <summary>
        /// 初始化当前用户的组信息
        /// </summary>
        /// <param name="groupID">当前组的主键</param>
        /// <returns>返回初始化后的组的实例对象</returns>
        protected override Group InitGroup(int groupID)
        {
            return this.GroupRepository.GetById(groupID);
        }
    }
}