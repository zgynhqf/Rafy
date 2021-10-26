/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161227
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161227 10:09
 * 
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using Rafy.Domain;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 资源仓储扩展
    /// </summary>
    public class ResourceRepositoryExtension : EntityRepositoryExt<ResourceRepository>
    {
        /// <summary>
        /// 获取用户的资源列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public virtual ResourceList GetResourceListByUserId(long userId)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            var roleList = roleRepository.GetRoleByUserId(userId);
            var roleIdList = roleList.Select(r => r.Id).Cast<long>().ToList();
            List<object> resourceIdList = new List<object>();
            var resourceOperationRepository = RepositoryFacade.ResolveInstance<ResourceOperationRepository>();
            var resourceOperationList = resourceOperationRepository.GetOperationByRoleList(roleIdList);
            foreach (var item in resourceOperationList)
            {
                if (!resourceIdList.Contains(item.ResourceId))
                {
                    resourceIdList.Add(item.ResourceId);
                }
            }
            return RepositoryFacade.ResolveInstance<ResourceRepository>().GetByIdList(resourceIdList.ToArray());
        }
    }

    /// <summary>
    /// 资源操作仓储扩展
    /// </summary>
    public static class ResourceRepositoryExtensionHelper
    {
        /// <summary>
        /// 获取用户的资源列表
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public static ResourceList GetResourceListByUserId(this ResourceOperationRepository repo, long userId)
        {
            return repo.Extension<ResourceRepositoryExtension>().GetResourceListByUserId(userId);
        }
    }
}
