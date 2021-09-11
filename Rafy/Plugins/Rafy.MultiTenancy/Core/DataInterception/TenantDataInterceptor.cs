/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141223
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141223 10:41
 * 
*******************************************************/

using Rafy.Domain;

namespace Rafy.MultiTenancy.Core.DataInterception
{
    /// <summary>
    /// 拦截实体的插入和查询，自动处理多租户的属性。
    /// </summary>
    internal static class TenantDataInterceptor
    {
        public static void Listen()
        {
            DataSaver.SubmitInterceptors.Add(typeof(TenanntSubmitInterceptor));
            RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;
        }

        private static void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {
            var dp = sender as RepositoryDataProvider;

            //如果要查询的实体是一个多租户实体，则需要添加多租户查询的信息。
            if (dp.Repository.EntityMeta.GetIsMultiTenancyEnabled())
            {
                //环境中需要提供相应的多租户信息。
                var tenantId = TenantContext.TenantId.Value;
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    //修改查询中的所有条件。
                    var modifier = new QueryModifier();
                    modifier.TenantId = tenantId;
                    modifier.Modify(e.Args.Query);
                }
            }
        }
    }
}