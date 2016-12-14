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
using Rafy.Domain;
using Rafy.Domain.ORM.Query;

namespace Rafy.RBAC.RoleManagement
{
    /// <summary>
    /// 数据权限拦截过滤器
    /// </summary>
    public class DataPermissionInterceptor
    {
        public void Intercept()
        {
            RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;
        }

        protected virtual void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {

        }
        public static readonly AppContextItem<bool> FilterEnabled =
          new AppContextItem<bool>("Rafy.RBAC.RoleManagement.DataPermission.FilterEnabled", true);
        /// <summary>
        /// 注册数据权限过滤拦截器
        /// 默认开启过滤
        ///取消过滤,需要设置上下文信息如下
        /// using (DataPermissionInterceptor.FilterEnabled.UseScopeValue(false))
        //  {
        //     dp.GetBy(xxxxx);
        //  }
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="e"></param>
        /// <param name="dataPermissionResourceFilte"></param>
        /// <param name="appender"></param>
        public static void RegisterRepositoryQuerying(RepositoryDataProvider dp, QueryingEventArgs e, Func<string, Resource> dataPermissionResourceFilte, MainTableWhereAppender appender)
        {
            if (FilterEnabled.Value)
            {
                var resource = dataPermissionResourceFilte(dp.Repository.EntityType.FullName);
                if (resource != null && resource.IsSupportDataPermission)
                {
                    appender.Append(e.Args.Query);
                }
            }
        }
    }
}
