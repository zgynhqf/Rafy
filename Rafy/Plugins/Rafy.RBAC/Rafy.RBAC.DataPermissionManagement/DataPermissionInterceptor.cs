/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161214 15:26
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using Rafy.Accounts;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.RBAC.RoleManagement;

namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 数据权限过滤拦截器
    /// 默认关闭过滤
    /// 开启过滤,需要设置上下文信息如下
    /// using (DataPermissionInterceptor.FilterEnabled.UseScopeValue(true))
    /// {
    ///    dp.GetBy(xxxxx);
    /// }
    /// </summary>
    internal class DataPermissionInterceptor
    {
        internal static readonly AppContextItem<bool> FilterEnabled =
            new AppContextItem<bool>("Rafy.RBAC.DataPermissionManagement.DataPermissionInterceptor.FilterEnabled", false);

        /// <summary>
        /// 注册拦截器
        /// </summary>
        public static void Intercept()
        {
            RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;
        }

        /// <summary>
        /// 数据权限过滤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {
            if (FilterEnabled.Value)
            {
                var currentUser = AccountContext.CurrentUser;

                var userRoleFilder = RafyEnvironment.ObjectContainer.Resolve<IUserRoleFinder>();
                var roles = userRoleFilder.FindByUser(currentUser);

                var dataPermissions = CollectDataPermissions(roles);

                var appender = new DataPermissionWhereAppender();

                foreach (var dataPermission in dataPermissions)
                {
                    var constraintBuilder = dataPermission.CreateBuilder();

                    appender.ConstrainsBuilders.Add(constraintBuilder);
                }

                appender.Append(e.Args.Query);

                //var dp = sender as RepositoryDataProvider;
                //var resource = FindDataPermissionResource(dp.Repository.EntityType.FullName);
                //if (resource != null && resource.GetIsSupportDataPermission())
                //{
                //    long currentUserId = 1000;
                //    var roleIdList = GetRoleIdListByUserId(currentUserId);
                //    var dataPermissionList = GetDataPermission(resource.Id, roleIdList);
                //    List<DataPermissionFilterMode> modeList = new List<DataPermissionFilterMode>();
                //    foreach (DataPermission item in dataPermissionList)
                //    {
                //        if (item.Mode == DataPermissionFilterMode.Custom || !modeList.Contains(item.Mode))
                //        {
                //            var whereAppender = new DataPermissionWhereAppender(item.Mode, GetCurrentAndLowerGroup);
                //            whereAppender.CurrentUser = null;
                //            whereAppender.CurrentGroup = null;
                //            whereAppender.Append(e.Args.Query);
                //            modeList.Add(item.Mode);
                //        }
                //    }
                //}
            }
        }

        private static IList<DataPermission> CollectDataPermissions(RoleList roles)
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// 获取当前组织及其下级
        ///// </summary>
        ///// <param name="group"></param>
        ///// <returns></returns>
        //protected virtual List<long> GetCurrentAndLowerGroup(Group group)
        //{
        //    if (group != null)
        //    {
        //        var q = new CommonQueryCriteria {
        //            new PropertyMatch(Group.TreeIndexProperty, PropertyOperator.Like, group.TreeIndex)
        //        };
        //        return RepositoryFacade.ResolveInstance<GroupRepository>().GetBy(q).Select(p => p.Id).Cast<long>().ToList();
        //    }
        //    return new List<long>();
        //}
        ///// <summary>
        ///// 获取资源角色对应的数据权限列表
        ///// </summary>
        ///// <param name="resourceId">资源Id</param>
        ///// <param name="roleIdList">角色Id列表</param>
        ///// <returns></returns>
        //protected virtual DataPermissionList GetDataPermission(long resourceId, List<long> roleIdList)
        //{
        //    return RepositoryFacade.ResolveInstance<DataPermissionRepository>()
        //        .GetDataPermissionList(resourceId, roleIdList);
        //}

        ///// <summary>
        ///// 获取用户的角色列表
        ///// </summary>
        ///// <param name="userId">用户Id</param>
        ///// <returns></returns>
        //protected virtual List<long> GetRoleIdListByUserId(long userId)
        //{
        //    var userRoleDc = DomainControllerFactory.Create<UserRoleController>();
        //    return userRoleDc.GetRoleList(userId).Select(p => p.Id).Cast<long>().ToList();
        //}

        ///// <summary>
        ///// 查找资源
        ///// </summary>
        ///// <param name="fullName">资源实体全名称</param>
        ///// <returns></returns>
        //protected virtual Resource FindDataPermissionResource(string fullName)
        //{
        //    var q = new CommonQueryCriteria {
        //        new PropertyMatch(ResourceExtension.ResourceEntityTypeProperty, PropertyOperator.Equal, fullName)
        //        };
        //    return RepositoryFacade.ResolveInstance<ResourceRepository>().GetFirstBy(q);
        //}
    }
}
