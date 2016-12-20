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
    public class RoleController : DomainController
    {
        /// <summary>
        /// 保存角色分配功能操作
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <param name="operationIdList">操作Id集合</param>
        public virtual void SaveRoleOperation(long roleId, List<long> operationIdList)
        {
            RoleOperationRepository roleOperationRepository = RepositoryFacade.ResolveInstance<RoleOperationRepository>();
            var roleOperationList = roleOperationRepository.GetByRoleId(roleId).Concrete().ToList();
            var changeRoleOpertaionList = roleOperationRepository.NewList();
            //处理删除的操作
            foreach (var item in roleOperationList)
            {
                if (operationIdList.All(id => id != item.OperationId))
                {
                    item.PersistenceStatus = PersistenceStatus.Deleted;
                    changeRoleOpertaionList.Add(item);
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

        /// <summary>
        /// 获取指定角色的操作列表
        /// </summary>
        /// <param name="roleId">角色Id</param>
        /// <returns></returns>
        public virtual ResourceOperationList GetOperationByRole(long roleId)
        {
            RoleOperationRepository roleOperationRepository = RepositoryFacade.ResolveInstance<RoleOperationRepository>();
            var q = new CommonQueryCriteria();
            q.Add(new PropertyMatch(RoleOperation.RoleIdProperty, PropertyOperator.Equal, roleId));
            q.EagerLoad = new EagerLoadOptions().LoadWith(RoleOperation.OperationProperty);
           var roleOperationList= roleOperationRepository.GetBy(q).Concrete().ToList();
            var resourceOperationList = new ResourceOperationList();
            foreach (var item in roleOperationList)
            {
                resourceOperationList.Add(item.Operation);
            }
            return resourceOperationList;
        }
    }
}
