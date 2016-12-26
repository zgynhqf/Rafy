/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161214 09:21
 * 
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using Rafy.Domain;

namespace Rafy.RBAC.RoleManagement.Controllers
{
    /// <summary>
    /// 角色领域控制器
    /// </summary>
    public class RoleController : DomainController
    {
        /// <summary>
        /// 保存角色分配功能操作
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="operationIdList">操作Id集合</param>
        public virtual void SetRoleOperation(long roleId, List<long> operationIdList)
        {
            RoleOperationRepository roleOperationRepository = RepositoryFacade.ResolveInstance<RoleOperationRepository>();
            var roleOperationList = roleOperationRepository.GetByRoleId(roleId).Concrete().ToList();
            var changeRoleOpertaionList = roleOperationRepository.NewList();
            //处理删除的操作
            foreach (var item in roleOperationList)
            {
                if (operationIdList.All(id => id != item.OperationId))
                {
                    changeRoleOpertaionList.Add(item);
                    item.PersistenceStatus = PersistenceStatus.Deleted;
                }
            }
            var addRole = new Role { Id = roleId };
            //处理新增操作
            foreach (var item in operationIdList)
            {
                if (roleOperationList.All(o => o.OperationId != item))
                {
                    RoleOperation roleOpertaion = new RoleOperation();
                    roleOpertaion.Role = addRole;
                    roleOpertaion.Operation = new ResourceOperation() { Id = item };
                    roleOpertaion.PersistenceStatus = PersistenceStatus.New;
                    changeRoleOpertaionList.Add(roleOpertaion);
                }
            }
            if (changeRoleOpertaionList.Count > 0)
            {
                roleOperationRepository.Save(changeRoleOpertaionList);
            }
        }

    }
}
