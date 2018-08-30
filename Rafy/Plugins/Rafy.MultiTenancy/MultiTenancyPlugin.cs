/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160406 11:22
 * 
*******************************************************/

using Rafy.MultiTenancy.Core.DataInterception;
using Rafy.ComponentModel;

namespace Rafy.MultiTenancy
{
    /// <summary>
    /// 多租户插件。使用此插件后，可以为指定的实体类型启用多租户。实体启用多租户后，会产生以下效果：
    /// <para>1.该实体自动添加一个 TenantId 属性。</para>
    /// <para>2.TenantId 属性自动映射到数据表中的 TenantId 列。</para>
    /// <para>3.该实体在插入持久层时，会自动设置 TenantId 的属性。</para>
    /// <para>4.在所有非手工编写的 SQL 查询时，会自动添加 TenantId = XXX 的条件。</para>
    /// </summary>
    public class MultiTenancyPlugin : DomainPlugin
    {
        /// <summary>
        /// 多租户的配置。
        /// </summary>
        public static readonly MultiTenancyConfiguration Configuration = new MultiTenancyConfiguration();

        public override void Initialize(IApp app)
        {
            //拦截实体的插入和查询，自动处理多租户的属性。
            TenantDataInterceptor.Listen();
        }
    }
}
