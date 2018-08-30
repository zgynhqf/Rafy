/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141222
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141222 16:23
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.MetaModel;

namespace Rafy.MultiTenancy.Core
{
    /// <summary>
    /// 对所有启用多租户的实体配置 TenantId 映射到数据表中的列。
    /// </summary>
    class TenantAwareEntityConfig : EntityConfig<Entity>
    {
        protected override void ConfigMeta()
        {
            if (MultiTenancyPlugin.Configuration.IsMultiTenancyEnabled(Meta.EntityType))
            {
                Meta.SetIsMultiTenancyEnabled(true);

                //强制映射多租户属性到数据库中。
                Meta.Property(TenantAwareEntityExtension.TenantIdProperty).MapColumn()
                    .HasLength("30");
            }
        }
    }
}