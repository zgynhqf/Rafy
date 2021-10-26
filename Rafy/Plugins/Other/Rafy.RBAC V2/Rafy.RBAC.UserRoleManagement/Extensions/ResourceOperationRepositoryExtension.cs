/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161227
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161227 10:32
 * 
*******************************************************/

using System.Linq;
using Rafy.Domain;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 资源操作仓储扩展
    /// </summary>
    public class ResourceOperationRepositoryExtension : EntityRepositoryExt<ResourceOperationRepository>
    {
        /// <summary>
        /// 获取指定用户、资源的操作列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <param name="resourceId">资源Id</param>
        /// <returns></returns>
        public virtual ResourceOperationList GetResourceOperation(long userId, long resourceId)
        {
            RoleRepository roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            var roleList = roleRepository.GetRoleByUserId(userId);
            var roleIdList = roleList.Select(r => r.Id).Cast<long>().ToList();
            var operationIdList = RepositoryFacade.ResolveInstance<ResourceOperationRepository>().GetOperationByRoleList(roleIdList).Select(o => (long)o.Id);
            var resourceOperationRepository = RepositoryFacade.ResolveInstance<ResourceOperationRepository>();
            var resourceOperationList = resourceOperationRepository.GetByParentId(resourceId);
            var newOperationList = resourceOperationRepository.NewList();
            var idList = operationIdList as long[] ?? operationIdList.ToArray();
            foreach (var item in resourceOperationList)
            {
                if (idList.Any(id => id == item.Id))
                {
                    newOperationList.Add(item);
                }
            }
            return newOperationList;
        }
    }

    /// <summary>
    /// 资源操作仓储扩展
    /// </summary>
    public static class ResourceOperationRepositoryExtensionHelper
    {
        /// <summary>
        /// 获取指定用户、资源的操作列表
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="userId">用户Id</param>
        /// <param name="resourceId">资源Id</param>
        /// <returns></returns>
        public static ResourceOperationList GetResourceOperation(this ResourceOperationRepository repo, long userId, long resourceId)
        {
            return repo.Extension<ResourceOperationRepositoryExtension>().GetResourceOperation(userId, resourceId);
        }
    }
}
